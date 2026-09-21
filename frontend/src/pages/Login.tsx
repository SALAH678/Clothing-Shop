import { Link, useNavigate, useLocation } from "react-router-dom";
import { useForm } from "react-hook-form";
import type { FieldValues } from "react-hook-form";
import { GoogleLogin, type CredentialResponse } from "@react-oauth/google";
import { useGoogleLogin, useLogin } from "../features/auth/hooks";
import type { ApiErrorResponse } from "../features/auth/types/ApiErrorResponse";
import RateLimitNotice from "../components/ui/RateLimitNotice";
import { useRateLimit } from "../lib/rateLimit";
import { gmailRules, passwordRules } from "../lib/validation";

export default function Login() {
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname?: string } })?.from?.pathname || "/profile";

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    mode: "onChange",
  });

  const { mutate: loginUser, isPending, error: loginError } = useLogin();
  const { mutate: googleLogin, isPending: isGooglePending, error: googleLoginError } = useGoogleLogin();

  // 429s from the backend limiters (auth-ip-spray-guard / auth-email-strict / auth-ip-relaxed).
  const loginRateLimit = useRateLimit(loginError);
  const googleRateLimit = useRateLimit(googleLoginError);
  const isRateLimited = loginRateLimit.active || googleRateLimit.active;

  const onSubmit = (data: FieldValues) => {
    loginUser(
      { email: data.email, password: data.password },
      {
        onSuccess: () => {
          navigate(from, { replace: true });
        },
      },
    );
  };

  const handleGoogleSuccess = ({ credential }: CredentialResponse) => {
    if (!credential) {
      return;
    }

    googleLogin(
      { IdToken: credential },
      {
        onSuccess: () => {
          navigate(from, { replace: true });
        },
      },
    );
  };

  const googleErrorMessage =
    (googleLoginError as ApiErrorResponse)?.response?.data?.detail ||
    (googleLoginError as ApiErrorResponse)?.response?.data?.message ||
    "Google login failed. Please try again.";

  return (
    <div className="grow flex items-center justify-center py-20 px-4 relative overflow-hidden bg-surface">
      <div className="absolute inset-0 pointer-events-none opacity-20 z-0">
        <div className="absolute top-1/4 right-1/4 w-96 h-96 border-2 border-primary transform -rotate-12"></div>
        <div className="absolute bottom-1/4 left-1/4 w-64 h-64 border-2 border-primary"></div>
      </div>

      <div className="w-full max-w-125 bg-white border-4 border-primary relative z-10 shadow-[8px_8px_0_0_#000]">
        <div className="p-8 md:p-10 border-b-4 border-primary bg-primary text-white">
          <h1 className="font-display text-3xl md:text-4xl font-black tracking-tighter uppercase text-center drop-shadow-[2px_2px_0_rgba(0,0,0,1)]">
            LOG IN
          </h1>
        </div>
        <div className="p-8 md:p-10">
          <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
            <div className="space-y-2">
              <label className="font-mono text-sm font-bold uppercase block" htmlFor="email">
                Email
              </label>
              <input
                className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${errors.email ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                id="email"
                placeholder="HELLO@GMAIL.COM"
                type="email"
                {...register("email", gmailRules())}
              />
              {errors.email && (
                <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                  {errors.email.message as string}
                </span>
              )}
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
                {...register("password", passwordRules())}
              />
              {errors.password && (
                <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                  {errors.password.message as string}
                </span>
              )}
            </div>
            {loginError && !loginRateLimit.active && (
              <div className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase">
                {(loginError as ApiErrorResponse)?.response?.data?.detail ||
                  (loginError as ApiErrorResponse)?.response?.data?.message ||
                  "Invalid email or password"}
              </div>
            )}
            {googleLoginError && !googleRateLimit.active && (
              <div className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase">
                {googleErrorMessage}
              </div>
            )}
            <RateLimitNotice message={loginRateLimit.message} />
            <RateLimitNotice message={googleRateLimit.message} />

            <div className="pt-4 space-y-4">
              <button
                disabled={isPending || isRateLimited}
                className="w-full bg-primary text-white border-2 border-primary py-4 font-mono text-lg uppercase font-black tracking-widest hover:-translate-y-1 hover:-translate-x-1 hover:shadow-[4px_4px_0_0_#000] hover:bg-white hover:text-black transition-all duration-200 active:translate-y-0 active:translate-x-0 active:shadow-none disabled:opacity-50 disabled:cursor-not-allowed"
                type="submit"
              >
                {isPending ? "LOGGING IN..." : "ENTER"}
              </button>
            </div>

            <div className="relative flex items-center py-2 mt-6">
              <div className="grow border-t-2 border-primary"></div>
              <span className="shrink-0 mx-4 font-mono text-sm text-secondary font-bold uppercase tracking-widest">
                OR
              </span>
              <div className="grow border-t-2 border-primary"></div>
            </div>

            <div className={`flex justify-center ${isGooglePending || isRateLimited ? "pointer-events-none opacity-50" : ""}`}>
              <GoogleLogin
                onSuccess={handleGoogleSuccess}
                onError={() => undefined}
                text="signin_with"
                theme="outline"
                size="large"
                width="400"
                useOneTap={false}
                auto_select={false}
              />
            </div>
          </form>
        </div>
        <div className="p-6 border-t-4 border-primary bg-surface-container-low text-center flex flex-col gap-3">
          <p className="font-mono text-sm text-secondary font-bold">
            DON'T HAVE AN ACCOUNT?{" "}
            <Link
              className="text-primary underline hover:bg-primary hover:text-white px-1 transition-colors duration-200"
              to="/auth/register"
              state={{ from: location.state?.from }}
            >
              REGISTER
            </Link>
          </p>
          <p className="font-mono text-sm text-secondary font-bold">
            DID YOU FORGET YOUR PASSWORD?{" "}
            <Link
              className="text-primary underline hover:bg-primary hover:text-white px-1 transition-colors duration-200"
              to="/auth/forgot-password"
            >
              RESET IT
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
