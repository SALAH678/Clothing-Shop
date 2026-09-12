import { Link, useLocation, useNavigate } from "react-router-dom";
import { useForm, useWatch } from "react-hook-form";
import type { FieldValues } from "react-hook-form";
import { GoogleLogin, type CredentialResponse } from "@react-oauth/google";
import { useGoogleRegister, useRegister } from "../features/auth/hooks";
import type { ApiErrorResponse } from "../features/auth/types/ApiErrorResponse";

export default function Register() {
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname?: string; search?: string; hash?: string } })?.from;
  const redirectPath = from ? `${from.pathname ?? ""}${from.search ?? ""}${from.hash ?? ""}` : "/profile";

  const {
    register,
    control,
    getValues,
    setError,
    handleSubmit,
    formState: { errors },
  } = useForm({
    mode: "onChange",
  });

  const { mutate: registerUser, isPending, error: registerError } = useRegister();
  const { mutate: registerWithGoogle, isPending: isGooglePending, error: googleError } = useGoogleRegister();
  const phoneNumber = useWatch({ control, name: "phone" });

  const onSubmit = (data: FieldValues) => {
    registerUser(
      {
        firstName: data.firstName,
        lastName: data.lastName,
        phoneNumber: data.phone,
        email: data.email,
        password: data.password,
      },
      {
        onSuccess: () => {
          navigate("/auth/verify-email", { state: { email: data.email } });
        },
      },
    );
  };

  const handleGoogleSuccess = ({ credential }: CredentialResponse) => {
    const phoneNumber = getValues("phone");
    if (!phoneNumber) {
      setError("phone", { type: "required", message: "Phone number is required" });
      return;
    }

    if (!credential) {
      return;
    }

    registerWithGoogle(
      { phoneNumber, IdToken: credential },
      {
        onSuccess: () => {
          navigate(redirectPath, { replace: true });
        },
      },
    );
  };

  const handleGoogleClick = () => {
    setError("phone", { type: "required", message: "Phone number is required" });
  };

  const googleErrorMessage =
    (googleError as ApiErrorResponse)?.response?.data?.detail ||
    (googleError as ApiErrorResponse)?.response?.data?.message;

  return (
    <div className="grow flex items-center justify-center py-20 px-4 relative overflow-hidden bg-surface">
      <div className="absolute inset-0 pointer-events-none opacity-20 z-0">
        <div className="absolute top-1/4 left-1/4 w-96 h-96 border-2 border-primary transform rotate-45"></div>
        <div className="absolute bottom-1/4 right-1/4 w-64 h-64 border-2 border-primary"></div>
      </div>

      <div className="w-full max-w-125 bg-white border-4 border-primary relative z-10 shadow-[8px_8px_0_0_#000]">
        <div className="p-8 md:p-10 border-b-4 border-primary bg-primary text-white">
          <h1 className="font-display text-3xl md:text-4xl font-black tracking-tighter uppercase text-center drop-shadow-[2px_2px_0_rgba(0,0,0,1)]">
            CREATE ACCOUNT
          </h1>
        </div>
        <div className="p-8 md:p-10">
          <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <label className="font-mono text-sm font-bold uppercase block" htmlFor="firstName">
                  First Name
                </label>
                <input
                  className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${errors.firstName ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                  id="firstName"
                  placeholder="JOHN"
                  type="text"
                  {...register("firstName", {
                    required: "First name is required",
                    pattern: { value: /^[A-Za-z\s]+$/, message: "Must contain only characters and spaces" },
                  })}
                />
                {errors.firstName && (
                  <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                    {errors.firstName.message as string}
                  </span>
                )}
              </div>
              <div className="space-y-2">
                <label className="font-mono text-sm font-bold uppercase block" htmlFor="lastName">
                  Last Name
                </label>
                <input
                  className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${errors.lastName ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                  id="lastName"
                  placeholder="DOE"
                  type="text"
                  {...register("lastName", {
                    required: "Last name is required",
                    pattern: { value: /^[A-Za-z\s]+$/, message: "Must contain only characters and spaces" },
                  })}
                />
                {errors.lastName && (
                  <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                    {errors.lastName.message as string}
                  </span>
                )}
              </div>
            </div>
            <div className="space-y-2">
              <label className="font-mono text-sm font-bold uppercase block" htmlFor="phone">
                Phone Number
              </label>
              <input
                className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${errors.phone ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                id="phone"
                placeholder="+213 0555 00 00 00"
                type="tel"
                maxLength={10}
                onInput={(e) => {
                  e.currentTarget.value = e.currentTarget.value.replace(/\D/g, "");
                }}
                {...register("phone", {
                  required: "Phone number is required",
                  pattern: { value: /^\d{10}$/, message: "Must be exactly 10 digits" },
                })}
              />
              {errors.phone && (
                <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                  {errors.phone.message as string}
                </span>
              )}
            </div>
            <div className="space-y-2">
              <label className="font-mono text-sm font-bold uppercase block" htmlFor="email">
                Email
              </label>
              <input
                className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${errors.email ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                id="email"
                placeholder="HELLO@GMAIL.COM"
                type="email"
                {...register("email", {
                  required: "Email is required",
                  pattern: { value: /^[a-zA-Z0-9._%+-]+@gmail\.com$/, message: "Must be a valid @gmail.com address" },
                })}
              />
              {errors.email && (
                <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                  {errors.email.message as string}
                </span>
              )}
              <p className="font-mono text-[10px] sm:text-xs text-secondary font-bold uppercase mt-2">
                Note: You need to put a real email
              </p>
            </div>
            <div className="space-y-2">
              <label className="font-mono text-sm font-bold uppercase block" htmlFor="password">
                Password
              </label>
              <input
                className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${errors.password ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                id="password"
                placeholder="••••••••"
                type="password"
                {...register("password", {
                  required: "Password is required",
                  minLength: { value: 6, message: "Must be at least 6 characters long" },
                  pattern: {
                    value: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{6,}$/,
                    message: "Must contain at least 1 uppercase, 1 lowercase, 1 number, and 1 special character",
                  },
                })}
              />
              {errors.password && (
                <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                  {errors.password.message as string}
                </span>
              )}
              <p className="font-mono text-[10px] sm:text-xs text-secondary font-bold uppercase mt-2">
                Note: Password must be at least 6 characters long and contain at least one uppercase letter, one
                lowercase letter, one number, and one special character.
              </p>
            </div>
            {registerError && (
              <div className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase">
                {(registerError as ApiErrorResponse)?.response?.data?.detail ||
                  (registerError as ApiErrorResponse)?.response?.data?.message ||
                  "Registration failed. Please check your data."}
              </div>
            )}
            {googleError && (
              <div className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase">
                {googleErrorMessage || "Google registration failed. Please try again."}
              </div>
            )}

            <div className="pt-4 space-y-4">
              <button
                disabled={isPending}
                className="w-full bg-primary text-white border-2 border-primary py-4 font-mono text-lg uppercase font-black tracking-widest hover:-translate-y-1 hover:-translate-x-1 hover:shadow-[4px_4px_0_0_#000] hover:bg-white hover:text-black transition-all duration-200 active:translate-y-0 active:translate-x-0 active:shadow-none disabled:opacity-50 disabled:cursor-not-allowed"
                type="submit"
              >
                {isPending ? "CREATING ACCOUNT..." : "REGISTER"}
              </button>
            </div>

            <div className="relative flex items-center py-2 mt-6">
              <div className="grow border-t-2 border-primary"></div>
              <span className="shrink-0 mx-4 font-mono text-sm text-secondary font-bold uppercase tracking-widest">
                OR
              </span>
              <div className="grow border-t-2 border-primary"></div>
            </div>

            {phoneNumber ? (
              <div className={`flex justify-center ${isGooglePending ? "pointer-events-none opacity-50" : ""}`}>
                <GoogleLogin
                  onSuccess={handleGoogleSuccess}
                  onError={() => undefined}
                  text="signup_with"
                  theme="outline"
                  size="large"
                  width="400"
                  useOneTap={false}
                  auto_select={false}
                />
              </div>
            ) : (
              <button
                type="button"
                onClick={handleGoogleClick}
                className="w-full bg-surface border-2 border-primary py-4 font-mono text-sm uppercase font-bold tracking-widest flex justify-center items-center gap-3 hover:-translate-y-1 hover:-translate-x-1 hover:shadow-[4px_4px_0_0_#000] hover:bg-white transition-all duration-200 active:translate-y-0 active:translate-x-0 active:shadow-none"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                  <path
                    d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                    fill="#4285F4"
                  />
                  <path
                    d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                    fill="#34A853"
                  />
                  <path
                    d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                    fill="#FBBC05"
                  />
                  <path
                    d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                    fill="#EA4335"
                  />
                </svg>
                SIGN UP WITH GOOGLE
              </button>
            )}
          </form>
        </div>
        <div className="p-6 border-t-4 border-primary bg-surface-container-low text-center">
          <p className="font-mono text-sm text-secondary font-bold">
            ALREADY HAVE AN ACCOUNT?{" "}
            <Link
              className="text-primary underline hover:bg-primary hover:text-white px-1 transition-colors duration-200"
              to="/auth/login"
            >
              LOG IN
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
