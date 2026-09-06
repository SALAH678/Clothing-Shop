import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { getProducts } from "../api/productApi";
import type { GetProductsParams } from "../types/GetProductsParams";

export function useProducts(params: GetProductsParams, enabled = true) {
  return useQuery({
    queryKey: ["products", params],
    queryFn: () => getProducts(params),
    enabled,
    placeholderData: keepPreviousData,
    staleTime: 0,
    refetchOnMount: "always",
    retry: 2,
  });
}
