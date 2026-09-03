import { useQuery } from "@tanstack/react-query";
import { getCategories } from "../api/categoryApi";

export function useCategories() {
  return useQuery({
    queryKey: ["categories"],
    queryFn: getCategories,
    staleTime: 1000 * 60 * 15, // 15 minutes
    retry: 2, // Retry two times on failure
  });
}
