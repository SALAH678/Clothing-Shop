import { useCallback, useEffect, useRef, useState, type ReactNode } from "react";
import { useLocation } from "react-router-dom";
import { useAuth } from "../../auth/hooks/useAuth";
import * as authApi from "../../auth/api/authApi";
import * as cartApi from "../api/cartApi";
import type { CartItem } from "../types/Cart";
import { CartContext } from "./cartContextValue";

export function CartProvider({ children }: { children: ReactNode }) {
  const { accessToken, login } = useAuth();
  const { pathname } = useLocation();
  const loadedToken = useRef<string | null>(null);
  const [items, setItems] = useState<CartItem[]>([]);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getCartTotal = (cartItems: CartItem[]) =>
    cartItems.reduce((total, item) => {
      const unitPrice = item.variant.product.basePrice - item.variant.product.discount;
      return total + unitPrice * item.quantity;
    }, 0);

  const loadCart = useCallback(async () => {
    if (accessToken === undefined) {
      setItems([]);
      setIsLoading(false);
      loadedToken.current = null;
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      if (accessToken === null) {
        const refreshedSession = await authApi.refresh();
        login(refreshedSession);
        return;
      }

      const cart = await cartApi.getCart();
      setItems(cart.items ?? []);
      loadedToken.current = accessToken;
    } catch {
      setItems([]);
      setError("Could not load your cart.");
    } finally {
      setIsLoading(false);
    }
  }, [accessToken, login]);

  useEffect(() => {
    if (pathname.startsWith("/auth/")) return;
    if (accessToken && loadedToken.current === accessToken) return;

    const run = async () => {
      await loadCart();
    };
    void run();
  }, [accessToken, loadCart, pathname]);

  const addToCart = async (variantId: string, quantity: number) => {
    setError(null);
    if (items.some((item) => item.variant.id === variantId)) {
      const duplicateMessage = "This product is already in your bag.";
      setError(duplicateMessage);
      throw new Error(duplicateMessage);
    }

    const addedItem = await cartApi.addCartItem(variantId, quantity);
    setItems((currentItems) => {
      const existingItem = currentItems.find((item) => item.variant.id === addedItem.variant.id);
      return existingItem
        ? currentItems.map((item) => (item.id === existingItem.id ? addedItem : item))
        : [...currentItems, addedItem];
    });
    setIsCartOpen(true);
  };

  const removeFromCart = async (cartItemId: string) => {
    setError(null);
    try {
      await cartApi.removeCartItem(cartItemId);
      setItems((currentItems) => currentItems.filter((item) => item.id !== cartItemId));
    } catch {
      setError("Could not remove this item from your cart.");
    }
  };

  const updateQuantity = (cartItemId: string, delta: number) => {
    setItems((currentItems) =>
      currentItems.map((item) =>
        item.id === cartItemId
          ? { ...item, quantity: Math.max(1, Math.min(item.variant.stockQuantity, item.quantity + delta)) }
          : item,
      ),
    );
  };

  const clearCart = () => {
    setItems([]);
    setError(null);
  };

  const cartCount = items.length;
  const cartTotal = getCartTotal(items);

  return (
    <CartContext.Provider
      value={{
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
      }}
    >
      {children}
    </CartContext.Provider>
  );
}
