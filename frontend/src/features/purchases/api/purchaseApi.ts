import { apiClient } from "../../../lib/apiClient";

export interface PurchaseItem {
  variantId: string;
  quantity: number;
}

export interface CreatePurchaseRequest {
  customerPhone: string;
  street: string;
  city: string;
  wilaya: string;
  origin: "BuyNow" | "Cart";
  purchaseItems: PurchaseItem[];
}

export interface CreatePurchaseResult {
  purchaseId: string;
  checkoutUrl: string;
}

export interface RetryPurchaseResult {
  checkoutUrl: string;
}

export async function createPurchase(
  payload: CreatePurchaseRequest
): Promise<CreatePurchaseResult> {
  const response = await apiClient.post<CreatePurchaseResult>("/purchases", payload);
  return response.data;
}

export async function retryPurchase(
  purchaseId: string
): Promise<RetryPurchaseResult> {
  const response = await apiClient.post<RetryPurchaseResult>(`/purchases/retry/${purchaseId}`);
  return response.data;
}
