import { apiClient } from "../../../lib/apiClient";
import type { Variant } from "../../products/types/Product";

/** POST /api/products/{productId}/variants — create a variant (Admin only). */
export async function createVariant(
  productId: string,
  size: string,
  color: string,
  stockQuantity: number
): Promise<Variant> {
  const response = await apiClient.post<Variant>(`/products/${productId}/variants`, {
    productId,
    size,
    color,
    stockQuantity,
  });
  return response.data;
}

/** PUT /api/products/variants/{variantId} — partial update of size/color/stock (Admin only). */
export async function updateVariant(
  variantId: string,
  changes: { size?: string; color?: string; stockQuantity?: number }
): Promise<Variant> {
  const response = await apiClient.put<Variant>(`/products/variants/${variantId}`, changes);
  return response.data;
}

/** DELETE /api/products/variants/{variantId} (Admin only). */
export async function deleteVariant(variantId: string): Promise<void> {
  await apiClient.delete(`/products/variants/${variantId}`);
}