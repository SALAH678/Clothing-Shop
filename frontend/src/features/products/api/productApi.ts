import { apiClient } from "../../../lib/apiClient";
import type { PaginatedList, Product } from "../types/Product";
import type { GetProductsParams } from "../types/GetProductsParams";

export async function getProducts(params: GetProductsParams): Promise<PaginatedList<Product>> {
  const response = await apiClient.get<PaginatedList<Product>>("/products", {
    params,
    paramsSerializer: {
      serialize: (values) => {
        const searchParams = new URLSearchParams();

        Object.entries(values).forEach(([key, value]) => {
          if (value == null) return;

          if (Array.isArray(value)) {
            value.forEach((item) => searchParams.append(key, String(item)));
            return;
          }

          searchParams.append(key, String(value));
        });

        return searchParams.toString();
      },
    },
  });

  return response.data;
}

export async function getProduct(productId: string): Promise<Product> {
  const response = await apiClient.get<Product>(`/products/${productId}`);
  return response.data;
}
