export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  Role: "Admin" | "Customer";
  isEmailVerified: boolean;
}

export interface LoginResponse {
  user: User;
  accessToken: string;
}
