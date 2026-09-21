import { useState, useRef, useEffect, type KeyboardEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useForm, type FieldValues } from "react-hook-form";
import { useForgotPassword, useResetPassword, useResendCode } from "../features/auth/hooks";
import type { ApiErrorResponse } from "../features/auth/types/ApiErrorResponse";
import RateLimitNotice from "../components/ui/RateLimitNotice";
import { isRateLimitError, useRateLimit } from "../lib/rateLimit";

export default function ForgotPassword() {
  const navigate = useNavigate();

  const [step, setStep] = useState<"request" | "reset">("request");
  const [email, setEmail] = useState("");
  const [code, setCode] = useState(["", "", "", "", "", ""]);
  const [timeLeft, setTimeLeft] = useState(300); // 5 minutes in seconds
  const [codeError, setCodeError] = useState("");
  const [statusMessage, setStatusMessage] = useState("");
  const [resendError, setResendError] = useState<unknown>(null);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  const { mutate: sendForgotEmail, isPending: isSendingEmail, error: forgotError } = useForgotPassword();

  const { mutate: resetUserPassword, isPending: isResetting, error: resetError } = useResetPassword();
  const { mutate: resend, isPending: isResending } = useResendCode();

  // 429s from the backend limiters (auth-target-strict / auth-email-strict).
  const forgotRateLimit = useRateLimit(forgotError);
  const resetRateLimit = useRateLimit(resetError);
  const resendRateLimit = useRateLimit(resendError);

  // Single countdown driver — the reset-step effect below owns the interval.
  // (Previously two effects each ran setInterval, doubling the tick speed.)

  const {
    register: reqRegister,
    handleSubmit: reqSubmit,
    formState: { errors: reqErrors },
  } = useForm({
    mode: "onChange",
  });

  const {
    register: resRegister,
    handleSubmit: resSubmit,
    formState: { errors: resErrors },
  } = useForm({
    mode: "onChange",
  });

  useEffect(() => {
    if (step !== "reset" || timeLeft <= 0) return;

    const timer = setInterval(() => {
      setTimeLeft((prev) => Math.max(0, prev - 1));
    }, 1000);

    return () => clearInterval(timer);
  }, [timeLeft, step]);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, "0")}`;
  };

  const onSendCode = (data: FieldValues) => {
    sendForgotEmail(data.email, {
      onSuccess: () => {
        setEmail(data.email);
        setTimeLeft(300); // Reset timer to 5 minutes
        setStep("reset");
      },
    });
  };

  const handleResendCode = () => {
    if (!email || isResending) return;

    setCodeError("");
    setStatusMessage("");

    resend(
      { email, verificationTokenType: "PasswordReset" },
      {
        onSuccess: () => {
          setResendError(null);
          setTimeLeft(300);
          setStatusMessage("A new reset code has been sent!");
        },
        onError: (err: Error) => {
          // Keep the raw error around so useRateLimit can run the live countdown.
          setResendError(err);
          if (!isRateLimitError(err)) {
            setCodeError((err as ApiErrorResponse)?.response?.data?.detail || "Failed to resend code.");
          }
        },
      },
    );
  };

  const onResetPassword = (data: FieldValues) => {
    const fullCode = code.join("");
    if (fullCode.length !== 6) {
      setCodeError("Please enter the 6-digit code");
      return;
    }
    setCodeError("");

    resetUserPassword(
      {
        email,
        code: fullCode,
        newPassword: data.newPassword,
      },
      {
        onSuccess: () => {
          navigate("/auth/login", { replace: true });
        },
      },
    );
  };

  const handleCodeChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return;

    setCodeError("");
    const newCode = [...code];
    if (value.length > 1) {
      const pastedDigits = value.replace(/\D/g, "").slice(0, 6).split("");
      for (let i = 0; i < pastedDigits.length; i++) {
        if (index + i < 6) newCode[index + i] = pastedDigits[i];
      }
      setCode(newCode);
      const nextFocus = Math.min(index + pastedDigits.length, 5);
      inputRefs.current[nextFocus]?.focus();
      return;
    }

    newCode[index] = value.slice(-1);
    setCode(newCode);

    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (index: number, e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Backspace" && !code[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  return (
    <div className="grow flex items-center justify-center py-20 px-4 relative overflow-hidden bg-surface">
      <div className="absolute inset-0 pointer-events-none opacity-20 z-0">
        <div className="absolute top-1/4 right-1/4 w-96 h-96 border-2 border-primary transform -rotate-12"></div>
        <div className="absolute bottom-1/4 left-1/4 w-64 h-64 border-2 border-primary transform rotate-45"></div>
      </div>

      <div className="w-full max-w-125 bg-white border-4 border-primary relative z-10 shadow-[8px_8px_0_0_#000]">
        <div className="p-8 md:p-10 border-b-4 border-primary bg-primary text-white">
          <h1 className="font-display text-3xl md:text-4xl font-black tracking-tighter uppercase text-center drop-shadow-[2px_2px_0_rgba(0,0,0,1)]">
            {step === "request" ? "FORGOT PASSWORD" : "RESET PASSWORD"}
          </h1>
        </div>
        <div className="p-8 md:p-10">
          {step === "request" ? (
            <>
              <p className="font-mono text-sm mb-6 text-secondary font-bold uppercase">
                Enter your email and we will send you a code to reset your password.
              </p>
              <form onSubmit={reqSubmit(onSendCode)} className="space-y-6">
                <div className="space-y-2">
                  <label className="font-mono text-sm font-bold uppercase block" htmlFor="email">
                    Email
                  </label>
                  <input
                    className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${reqErrors.email ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                    id="email"
                    placeholder="HELLO@GMAIL.COM"
                    type="email"
                    {...reqRegister("email", {
                      required: "Email is required",
                      pattern: {
                        value: /^[a-zA-Z0-9._%+-]+@gmail\.com$/,
                        message: "Must be a valid @gmail.com address",
                      },
                    })}
                  />
                  {reqErrors.email && (
                    <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                      {reqErrors.email.message as string}
                    </span>
                  )}
                  <p className="font-mono text-[10px] sm:text-xs text-secondary font-bold uppercase mt-2">
                    Note: You need to put a real email
                  </p>
                </div>

                {forgotError && !forgotRateLimit.active && (
                  <div className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase">
                    {(forgotError as ApiErrorResponse)?.response?.data?.detail ||
                      (forgotError as ApiErrorResponse)?.response?.data?.message ||
                      "Failed to send reset code. Please verify email."}
                  </div>
                )}
                <RateLimitNotice message={forgotRateLimit.message} />

                <div className="pt-4 space-y-4">
                  <button
                    disabled={isSendingEmail || forgotRateLimit.active}
                    className="w-full bg-primary text-white border-2 border-primary py-4 font-mono text-lg uppercase font-black tracking-widest hover:-translate-y-1 hover:-translate-x-1 hover:shadow-[4px_4px_0_0_#000] hover:bg-white hover:text-black transition-all duration-200 active:translate-y-0 active:translate-x-0 active:shadow-none disabled:opacity-50 disabled:cursor-not-allowed"
                    type="submit"
                  >
                    {isSendingEmail ? "SENDING CODE..." : "SEND CODE"}
                  </button>
                </div>
              </form>
            </>
          ) : (
            <>
              <p className="font-mono text-sm mb-6 text-secondary font-bold uppercase">
                Enter the 6-digit code sent to your email to set a new password.
              </p>
              <form onSubmit={resSubmit(onResetPassword)} className="space-y-6">
                <div className="space-y-2">
                  <label className="font-mono text-sm font-bold uppercase block">6-Digit Code</label>
                  <div className="flex justify-between gap-1 md:gap-2">
                    {code.map((digit, index) => (
                      <input
                        key={index}
                        ref={(el) => {
                          inputRefs.current[index] = el;
                        }}
                        type="text"
                        inputMode="numeric"
                        autoComplete="one-time-code"
                        pattern="\d*"
                        maxLength={6}
                        value={digit}
                        onChange={(e) => handleCodeChange(index, e.target.value)}
                        onKeyDown={(e) => handleKeyDown(index, e)}
                        className={`w-10 h-12 sm:w-12 sm:h-14 bg-surface border-2 text-center font-mono text-xl sm:text-2xl font-bold focus:outline-none focus:ring-0 focus:bg-white transition-colors duration-200 ${codeError ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                      />
                    ))}
                  </div>
                  {codeError && (
                    <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">{codeError}</span>
                  )}
                </div>

                {statusMessage && (
                  <div className="p-3 bg-green-50 border-2 border-green-500 font-mono text-xs font-bold text-green-700 uppercase mt-2">
                    {statusMessage}
                  </div>
                )}

                <div className="mt-4 text-center font-mono text-sm font-bold uppercase text-secondary">
                  {timeLeft > 0 ? (
                    <span>
                      Resend code in <span className="text-primary">{formatTime(timeLeft)}</span>
                    </span>
                  ) : (
                    <button
                      type="button"
                      onClick={handleResendCode}
                      disabled={isResending || resendRateLimit.active}
                      className="text-primary underline hover:bg-primary hover:text-white px-1 transition-colors duration-200 disabled:opacity-50"
                    >
                      {isResending ? "SENDING CODE..." : "RESEND CODE"}
                    </button>
                  )}
                </div>
                <RateLimitNotice message={resendRateLimit.message} />
                <p className="font-mono text-[10px] sm:text-xs text-secondary font-bold uppercase mt-2 text-center">
                  Note: If you didn't find the code, check your spam section.
                </p>

                <div className="space-y-2">
                  <label className="font-mono text-sm font-bold uppercase block" htmlFor="new-password">
                    New Password
                  </label>
                  <input
                    className={`w-full bg-surface border-2 px-4 py-3 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white placeholder:text-zinc-400 transition-colors duration-200 ${resErrors.newPassword ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                    id="new-password"
                    placeholder="••••••••"
                    type="password"
                    {...resRegister("newPassword", {
                      required: "Password is required",
                      minLength: { value: 6, message: "Must be at least 6 characters long" },
                      pattern: {
                        value: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{6,}$/,
                        message: "Must contain at least 1 uppercase, 1 lowercase, 1 number, and 1 special character",
                      },
                    })}
                  />
                  {resErrors.newPassword && (
                    <span className="text-red-500 font-mono text-xs font-bold uppercase mt-1 block">
                      {resErrors.newPassword.message as string}
                    </span>
                  )}
                  <p className="font-mono text-[10px] sm:text-xs text-secondary font-bold uppercase mt-2">
                    Note: Password must be at least 6 characters long and contain at least one uppercase letter, one
                    lowercase letter, one number, and one special character.
                  </p>
                </div>

                {resetError && !resetRateLimit.active && (
                  <div className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase">
                    {(resetError as ApiErrorResponse)?.response?.data?.detail ||
                      (resetError as ApiErrorResponse)?.response?.data?.message ||
                      "Password reset failed. Please check code or try again."}
                  </div>
                )}
                <RateLimitNotice message={resetRateLimit.message} />

                <div className="pt-4 space-y-4 flex flex-col items-center">
                  <button
                    disabled={isResetting || resetRateLimit.active}
                    className="w-full bg-primary text-white border-2 border-primary py-4 font-mono text-lg uppercase font-black tracking-widest hover:-translate-y-1 hover:-translate-x-1 hover:shadow-[4px_4px_0_0_#000] hover:bg-white hover:text-black transition-all duration-200 active:translate-y-0 active:translate-x-0 active:shadow-none disabled:opacity-50 disabled:cursor-not-allowed"
                    type="submit"
                  >
                    {isResetting ? "RESETTING PASSWORD..." : "CONFIRM RESET"}
                  </button>
                  <button
                    type="button"
                    onClick={() => setStep("request")}
                    className="text-primary font-mono text-xs font-bold uppercase tracking-widest hover:underline mt-2"
                  >
                    ← BACK TO EMAIL
                  </button>
                </div>
              </form>
            </>
          )}
        </div>
        <div className="p-6 border-t-4 border-primary bg-surface-container-low text-center">
          <p className="font-mono text-sm text-secondary font-bold">
            REMEMBERED IT?{" "}
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
