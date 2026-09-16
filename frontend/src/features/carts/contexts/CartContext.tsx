import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { useLocation } from "react-router-dom";
import { useAuth } from "../../auth/hooks/useAuth";
import * as cartApi from "../api/cartApi";
import type { CartItem } from "../types/Cart";
import { CartContext } from "./cartContextValue";
import { getPrice } from "../../../lib/pricing";

export function CartProvider({ children }: { children: ReactNode }) {
  const { accessToken, isAuthenticated, isLoading: authIsLoading } = useAuth();
  const { pathname } = useLocation();
  const loadedToken = useRef<string | null>(null);
  const [items, setItems] = useState<CartItem[]>([]);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadCart = useCallback(async () => {
    // Wait for auth boot to finish; AuthProvider owns the single refresh call.
    if (authIsLoading || accessToken === undefined) return;
    if (!isAuthenticated || accessToken === null) {
      setItems([]);
      loadedToken.current = null;
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const cart = await cartApi.getCart();
      setItems(cart.items ?? []);
      loadedToken.current = accessToken;
    } catch {
      setItems([]);
      setError("Could not load your cart.");
    } finally {
      setIsLoading(false);
    }
  }, [accessToken, authIsLoading, isAuthenticated]);

  useEffect(() => {
    if (pathname.startsWith("/auth/")) return;
    if (!accessToken || loadedToken.current === accessToken) return;
    void loadCart();
  }, [accessToken, loadCart, pathname]);

  const addToCart = useCallback(async (variantId: string, quantity: number) => {
    setError(null);
    try {
      // The backend upserts: re-adding a variant increases its quantity rather
      // than rejecting it, and responds with the full cart.
      const cart = await cartApi.addCartItem(variantId, quantity);
      setItems(cart.items ?? []);
      setIsCartOpen(true);
    } catch {
      const message = "Could not add this item to your bag.";
      setError(message);
      throw new Error(message);
    }
  }, []);

  const removeFromCart = useCallback(async (cartItemId: string) => {
    setError(null);
    try {
      await cartApi.removeCartItem(cartItemId);
      setItems((currentItems) => currentItems.filter((item) => item.id !== cartItemId));
    } catch {
      setError("Could not remove this item from your cart.");
    }
  }, []);

  const updateQuantity = useCallback(
    async (cartItemId: string, delta: number) => {
      const item = items.find((currentItem) => currentItem.id === cartItemId);
      if (!item) return;

      const nextQuantity = item.quantity + delta;
      setError(null);

      try {
        // No PATCH endpoint exists for quantities, so:
        //  - increase -> POST with a delta (server increments, one round-trip)
        //  - decrease -> DELETE + POST with the absolute quantity
        const cart =
          delta > 0 && nextQuantity > 0
            ? await cartApi.addCartItem(item.variant.id, delta)
            : await cartApi.replaceCartItemQuantity(cartItemId, item.variant.id, nextQuantity);

        setItems(cart.items ?? []);
      } catch {
        setError("Could not update the quantity.");
        // Reconcile with the server instead of silently diverging from it.
        void loadCart();
      }
    },
    [items, loadCart],
  );

  // The API exposes no clear-cart endpoint, so this only empties local state.
  // After a successful checkout the cart is (re)loaded from the server anyway.
  const clearCart = useCallback(() => {
    setItems([]);
    setError(null);
  }, []);

  const value = useMemo(() => {
    const cartCount = items.reduce((sum, item) => sum + item.quantity, 0);
    const cartTotal = items.reduce((total, item) => {
      return total + getPrice(item.variant.product.basePrice, item.variant.product.discount) * item.quantity;
    }, 0);
    return {
      items,
      isLoading,
      error,
      addToCart,
      removeFromCart,
      updateQuantity,
      clearCart,
      isCartOpen,
      setIsCartOpen,
      cartCount,
      cartTotal,
    };
  }, [items, isLoading, error, addToCart, removeFromCart, updateQuantity, clearCart, isCartOpen]);

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}
