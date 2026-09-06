import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getCurrentUserProfile,
  updateCurrentUserProfile,
  type UpdateUserProfileRequest,
} from "../api/userApi";
import { useAuth } from "../../auth/hooks/useAuth";

export const USER_PROFILE_QUERY_KEY = ["currentUserProfile"];

export function useCurrentUserProfile() {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: USER_PROFILE_QUERY_KEY,
    queryFn: getCurrentUserProfile,
    enabled: !!accessToken,
  });
}

export function useUpdateCurrentUserProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateUserProfileRequest) => updateCurrentUserProfile(data),
    onSuccess: (updatedUser) => {
      queryClient.setQueryData(USER_PROFILE_QUERY_KEY, updatedUser);
    },
  });
}
