import { apiClient } from "../../../lib/apiClient";
import type { Cart } from "../types/Cart";

export async function getCart(): Promise<Cart> {
  const response = await apiClient.get<Cart>("/carts");
  return response.data;
}

/**
 * POST /api/carts/items/{variantId}
 *
 * The backend upserts: it adds the variant, or INCREASES the existing line by
 * `quantity` when that variant is already in the cart ("...or increases its
 * quantity" per the endpoint summary). It always responds with the full
 * `CartDto`, so callers should replace cart state with this server truth
 * instead of splicing a single item locally.
 */
export async function addCartItem(variantId: string, quantity: number): Promise<Cart> {
  const response = await apiClient.post<Cart>(`/carts/items/${variantId}`, { quantity });
  return response.data;
}

export async function removeCartItem(cartItemId: string): Promise<void> {
  await apiClient.delete(`/carts/items/${cartItemId}`);
}

/**
 * Set an absolute quantity for a cart line.
 *
 * The API exposes no PATCH/PUT for quantities (`Cart.UpdateItemQuantity` exists
 * in the domain but is not mapped to an endpoint), so we remove the line and add
 * it back with the desired quantity. Both helpers return the fresh server cart.
 * Use this only when *decreasing*; increases can call `addCartItem` with a delta
 * and save a round-trip.
 */
export async function replaceCartItemQuantity(
  cartItemId: string,
  variantId: string,
  quantity: number,
): Promise<Cart> {
  await removeCartItem(cartItemId);

  if (quantity <= 0) return getCart();

  return addCartItem(variantId, quantity);
}
