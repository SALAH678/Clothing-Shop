import { apiClient } from "../../../lib/apiClient";
import type { Category } from "../types/Category";

export async function getCategories(): Promise<Category[]> {
  const response = await apiClient.get<Category[]>("/categories");
  return response.data;
}

/** POST /api/categories — multipart form: CategoryName + optional Image file (Admin only). */
export async function createCategory(categoryName: string, image?: File): Promise<Category> {
  const formData = new FormData();
  formData.append("CategoryName", categoryName);
  if (image) {
    formData.append("Image", image);
  }
  const response = await apiClient.post<Category>("/categories", formData);
  return response.data;
}

/** PUT /api/categories/{id} — multipart form: optional CategoryName + optional Image file (Admin only). */
export async function updateCategory(
  categoryId: string,
  categoryName?: string,
  image?: File
): Promise<Category> {
  const formData = new FormData();
  if (categoryName) {
    formData.append("CategoryName", categoryName);
  }
  if (image) {
    formData.append("Image", image);
  }
  const response = await apiClient.put<Category>(`/categories/${categoryId}`, formData);
  return response.data;
}

/** DELETE /api/categories/{id} (Admin only). */
export async function deleteCategory(categoryId: string): Promise<void> {
  await apiClient.delete(`/categories/${categoryId}`);
}

/** PUT /api/categories/{id}/subcategories — assign subcategories to a parent (Admin only). */
export async function assignSubCategories(
  categoryId: string,
  subCategoryIds: string[]
): Promise<void> {
  await apiClient.put(`/categories/${categoryId}/subcategories`, {
    categoryId,
    subCategoryIds,
  });
}

/** DELETE /api/categories/{id}/subcategories — unassign subcategories from a parent (Admin only). */
export async function unassignSubCategories(
  categoryId: string,
  subCategoryIds: string[]
): Promise<void> {
  await apiClient.delete(`/categories/${categoryId}/subcategories`, {
    data: { categoryId, subCategoryIds },
  });
}
