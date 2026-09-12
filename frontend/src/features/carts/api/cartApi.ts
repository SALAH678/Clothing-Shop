import { apiClient } from "../../../lib/apiClient";
import type { Cart, CartItem } from "../types/Cart";

export async function getCart(): Promise<Cart> {
  const response = await apiClient.get<Cart>("/carts");
  return response.data;
}

export async function addCartItem(variantId: string, quantity: number): Promise<CartItem> {
  const response = await apiClient.post<CartItem | { items?: CartItem[] }>(`/carts/items/${variantId}`, { quantity });
  const data = response.data;

  if (data && "variant" in data && data.variant) {
    return data as CartItem;
  }

  if (data && "items" in data) {
    const cartItem = data.items?.find((item) => item.variant?.id === variantId);
    if (cartItem) {
      return cartItem;
    }
  }

  throw new Error("The add-to-cart response did not include a cart item.");
}

export async function removeCartItem(cartItemId: string): Promise<void> {
  await apiClient.delete(`/carts/items/${cartItemId}`);
}
