import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createVariant, deleteVariant, updateVariant } from "../api/adminVariantApi";

// Prefix of the key used by useAdminProducts, so invalidating it refreshes
// every cached admin products page (and the open variants modal) at once.
const ADMIN_PRODUCTS_QUERY_KEY = ["admin", "products"];

function useInvalidateAdminProducts() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: ADMIN_PRODUCTS_QUERY_KEY });
}

/** POST /api/products/{productId}/variants */
export function useCreateVariant() {
  const invalidateProducts = useInvalidateAdminProducts();
  return useMutation({
    mutationFn: ({
      productId,
      size,
      color,
      stockQuantity,
    }: {
      productId: string;
      size: string;
      color: string;
      stockQuantity: number;
    }) => createVariant(productId, size, color, stockQuantity),
    onSuccess: invalidateProducts,
  });
}

/** PUT /api/products/variants/{variantId} */
export function useUpdateVariant() {
  const invalidateProducts = useInvalidateAdminProducts();
  return useMutation({
    mutationFn: ({
      variantId,
      size,
      color,
      stockQuantity,
    }: {
      variantId: string;
      size?: string;
      color?: string;
      stockQuantity?: number;
    }) => updateVariant(variantId, { size, color, stockQuantity }),
    onSuccess: invalidateProducts,
  });
}

/** DELETE /api/products/variants/{variantId} */
export function useDeleteVariant() {
  const invalidateProducts = useInvalidateAdminProducts();
  return useMutation({
    mutationFn: ({ variantId }: { variantId: string }) => deleteVariant(variantId),
    onSuccess: invalidateProducts,
  });
}