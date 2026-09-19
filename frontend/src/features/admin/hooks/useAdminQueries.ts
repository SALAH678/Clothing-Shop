import { useQuery } from "@tanstack/react-query";
import {
  ADMIN_PAGE_SIZE,
  getAdminProducts,
  getDashboardOverview,
  getPurchases,
  getUsers,
} from "../api/adminApi";

// Admin dashboards should feel live; a short stale time keeps numbers fresh
// without hammering the API on every tab switch.
const STALE_MS = 30 * 1000;

/** GET /api/dashboard/overview */
export function useDashboardOverview(enabled = true) {
  return useQuery({
    queryKey: ["admin", "overview"],
    queryFn: getDashboardOverview,
    enabled,
    staleTime: STALE_MS,
  });
}

/** GET /api/users — page size is always ADMIN_PAGE_SIZE (10). */
export function useAdminUsers(pageNumber: number, enabled = true) {
  return useQuery({
    queryKey: ["admin", "users", pageNumber],
    queryFn: () => getUsers(pageNumber, ADMIN_PAGE_SIZE),
    enabled,
    staleTime: STALE_MS,
    placeholderData: (previous) => previous, // keep old rows while flipping pages
  });
}

/** GET /api/purchases — page size is always ADMIN_PAGE_SIZE (10). */
export function useAdminPurchases(pageNumber: number, enabled = true) {
  return useQuery({
    queryKey: ["admin", "purchases", pageNumber],
    queryFn: () => getPurchases(pageNumber, ADMIN_PAGE_SIZE),
    enabled,
    staleTime: STALE_MS,
    placeholderData: (previous) => previous,
  });
}

/** GET /api/products — paginated with no filters and no category. */
export function useAdminProducts(pageNumber: number, enabled = true) {
  return useQuery({
    queryKey: ["admin", "products", pageNumber],
    queryFn: () => getAdminProducts(pageNumber, ADMIN_PAGE_SIZE),
    enabled,
    staleTime: STALE_MS,
    placeholderData: (previous) => previous,
  });
}