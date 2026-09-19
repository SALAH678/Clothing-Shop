import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  assignSubCategories,
  createCategory,
  deleteCategory,
  unassignSubCategories,
  updateCategory,
} from "../../categories/api/categoryApi";

// Same key the storefront useCategories hook uses, so admin mutations
// automatically refresh the category list everywhere in the app.
const CATEGORIES_QUERY_KEY = ["categories"];

function useInvalidateCategories() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: CATEGORIES_QUERY_KEY });
}

export function useCreateCategory() {
  const invalidateCategories = useInvalidateCategories();
  return useMutation({
    mutationFn: ({ name, image }: { name: string; image?: File }) =>
      createCategory(name, image),
    onSuccess: invalidateCategories,
  });
}

export function useUpdateCategory() {
  const invalidateCategories = useInvalidateCategories();
  return useMutation({
    mutationFn: ({
      categoryId,
      name,
      image,
    }: {
      categoryId: string;
      name?: string;
      image?: File;
    }) => updateCategory(categoryId, name, image),
    onSuccess: invalidateCategories,
  });
}

export function useDeleteCategory() {
  const invalidateCategories = useInvalidateCategories();
  return useMutation({
    mutationFn: ({ categoryId }: { categoryId: string }) => deleteCategory(categoryId),
    onSuccess: invalidateCategories,
  });
}

export function useAssignSubCategories() {
  const invalidateCategories = useInvalidateCategories();
  return useMutation({
    mutationFn: ({
      categoryId,
      subCategoryIds,
    }: {
      categoryId: string;
      subCategoryIds: string[];
    }) => assignSubCategories(categoryId, subCategoryIds),
    onSuccess: invalidateCategories,
  });
}

export function useUnassignSubCategories() {
  const invalidateCategories = useInvalidateCategories();
  return useMutation({
    mutationFn: ({
      categoryId,
      subCategoryIds,
    }: {
      categoryId: string;
      subCategoryIds: string[];
    }) => unassignSubCategories(categoryId, subCategoryIds),
    onSuccess: invalidateCategories,
  });
}