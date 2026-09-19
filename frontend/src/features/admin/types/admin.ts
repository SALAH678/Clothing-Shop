import type { PaginatedList } from "../../products/types/Product";

export type { PaginatedList };

export type AdminTab = "overview" | "users" | "products" | "categories" | "purchases";

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