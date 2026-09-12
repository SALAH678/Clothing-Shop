import { useQuery } from "@tanstack/react-query";
import { getProduct } from "../api/productApi";

export function useProduct(productId: string | undefined) {
  return useQuery({
    queryKey: ["product", productId],
    queryFn: () => getProduct(productId as string),
    enabled: Boolean(productId),
    retry: 2,
  });
}
