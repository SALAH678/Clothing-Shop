import { CheckCircle, ArrowRight } from "lucide-react";
import { Link } from "react-router-dom";

export default function CheckoutSuccess() {
  const purchaseId = sessionStorage.getItem("lastPurchaseId");

  return (
    <div className="min-h-[80vh] flex flex-col items-center justify-center p-6 bg-surface">
      <div className="max-w-2xl w-full bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-10 md:p-16 flex flex-col items-center text-center gap-8 relative overflow-hidden">
        {/* Background accent */}
        <div className="absolute -top-10 -right-10 w-40 h-40 bg-surface-container-highest rounded-full blur-3xl opacity-50 pointer-events-none"></div>

        <CheckCircle className="w-24 h-24 text-green-600 relative z-10" strokeWidth={2} />

        <div className="flex flex-col gap-4 relative z-10">
          <h1 className="font-display text-4xl md:text-6xl font-black uppercase tracking-tighter">Order Confirmed</h1>
          {purchaseId && (
            <div className="font-mono text-xs text-primary font-bold bg-surface border border-primary px-3 py-1 inline-block mx-auto uppercase">
              Order ID: {purchaseId}
            </div>
          )}
          <p className="font-mono text-base md:text-lg font-bold text-secondary uppercase tracking-widest leading-relaxed">
            Thanks for your purchase! <br /> We will contact you soon.
          </p>
        </div>

        <Link
          to="/categories"
          className="mt-8 w-full sm:w-auto bg-primary text-white font-mono text-lg font-black py-4 px-8 border-2 border-primary hover:bg-white hover:text-primary transition-all duration-300 uppercase flex justify-center items-center gap-4 shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none relative z-10"
        >
          <span>CONTINUE SHOPPING</span>
          <ArrowRight className="w-6 h-6" />
        </Link>
      </div>
    </div>
  );
}
