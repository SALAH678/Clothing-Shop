import { useInfiniteQuery } from "@tanstack/react-query";
import { getProducts } from "../api/productApi";
import type { GetProductsParams } from "../types/GetProductsParams";

export type InfiniteProductsParams = Omit<GetProductsParams, "pageNumber" | "pageSize"> & {
  pageSize?: number;
};

const MAX_PAGES = 5; // 5 x 12 = 60 items cap, then ask user to refine filters

export function useInfiniteProducts(params: InfiniteProductsParams, enabled = true) {
  const { pageSize = 12, ...filters } = params;
  return useInfiniteQuery({
    queryKey: ["products", "infinite", { ...filters, pageSize }],
    queryFn: ({ pageParam = 1 }) => getProducts({ ...filters, pageNumber: pageParam, pageSize }),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => {
      if (lastPage.pageNumber >= lastPage.totalPages) return undefined;
      if (lastPage.pageNumber >= MAX_PAGES) return undefined;
      return lastPage.pageNumber + 1;
    },
    enabled,
    placeholderData: (previous) => previous,
  });
}

export { MAX_PAGES };
