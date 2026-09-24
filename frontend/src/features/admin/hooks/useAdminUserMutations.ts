import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createUser, type CreateAdminUserInput } from "../api/adminApi";
import type { AdminUser } from "../types/admin";

const ADMIN_USERS_QUERY_KEY = ["admin", "users"];

function useInvalidateAdminUsers() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: ADMIN_USERS_QUERY_KEY });
}

/** POST /api/users — create a user (Admin only). Invalidates the users list. */
export function useCreateUser() {
  const invalidateUsers = useInvalidateAdminUsers();
  return useMutation<AdminUser, unknown, CreateAdminUserInput>({
    mutationFn: (input) => createUser(input),
    onSuccess: invalidateUsers,
  });
}
