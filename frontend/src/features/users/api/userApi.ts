import { apiClient } from "../../../lib/apiClient";
import type { User } from "../../auth/types/LoginResponse";

export interface UpdateUserProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export async function getCurrentUserProfile(): Promise<User> {
  const response = await apiClient.get<User>("/users/current");
  return response.data;
}

export async function updateCurrentUserProfile(
  data: UpdateUserProfileRequest
): Promise<User> {
  const response = await apiClient.put<User>("/users/current", data);
  return response.data;
}
