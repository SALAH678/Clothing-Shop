import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createProduct,
  deleteProduct,
  updateProduct,
  type CreateProductInput,
  type ProductInput,
} from "../api/adminProductApi";

const ADMIN_PRODUCTS_QUERY_KEY = ["admin", "products"];

function useInvalidateAdminProducts() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: ADMIN_PRODUCTS_QUERY_KEY });
}

export function useCreateProduct() {
  const invalidateProducts = useInvalidateAdminProducts();
  return useMutation({
    mutationFn: (input: CreateProductInput) => createProduct(input),
    onSuccess: invalidateProducts,
  });
}

export function useUpdateProduct() {
  const invalidateProducts = useInvalidateAdminProducts();
  return useMutation({
    mutationFn: ({ productId, input }: { productId: string; input: ProductInput }) => updateProduct(productId, input),
    onSuccess: invalidateProducts,
  });
}

export function useDeleteProduct() {
  const invalidateProducts = useInvalidateAdminProducts();
  return useMutation({
    mutationFn: ({ productId }: { productId: string }) => deleteProduct(productId),
    onSuccess: invalidateProducts,
  });
}