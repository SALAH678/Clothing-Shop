import type { PaginatedList } from "../../products/types/Product";

export type { PaginatedList };

/**
 * Canonical admin tabs. This array is the single source of truth for the
 * `/admin/:tab` route segment, the sidebar links and the `AdminTab` union,
 * so adding a tab in one place keeps routing and navigation in sync.
 */
export const ADMIN_TABS = ["overview", "users", "products", "categories", "purchases"] as const;

export type AdminTab = (typeof ADMIN_TABS)[number];

/** Narrows the raw `:tab` route param to a known tab. */
export const isAdminTab = (value: string | undefined): value is AdminTab =>
  value !== undefined && (ADMIN_TABS as readonly string[]).includes(value);

/** Builds the canonical URL for a tab, e.g. `/admin/users`. */
export const adminTabPath = (tab: AdminTab) => `/admin/${tab}`;

/** Shape of GET /api/dashboard/overview (backend OverviewDto). */
export interface DashboardOverviewStats {
  totalPurchases: number;
  totalProducts: number;
  totalUsers: number;
  totalCategories: number;
}

/** Shape of a user in GET /api/users (backend UserDto). */
export interface AdminUser {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  phoneNumber: string;
  isEmailVerified: boolean;
}

/** Shape of a purchase in GET /api/purchases (backend PurchaseDto). */
export interface AdminPurchaseItem {
  quantity: number;
  productName: string;
  unitPrice: number;
}

export interface AdminPurchase {
  purchaseId: string;
  customerName: string;
  customerPhoneNumber: string | null;
  wilaya: string;
  city: string;
  street: string;
  purchaseItems: AdminPurchaseItem[];
  totalAmount: number;
  status: string;
  date: string;
}