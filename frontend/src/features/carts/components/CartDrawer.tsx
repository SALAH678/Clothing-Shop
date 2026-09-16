import { useLocation, useNavigate } from "react-router-dom";
import { X, Trash2, ShoppingBag } from "lucide-react";
import { useCart } from "../hooks/useCart";
import { useAuth } from "../../auth/hooks/useAuth";

function getImageUrl(imageUrl: string | null) {
  if (!imageUrl) return undefined;
  if (/^https?:\/\//i.test(imageUrl)) return imageUrl;

  const backEndUrl = import.meta.env.VITE_API_URL ?? "";
  return `${backEndUrl.replace(/\/$/, "")}/${imageUrl.replace(/^\//, "")}`;
}

export default function CartDrawer() {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated } = useAuth();
  const { items, isLoading, error, isCartOpen, setIsCartOpen, removeFromCart, cartTotal } = useCart();

  if (!isCartOpen) return null;

  const handleCheckout = () => {
    setIsCartOpen(false);

    if (!isAuthenticated) {
      navigate("/auth/login", { state: { from: location } });
      return;
    }

    if (items.length === 0) return;

    navigate("/checkout", {
      state: {
        origin: "Cart",
        cartItems: items.map((item) => ({
          variantId: item.variant.id,
          quantity: item.quantity,
          name: item.variant.product.name,
          price: item.variant.product.basePrice - item.variant.product.discount,
          color: item.variant.color,
          size: item.variant.size,
          imageUrl: item.variant.product.imageUrl ?? "",
        })),
      },
    });
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/60 z-60 backdrop-blur-md transition-opacity"
        onClick={() => setIsCartOpen(false)}
      />

      {/* Drawer */}
      <div className="fixed top-0 right-0 h-full w-full sm:w-112.5 bg-surface border-l-4 border-primary z-70 shadow-[-10px_0_30px_rgba(0,0,0,0.5)] flex flex-col animate-in slide-in-from-right duration-300">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b-4 border-primary bg-primary text-white">
          <h2 className="font-display text-3xl font-black tracking-tighter uppercase drop-shadow-[2px_2px_0_rgba(0,0,0,1)]">
            YOUR CART
          </h2>
          <button
            onClick={() => setIsCartOpen(false)}
            className="w-10 h-10 border-2 border-transparent hover:border-white flex items-center justify-center hover:rotate-90 transition-all duration-300 active:scale-90 hover:shadow-[4px_4px_0_rgba(0,0,0,1)] hover:bg-white hover:text-black"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Items */}
        <div className="flex-1 overflow-y-auto p-6 space-y-4">
          {isLoading ? (
            <div className="h-full flex items-center justify-center">
              <span className="font-mono text-xs uppercase animate-pulse">Updating cart...</span>
            </div>
          ) : items.length === 0 ? (
            <div className="h-full flex flex-col items-center justify-center text-center p-8 border-2 border-dashed border-primary">
              <ShoppingBag className="w-12 h-12 mb-4 opacity-50 stroke-1" />
              <p className="font-mono text-sm uppercase font-bold text-secondary">Your Bag is Empty</p>
            </div>
          ) : (
            items.map((item) => (
              <div
                key={item.id}
                className="flex gap-4 p-4 border-2 border-primary bg-white shadow-[4px_4px_0_0_#000] relative group hover:-translate-y-0.5 hover:-translate-x-0.5 hover:shadow-[6px_6px_0_0_#000] transition-all duration-200"
              >
                <div className="w-20 h-24 bg-surface border-2 border-primary shrink-0 relative overflow-hidden">
                  {item.variant.product.imageUrl && (
                    <img
                      src={getImageUrl(item.variant.product.imageUrl)}
                      alt={item.variant.product.name}
                      className="w-full h-full object-cover grayscale contrast-125 group-hover:grayscale-0 transition-all duration-300"
                    />
                  )}
                </div>
                <div className="flex-1 flex flex-col justify-between">
                  <div className="pr-6">
                    <h3 className="font-mono text-sm font-black uppercase leading-tight line-clamp-1">
                      {item.variant.product.name}
                    </h3>
                    <div className="flex gap-3 text-xs font-mono text-secondary mt-1">
                      <span>SZ: {item.variant.size}</span>
                      <span>•</span>
                      <span>CLR: {item.variant.color}</span>
                    </div>
                  </div>
                  <div className="flex items-center justify-between mt-2">
                    <span className="font-mono text-xs font-bold border border-primary px-2 py-0.5 bg-surface">
                      QTY: {item.quantity}
                    </span>
                    <span className="font-mono text-sm font-black">
                      {(item.variant.product.basePrice - item.variant.product.discount).toLocaleString()} DA
                    </span>
                  </div>
                </div>
                <button
                  onClick={() => removeFromCart(item.id)}
                  className="absolute top-4 right-4 text-secondary hover:text-red-600 transition-colors"
                  aria-label="Remove item"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            ))
          )}
          {error && (
            <p className="text-red-600 font-mono text-xs font-bold uppercase" role="alert">
              {error}
            </p>
          )}
        </div>

        {/* Footer */}
        {items.length > 0 && (
          <div className="p-6 border-t-4 border-primary bg-surface shadow-[0_-10px_20px_rgba(0,0,0,0.05)] relative z-10">
            <div className="flex justify-between items-end mb-6 border-b-2 border-dashed border-primary pb-4">
              <span className="font-mono text-sm font-bold uppercase tracking-widest text-secondary">Total Amount</span>
              <span className="font-display text-3xl font-black">{cartTotal.toLocaleString()} DA</span>
            </div>
            <button
              onClick={handleCheckout}
              className="w-full bg-primary text-white font-mono text-lg uppercase py-5 px-6 border-2 border-primary tracking-widest font-black transition-all duration-300 ease-out hover:-translate-y-2 hover:-translate-x-1 shadow-[4px_4px_0_0_#000] hover:shadow-[8px_8px_0_0_#000] hover:bg-white hover:text-black active:translate-y-0 active:translate-x-0 active:shadow-none cursor-pointer"
            >
              PROCEED TO CHECKOUT
            </button>
          </div>
        )}
      </div>
    </>
  );
}
