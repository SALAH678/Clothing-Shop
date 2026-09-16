import { createContext } from "react";
import type { CartItem } from "../types/Cart";

export interface CartContextType {
  items: CartItem[];
  isLoading: boolean;
  error: string | null;
  addToCart: (variantId: string, quantity: number) => Promise<void>;
  removeFromCart: (cartItemId: string) => Promise<void>;
  updateQuantity: (cartItemId: string, delta: number) => Promise<void>;
  clearCart: () => void;
  isCartOpen: boolean;
  setIsCartOpen: (isOpen: boolean) => void;
  cartCount: number;
  cartTotal: number;
}

export const CartContext = createContext<CartContextType | undefined>(undefined);
