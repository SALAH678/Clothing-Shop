import { apiClient } from "../../../lib/apiClient";
import type { Product } from "../../products/types/Product";

export interface ProductInput {
  name: string;
  description?: string;
  basePrice: number;
  discount?: number;
  categoryId: string;
}

export interface CreateVariantInput {
  size: string;
  color: string;
  stockQuantity: number;
}

export interface CreateProductInput {
  name: string;
  description?: string;
  basePrice: number;
  discount?: number;
  categoryId: string;
  variants: CreateVariantInput[];
  images: File[];
  mainImageIndex: number;
}

/** POST /api/products — create a product with images and variants (Admin only, multipart/form-data). */
export async function createProduct(input: CreateProductInput): Promise<Product> {
  const formData = new FormData();
  formData.append("name", input.name);
  if (input.description) formData.append("description", input.description);
  formData.append("basePrice", String(input.basePrice));
  if (input.discount !== undefined) formData.append("discount", String(input.discount));
  formData.append("categoryId", input.categoryId);
  formData.append(
    "variantsJson",
    JSON.stringify(
      input.variants.map((variant) => ({
        Size: variant.size,
        Color: variant.color,
        StockQuantity: variant.stockQuantity,
      })),
    ),
  );
  input.images.forEach((file) => formData.append("images", file));
  formData.append("mainImageIndex", String(input.mainImageIndex));

  // Do not set Content-Type manually — the browser/axios needs to generate
  // the multipart boundary itself. If apiClient has a default JSON
  // Content-Type header, it must not apply to this request.
  const response = await apiClient.post<Product>("/products", formData);
  return response.data;
}

/** PUT /api/products/{productId} — update a product (Admin only). */
export async function updateProduct(productId: string, input: ProductInput): Promise<Product> {
  const response = await apiClient.put<Product>(`/products/${productId}`, input);
  return response.data;
}

/** DELETE /api/products/{productId} — delete a product (Admin only). */
export async function deleteProduct(productId: string): Promise<void> {
  await apiClient.delete(`/products/${productId}`);
}