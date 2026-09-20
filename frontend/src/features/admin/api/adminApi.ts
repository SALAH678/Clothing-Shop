import { apiClient } from "../../../lib/apiClient";
import type { PaginatedList, Product } from "../../products/types/Product";
import type { AdminPurchase, AdminUser, DashboardOverviewStats } from "../types/admin";

/** Fixed page size for every admin paginated table. */
export const ADMIN_PAGE_SIZE = 10;

/** GET /api/dashboard/overview — aggregated dashboard statistics (Admin only). */
export async function getDashboardOverview(): Promise<DashboardOverviewStats> {
  const response = await apiClient.get<DashboardOverviewStats>("/dashboard/overview");
  return response.data;
}

/** GET /api/users — paginated list of all users (Admin only). */
export async function getUsers(
  pageNumber = 1,
  pageSize = ADMIN_PAGE_SIZE
): Promise<PaginatedList<AdminUser>> {
  const response = await apiClient.get<PaginatedList<AdminUser>>("/users", {
    params: { pageNumber, pageSize },
  });
  return response.data;
}

/** GET /api/purchases — paginated list of all purchases (Admin only). */
export async function getPurchases(
  pageNumber = 1,
  pageSize = ADMIN_PAGE_SIZE
): Promise<PaginatedList<AdminPurchase>> {
  const response = await apiClient.get<PaginatedList<AdminPurchase>>("/purchases", {
    params: { pageNumber, pageSize },
  });
  return response.data;
}

/** Optional filters for the admin products table (server-side supported). */
export interface AdminProductFilters {
  /** Case-insensitive match on the product name. */
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  /** Only "price" is supported by the API; omit for date (newest/oldest) ordering. */
  sortBy?: string;
  /** With sortBy: false = ascending, true = descending. Without sortBy: true = newest first, false = oldest first. */
  descending: boolean;
}

/** GET /api/products — paginated products with optional search / price / sort filters. */
export async function getAdminProducts(
  pageNumber = 1,
  pageSize = ADMIN_PAGE_SIZE,
  filters: AdminProductFilters = { descending: true }
): Promise<PaginatedList<Product>> {
  const response = await apiClient.get<PaginatedList<Product>>("/products", {
    // Undefined values are dropped by axios, so empty filters send nothing extra.
    params: {
      pageNumber,
      pageSize,
      search: filters.search,
      minPrice: filters.minPrice,
      maxPrice: filters.maxPrice,
      sortBy: filters.sortBy,
      descending: filters.descending,
    },
  });
  return response.data;
}