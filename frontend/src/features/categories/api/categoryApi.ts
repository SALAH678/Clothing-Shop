import { apiClient } from "../../../lib/apiClient";
import type { Category } from "../types/Category";

export async function getCategories(): Promise<Category[]> {
  const response = await apiClient.get<Category[]>("/categories");
  return response.data;
}
