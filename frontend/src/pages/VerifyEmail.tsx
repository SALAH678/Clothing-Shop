import { useState, useRef, type KeyboardEvent, useEffect } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useVerifyEmail, useResendCode } from "../features/auth/hooks";
import type { ApiErrorResponse } from "../features/auth/types/ApiErrorResponse";

export default function VerifyEmail() {
  const navigate = useNavigate();
  const location = useLocation();
  const email = location.state?.email || "";

  const [code, setCode] = useState(["", "", "", "", "", ""]);
  const [codeError, setCodeError] = useState("");
  const [statusMessage, setStatusMessage] = useState("");
  const [timeLeft, setTimeLeft] = useState(300); // 5 minutes in seconds
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  const { mutate: verify, isPending: isVerifying, error: verifyError } = useVerifyEmail();
  const { mutate: resend, isPending: isResending } = useResendCode();

  useEffect(() => {
    if (timeLeft <= 0) return;

    const timer = setInterval(() => {
      setTimeLeft((prev) => prev - 1);
    }, 1000);

    return () => clearInterval(timer);
  }, [timeLeft]);

  const handleVerify = (e: React.FormEvent) => {
    e.preventDefault();
    const fullCode = code.join("");
    if (fullCode.length !== 6) {
      setCodeError("Please enter the 6-digit code");
      return;
    }
    setCodeError("");
    setStatusMessage("");

    verify(
      { email, code: fullCode },
      {
        onSuccess: () => {
          // Redirect to login or profile
          navigate("/auth/login", { replace: true });
        },
      },
    );
  };

  const handleResendCode = (e: React.MouseEvent) => {
    e.preventDefault();
    if (!email || isResending) return;

    setCodeError("");
    setStatusMessage("");

    resend(
      { email, verificationTokenType: "EmailVerification" },
      {
        onSuccess: () => {
          setTimeLeft(300); // Reset timer to 5 minutes
          setStatusMessage("A new verification code has been sent!");
        },
        onError: (err: Error) => {
          setCodeError((err as ApiErrorResponse)?.response?.data?.detail || "Failed to resend verification code.");
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

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, "0")}`;
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
            VERIFY EMAIL
          </h1>
        </div>
        <div className="p-8 md:p-10">
          <p className="font-mono text-sm mb-6 text-secondary font-bold uppercase">
            Enter the 6-digit code sent to your email to verify your account.
          </p>
          <form onSubmit={handleVerify} className="space-y-6">
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

              {verifyError && (
                <div className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase mt-2">
                  {(verifyError as ApiErrorResponse)?.response?.data?.detail ||
                    (verifyError as ApiErrorResponse)?.response?.data?.message ||
                    "Invalid verification code."}
                </div>
              )}

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
                    disabled={isResending}
                    className="text-primary underline hover:bg-primary hover:text-white px-1 transition-colors duration-200 disabled:opacity-50"
                  >
                    {isResending ? "SENDING CODE..." : "RESEND CODE"}
                  </button>
                )}
              </div>
              <p className="font-mono text-[10px] sm:text-xs text-secondary font-bold uppercase mt-2 text-center">
                Note: If you didn't find the code, check your spam section.
              </p>
            </div>

            <div className="pt-4 space-y-4 flex flex-col items-center">
              <button
                disabled={isVerifying}
                className="w-full bg-primary text-white border-2 border-primary py-4 font-mono text-lg uppercase font-black tracking-widest hover:-translate-y-1 hover:-translate-x-1 hover:shadow-[4px_4px_0_0_#000] hover:bg-white hover:text-black transition-all duration-200 active:translate-y-0 active:translate-x-0 active:shadow-none disabled:opacity-50 disabled:cursor-not-allowed"
                type="submit"
              >
                {isVerifying ? "VERIFYING..." : "VERIFY"}
              </button>
            </div>
          </form>
        </div>
        <div className="p-6 border-t-4 border-primary bg-surface-container-low text-center">
          <p className="font-mono text-sm text-secondary font-bold">
            WRONG EMAIL?{" "}
            <Link
              className="text-primary underline hover:bg-primary hover:text-white px-1 transition-colors duration-200"
              to="/auth/register"
            >
              SIGN UP AGAIN
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
