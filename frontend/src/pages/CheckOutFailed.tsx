import { useState } from "react";
import { AlertTriangle, RotateCcw } from "lucide-react";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { retryPurchase } from "../features/purchases/api/purchaseApi";

export default function CheckoutFailed() {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const [isRetrying, setIsRetrying] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Retrieve purchaseId from search params (?purchaseId=...), location state, or sessionStorage
  const purchaseId =
    searchParams.get("purchaseId") ||
    (location.state as { purchaseId?: string } | null)?.purchaseId ||
    sessionStorage.getItem("lastPurchaseId");

  const handleRetry = async () => {
    if (!purchaseId) {
      navigate("/checkout");
      return;
    }

    setIsRetrying(true);
    setErrorMessage(null);

    try {
      const result = await retryPurchase(purchaseId);
      if (result.checkoutUrl) {
        if (/^https?:\/\//i.test(result.checkoutUrl)) {
          window.location.assign(result.checkoutUrl);
        } else {
          navigate(result.checkoutUrl);
        }
      } else {
        navigate("/checkout");
      }
    } catch (err: unknown) {
      const responseData = (err as { response?: { data?: { error?: string; detail?: string; message?: string } } })?.response?.data;
      const errorText =
        responseData?.detail ||
        responseData?.error ||
        responseData?.message ||
        (err instanceof Error ? err.message : "Failed to retry payment. Please try again or return to checkout.");
      setErrorMessage(errorText);
    } finally {
      setIsRetrying(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex flex-col items-center justify-center p-6 bg-surface">
      <div className="max-w-2xl w-full bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-10 md:p-16 flex flex-col items-center text-center gap-8 relative overflow-hidden">
        {/* Background accent */}
        <div className="absolute -top-10 -left-10 w-40 h-40 bg-red-100 rounded-full blur-3xl opacity-50 pointer-events-none"></div>

        <AlertTriangle className="w-24 h-24 text-red-600 relative z-10" strokeWidth={2} />

        <div className="flex flex-col gap-4 relative z-10">
          <h1 className="font-display text-4xl md:text-6xl font-black uppercase tracking-tighter">Checkout Failed</h1>
          {purchaseId && (
            <div className="font-mono text-xs text-primary font-bold bg-surface border border-primary px-3 py-1 inline-block mx-auto uppercase">
              Order ID: {purchaseId}
            </div>
          )}
          <p className="font-mono text-base md:text-lg font-bold text-secondary uppercase tracking-widest leading-relaxed">
            Something went wrong processing your order. <br /> Please try again.
          </p>
        </div>

        {errorMessage && (
          <div className="p-4 bg-red-50 border-2 border-red-600 text-red-600 font-mono text-xs font-bold uppercase w-full">
            {errorMessage}
          </div>
        )}

        <button
          onClick={handleRetry}
          disabled={isRetrying}
          className="mt-4 w-full sm:w-auto bg-primary text-white font-mono text-lg font-black py-4 px-8 border-2 border-primary hover:bg-white hover:text-primary transition-all duration-300 uppercase flex justify-center items-center gap-4 shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none relative z-10 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
        >
          <RotateCcw className={`w-6 h-6 ${isRetrying ? "animate-spin" : ""}`} />
          <span>{isRetrying ? "RETRYING PAYMENT..." : "RETRY CHECKOUT"}</span>
        </button>
      </div>
    </div>
  );
}
