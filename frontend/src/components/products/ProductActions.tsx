import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../features/auth/hooks/useAuth";
import { useCart } from "../../features/carts/hooks/useCart";

export default function ProductActions({
  variantId,
  quantity,
  disabled,
}: {
  variantId?: string;
  quantity: number;
  disabled?: boolean;
}) {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated } = useAuth();
  const { addToCart } = useCart();
  const [isAdding, setIsAdding] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAddToBag = async () => {
    if (!isAuthenticated) {
      navigate("/auth/login", { state: { from: location } });
      return;
    }

    if (!variantId || disabled) {
      setError("Choose an available size and color first.");
      return;
    }

    setIsAdding(true);
    setError(null);
    try {
      await addToCart(variantId, quantity);
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : "Could not add this product to your cart.");
    } finally {
      setIsAdding(false);
    }
  };

  return (
    <div className="p-8 flex flex-col gap-4 border-b-2 border-primary bg-surface-container-low h-full">
      <button
        type="button"
        disabled={isAdding}
        onClick={handleAddToBag}
        className="w-full py-4 border-2 border-primary bg-primary text-white font-mono text-lg font-black tracking-widest uppercase hover:bg-white hover:text-primary transition-all duration-200 flex justify-center items-center gap-2 shadow-[4px_4px_0_0_#000] hover:shadow-none hover:translate-x-1 hover:translate-y-1 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <svg
          width="24"
          height="24"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4Z" />
          <path d="M3 6h18" />
          <path d="M16 10a4 4 0 0 1-8 0" />
        </svg>
        {isAdding ? "ADDING..." : "ADD TO BAG"}
      </button>
      <button className="w-full py-4 border-2 border-primary bg-transparent text-primary font-mono text-lg font-black tracking-widest uppercase hover:bg-primary hover:text-white transition-all duration-200 flex justify-center items-center gap-2 hover:shadow-[4px_4px_0_0_#000] active:shadow-none active:translate-x-1 active:translate-y-1">
        <svg
          width="24"
          height="24"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" />
        </svg>
        BUY NOW
      </button>
      {error && (
        <p className="text-red-600 font-mono text-xs font-bold uppercase" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}
