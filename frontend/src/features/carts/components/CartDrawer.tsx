import { X, Trash2, ShoppingBag } from "lucide-react";
import { useCart } from "../hooks/useCart";

function getImageUrl(imageUrl: string | null) {
  if (!imageUrl) return undefined;
  if (/^https?:\/\//i.test(imageUrl)) return imageUrl;

  const backEndUrl = import.meta.env.VITE_API_URL ?? "";
  return `${backEndUrl.replace(/\/$/, "")}/${imageUrl.replace(/^\//, "")}`;
}

export default function CartDrawer() {
  const { items, isLoading, error, isCartOpen, setIsCartOpen, removeFromCart, cartTotal } = useCart();

  if (!isCartOpen) return null;

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
        <div className="flex-1 overflow-y-auto p-6 space-y-6 bg-surface-container-lowest">
          {isLoading ? (
            <div className="h-full flex items-center justify-center font-mono uppercase">Loading cart...</div>
          ) : items.length === 0 ? (
            <div className="h-full flex flex-col items-center justify-center text-center space-y-6">
              <ShoppingBag className="w-24 h-24 text-primary opacity-20" />
              <span className="font-mono text-lg uppercase font-bold text-primary opacity-50">Cart is empty</span>
            </div>
          ) : (
            items.map((item) => (
              <div
                key={item.id}
                className="flex gap-4 bg-white border-2 border-primary p-3 group transition-all duration-300 hover:-translate-y-1 hover:-translate-x-1 hover:shadow-[6px_6px_0_0_#000]"
              >
                <div className="w-24 h-28 sm:w-28 sm:h-32 border-2 border-primary bg-surface-container shrink-0 overflow-hidden relative">
                  <img
                    src={getImageUrl(item.variant.product.imageUrl)}
                    alt={item.variant.product.name}
                    className="w-full h-full object-cover grayscale contrast-125 group-hover:grayscale-0 transition-all duration-500 scale-105 group-hover:scale-100"
                  />
                  <div className="absolute inset-0 border-[3px] border-transparent group-hover:border-primary transition-colors"></div>
                </div>

                <div className="flex-1 flex flex-col justify-between py-1">
                  <div className="flex justify-between items-start gap-2">
                    <div>
                      <h3 className="font-mono text-xs sm:text-sm font-bold line-clamp-2 uppercase leading-tight">
                        {item.variant.product.name}
                      </h3>
                      <p className="font-mono text-[10px] uppercase text-secondary mt-1">
                        {item.variant.color} / {item.variant.size}
                      </p>
                    </div>
                    <button
                      onClick={() => removeFromCart(item.id)}
                      className="text-primary hover:text-white hover:bg-red-600 transition-colors border-2 border-transparent hover:border-primary p-1 active:scale-90"
                      title="Remove item"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>

                  <div className="mt-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                    <p className="font-mono text-xs font-bold uppercase">Quantity: {item.quantity}</p>

                    <p className="font-mono text-sm sm:text-base font-black whitespace-nowrap">
                      {(
                        (item.variant.product.basePrice - item.variant.product.discount) *
                        item.quantity
                      ).toLocaleString()}{" "}
                      DA
                    </p>
                  </div>
                </div>
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
            <button className="w-full bg-primary text-white font-mono text-lg uppercase py-5 px-6 border-2 border-primary tracking-widest font-black transition-all duration-300 ease-out hover:-translate-y-2 hover:-translate-x-1 shadow-[4px_4px_0_0_#000] hover:shadow-[8px_8px_0_0_#000] hover:bg-white hover:text-black active:translate-y-0 active:translate-x-0 active:shadow-none">
              PROCEED TO CHECKOUT
            </button>
          </div>
        )}
      </div>
    </>
  );
}
