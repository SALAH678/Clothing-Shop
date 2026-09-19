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

/** GET /api/products — paginated products with no filters and no category. */
export async function getAdminProducts(
  pageNumber = 1,
  pageSize = ADMIN_PAGE_SIZE
): Promise<PaginatedList<Product>> {
  const response = await apiClient.get<PaginatedList<Product>>("/products", {
    params: { pageNumber, pageSize },
  });
  return response.data;
}