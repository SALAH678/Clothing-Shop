import { useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useForm, type SubmitHandler } from "react-hook-form";
import { useCart } from "../features/carts/hooks/useCart";
import { ArrowLeft, ArrowRight } from "lucide-react";
import { createPurchase, type CreatePurchaseRequest } from "../features/purchases/api/purchaseApi";
import { useProduct } from "../features/products/hooks/useProduct";
import EmptyState from "../components/ui/EmptyState";
import { resolveImageUrl } from "../lib/imageUrl";
import { formatPriceDA, getPrice } from "../lib/pricing";
import { getRateLimitMessage } from "../lib/rateLimit";

interface CheckoutFormData {
  phone: string;
  wilaya: string;
  city: string;
  street: string;
}

interface BuyNowLocationState {
  productId?: string;
  variantId: string;
  quantity: number;
}

export default function Checkout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { items: cartItems, isLoading: cartLoading, setIsCartOpen, clearCart } = useCart();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submissionError, setSubmissionError] = useState<string | null>(null);

  const locationState = location.state as {
    origin?: "BuyNow" | "Cart";
    buyNowItem?: BuyNowLocationState;
  } | null;

  const buyNowItem = locationState?.origin === "BuyNow" ? locationState?.buyNowItem : undefined;
  const isBuyNow = Boolean(buyNowItem?.variantId);

  // Re-derive Buy Now display data from the catalog so a refresh, direct URL,
  // or tampered location.state can never change names/prices.
  const { data: buyNowProduct, isPending: buyNowPending } = useProduct(isBuyNow ? buyNowItem?.productId : undefined);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CheckoutFormData>({
    mode: "onBlur",
  });

  // Source of truth: cart context for Cart origin, catalog API for Buy Now.
  // location.state carries ids only (variantId/productId/quantity) — never prices.
  const displayedItems = useMemo(() => {
    if (isBuyNow && buyNowItem) {
      const variant = buyNowProduct?.variants.find((v) => v.id === buyNowItem.variantId);
      if (!buyNowProduct || !variant) return [];
      const mainImage = buyNowProduct.images.find((img) => img.isMain) ?? buyNowProduct.images[0];
      return [
        {
          id: variant.id,
          variantId: variant.id,
          name: buyNowProduct.name,
          quantity: Math.max(1, Math.min(variant.stockQuantity, buyNowItem.quantity)),
          price: getPrice(buyNowProduct.basePrice, buyNowProduct.discount),
          color: variant.color,
          size: variant.size,
          imageUrl: resolveImageUrl(mainImage?.imageUrl) ?? "",
        },
      ];
    }
    return cartItems.map((item) => ({
      id: item.id,
      variantId: item.variant.id,
      name: item.variant.product.name,
      quantity: item.quantity,
      price: getPrice(item.variant.product.basePrice, item.variant.product.discount),
      color: item.variant.color,
      size: item.variant.size,
      imageUrl: resolveImageUrl(item.variant.product.imageUrl) ?? "",
    }));
  }, [isBuyNow, buyNowItem, buyNowProduct, cartItems]);

  const totalAmount = displayedItems.reduce((sum, item) => sum + item.price * item.quantity, 0);

  if (isBuyNow && !buyNowPending && displayedItems.length === 0) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center gap-6 p-8">
        <EmptyState
          title="Item unavailable"
          message="This Buy Now item could not be loaded. It may be out of stock or the link expired."
        />
        <button
          type="button"
          onClick={() => navigate(-1)}
          className="font-mono text-sm font-bold uppercase border-2 border-primary px-6 py-3 hover:bg-primary hover:text-white transition-colors"
        >
          Go back
        </button>
      </div>
    );
  }

  const onSubmit: SubmitHandler<CheckoutFormData> = async (data) => {
    if (isSubmitting) return;

    if (displayedItems.length === 0) {
      setSubmissionError("There are no items in your order.");
      return;
    }

    setIsSubmitting(true);
    setSubmissionError(null);

    const purchaseOrigin: "BuyNow" | "Cart" = isBuyNow ? "BuyNow" : "Cart";

    const payload: CreatePurchaseRequest = {
      customerPhone: data.phone.trim(),
      wilaya: data.wilaya.trim(),
      city: data.city.trim(),
      street: data.street.trim(),
      origin: purchaseOrigin,
      purchaseItems: displayedItems.map((item) => ({
        variantId: item.variantId,
        quantity: item.quantity,
      })),
    };

    try {
      const result = await createPurchase(payload);

      // If purchased from cart, clear client-side cart state to avoid unnecessary fetch requests
      if (purchaseOrigin === "Cart") {
        clearCart();
      }

      // Store purchaseId for confirmation or history lookup
      sessionStorage.setItem("lastPurchaseId", result.purchaseId);

      if (result.checkoutUrl) {
        if (/^https?:\/\//i.test(result.checkoutUrl)) {
          window.location.assign(result.checkoutUrl);
        } else {
          navigate(result.checkoutUrl);
        }
      } else {
        navigate("/checkout/success");
      }
    } catch (err: unknown) {
      const responseData = (err as { response?: { data?: { error?: string; detail?: string; message?: string } } })
        ?.response?.data;
      const errorMsg =
        responseData?.detail ||
        responseData?.error ||
        responseData?.message ||
        (err instanceof Error ? err.message : "Failed to place order. Please check your information and try again.");
      // A 429 from purchase-strict has no body — surface the retry window instead.
      setSubmissionError(getRateLimitMessage(err, errorMsg));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col font-mono selection:bg-primary selection:text-white bg-surface pb-12">
      {/* Header */}
      <header className="w-full border-b-4 border-primary bg-white sticky top-0 z-50 h-16 flex justify-between items-center px-4 md:px-8 shadow-sm">
        <button
          onClick={() => {
            if (isBuyNow) {
              navigate(-1);
            } else {
              navigate(-1);
              setIsCartOpen(true);
            }
          }}
          className="font-mono text-sm font-bold uppercase hover:bg-primary hover:text-white px-2 py-1 transition-colors flex items-center gap-2 border-2 border-transparent hover:border-primary active:scale-95 cursor-pointer"
        >
          <ArrowLeft className="w-4 h-4" />
          <span className="hidden sm:inline">{isBuyNow ? "RETURN TO PRODUCT" : "RETURN TO BAG"}</span>
        </button>
        <div className="font-display text-2xl font-black tracking-tighter text-primary uppercase">CLOTHING SHOP</div>
      </header>

      {/* Main Checkout Grid Structure */}
      <main className="grow max-w-7xl mx-auto w-full grid grid-cols-1 md:grid-cols-2 divide-y-4 md:divide-y-0 md:divide-x-4 divide-primary border-l-4 border-r-4 border-t-4 border-b-4 border-primary shadow-[12px_12px_0_0_#000] mt-8 bg-white relative z-10">
        {/* LEFT COLUMN: SHIPPING & CONTACT */}
        <section className="p-6 md:p-10 bg-white flex flex-col gap-12">
          <div className="flex flex-col gap-2 border-b-4 border-primary pb-6">
            <h1 className="font-display text-4xl md:text-5xl font-black uppercase tracking-tighter">
              SHIPPING & CONTACT
            </h1>
          </div>

          <form id="checkout-form" onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-8">
            {/* Contact Info */}
            <div className="flex flex-col gap-6">
              <h2 className="font-mono text-lg font-black border-b-2 border-primary pb-2 uppercase tracking-widest">
                CONTACT INFORMATION
              </h2>
              <div className="flex flex-col gap-2">
                <label className="font-mono text-sm font-bold uppercase" htmlFor="phone">
                  Phone Number
                </label>
                <input
                  className={`w-full bg-surface border-2 p-4 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white transition-colors placeholder:text-zinc-500 ${
                    errors.phone ? "border-red-600 focus:border-red-600" : "border-primary focus:border-primary"
                  }`}
                  id="phone"
                  placeholder="05 / 06 / 07XXXXXXXX"
                  type="tel"
                  maxLength={10}
                  {...register("phone", {
                    required: "Phone number is required",
                    pattern: {
                      value: /^0[5-7]\d{8}$/,
                      message: "Must be 10 digits starting with 05, 06, or 07",
                    },
                  })}
                />
                {errors.phone && (
                  <p className="text-red-600 font-mono text-xs font-bold uppercase" role="alert">
                    {errors.phone.message}
                  </p>
                )}
              </div>
            </div>

            {/* Shipping Details */}
            <div className="flex flex-col gap-6 pt-6">
              <h2 className="font-mono text-lg font-black border-b-2 border-primary pb-2 uppercase tracking-widest">
                SHIPPING DESTINATION
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="flex flex-col gap-2">
                  <label className="font-mono text-sm font-bold uppercase" htmlFor="wilaya">
                    Wilaya
                  </label>
                  <input
                    className={`w-full bg-surface border-2 p-4 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white transition-colors placeholder:text-zinc-500 ${
                      errors.wilaya ? "border-red-600 focus:border-red-600" : "border-primary focus:border-primary"
                    }`}
                    id="wilaya"
                    placeholder="ENTER WILAYA (e.g. Algiers)"
                    type="text"
                    {...register("wilaya", {
                      required: "Wilaya is required",
                      pattern: {
                        value: /^[a-zA-Z\u0600-\u06FF]+$/,
                        message: "Wilaya must be one word and contain only letters",
                      },
                      minLength: {
                        value: 2,
                        message: "Wilaya must be at least 2 characters",
                      },
                      maxLength: {
                        value: 100,
                        message: "Wilaya cannot exceed 100 characters",
                      },
                    })}
                  />
                  {errors.wilaya && (
                    <p className="text-red-600 font-mono text-xs font-bold uppercase" role="alert">
                      {errors.wilaya.message}
                    </p>
                  )}
                </div>
                <div className="flex flex-col gap-2">
                  <label className="font-mono text-sm font-bold uppercase" htmlFor="city">
                    City
                  </label>
                  <input
                    className={`w-full bg-surface border-2 p-4 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white transition-colors placeholder:text-zinc-500 ${
                      errors.city ? "border-red-600 focus:border-red-600" : "border-primary focus:border-primary"
                    }`}
                    id="city"
                    placeholder="ENTER CITY"
                    type="text"
                    {...register("city", {
                      required: "City is required",
                      pattern: {
                        value: /^[a-zA-Z\u0600-\u06FF\s]+$/,
                        message: "City can only contain letters and spaces",
                      },
                      minLength: {
                        value: 2,
                        message: "City must be at least 2 characters",
                      },
                      maxLength: {
                        value: 100,
                        message: "City cannot exceed 100 characters",
                      },
                    })}
                  />
                  {errors.city && (
                    <p className="text-red-600 font-mono text-xs font-bold uppercase" role="alert">
                      {errors.city.message}
                    </p>
                  )}
                </div>
              </div>
              <div className="flex flex-col gap-2">
                <label className="font-mono text-sm font-bold uppercase" htmlFor="street">
                  Shipping Address
                </label>
                <textarea
                  className={`w-full bg-surface border-2 p-4 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white transition-colors resize-none placeholder:text-zinc-500 ${
                    errors.street ? "border-red-600 focus:border-red-600" : "border-primary focus:border-primary"
                  }`}
                  id="street"
                  placeholder="STREET ADDRESS, APARTMENT, SUITE, ETC."
                  rows={3}
                  {...register("street", {
                    required: "Shipping address is required",
                    minLength: {
                      value: 5,
                      message: "Address must be at least 5 characters",
                    },
                    maxLength: {
                      value: 200,
                      message: "Address cannot exceed 200 characters",
                    },
                    pattern: {
                      value: /^[a-zA-Z0-9\u0600-\u06FF\s,.-]+$/,
                      message: "Address can only contain letters, numbers, and spaces without special characters",
                    },
                  })}
                />
                {errors.street && (
                  <p className="text-red-600 font-mono text-xs font-bold uppercase" role="alert">
                    {errors.street.message}
                  </p>
                )}
              </div>
            </div>
          </form>
        </section>

        {/* RIGHT COLUMN: SUMMARY */}
        <section className="flex flex-col bg-surface-container-lowest">
          {/* Order Summary Section */}
          <div className="p-6 md:p-10 flex flex-col gap-8 grow bg-white">
            <h3 className="font-display text-2xl font-black uppercase tracking-tighter border-b-4 border-primary pb-4">
              ORDER SUMMARY
            </h3>

            {/* Dynamic Product Preview */}
            <div className="flex flex-col gap-4 max-h-80 overflow-y-auto pr-2" style={{ scrollbarWidth: "thin" }}>
              {displayedItems.length === 0 ? (
                <div className="text-center font-mono font-bold text-secondary py-8 border-2 border-dashed border-primary">
                  YOUR ORDER IS EMPTY
                </div>
              ) : (
                displayedItems.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-start gap-4 border-2 border-primary p-3 bg-white shadow-[4px_4px_0_0_#000] transition-all hover:translate-x-1 hover:translate-y-1 hover:shadow-[2px_2px_0_0_#000]"
                  >
                    <div className="w-20 h-24 bg-surface border-2 border-primary shrink-0 relative overflow-hidden">
                      {item.imageUrl ? (
                        <img
                          src={item.imageUrl}
                          alt={item.name}
                          loading="lazy"
                          decoding="async"
                          className="w-full h-full object-cover transition-transform duration-300 hover:scale-105"
                        />
                      ) : (
                        <div className="w-full h-full bg-zinc-200 flex items-center justify-center font-mono text-[10px] text-zinc-400">
                          NO IMG
                        </div>
                      )}
                    </div>
                    <div className="flex flex-col justify-between h-full grow py-1">
                      <div>
                        <p className="font-mono text-sm font-bold uppercase leading-tight line-clamp-2">{item.name}</p>
                        <p className="font-mono text-xs text-secondary font-bold uppercase mt-1">
                          SIZE: {item.size} • COLOR: {item.color}
                        </p>
                        <p className="font-mono text-xs text-secondary font-bold uppercase mt-0.5">
                          QTY: {item.quantity}
                        </p>
                      </div>
                      <p className="font-mono text-sm font-black text-right">
                        {formatPriceDA(item.price * item.quantity)}
                      </p>
                    </div>
                  </div>
                ))
              )}
            </div>

            {/* Totals */}
            <div className="flex flex-col gap-4 font-mono text-sm font-bold pt-4">
              <div className="flex justify-between items-center">
                <span className="text-secondary uppercase tracking-widest">SUBTOTAL</span>
                <span>{formatPriceDA(totalAmount)}</span>
              </div>
              <div className="flex justify-between items-center border-t-4 border-dashed border-primary pt-4 mt-2">
                <span className="font-black text-xl uppercase tracking-widest">TOTAL</span>
                <span className="font-display font-black text-3xl">{formatPriceDA(totalAmount)}</span>
              </div>
            </div>

            {submissionError && (
              <div className="p-4 bg-red-50 border-2 border-red-600 text-red-600 font-mono text-xs font-bold uppercase">
                {submissionError}
              </div>
            )}

            {/* Primary Action */}
            <button
              form="checkout-form"
              type="submit"
              disabled={
                displayedItems.length === 0 || isSubmitting || (isBuyNow && buyNowPending) || (!isBuyNow && cartLoading)
              }
              aria-busy={isSubmitting}
              className="mt-4 w-full bg-primary text-white font-mono text-lg font-black p-5 border-2 border-primary hover:bg-white hover:text-primary transition-all duration-300 uppercase flex justify-between items-center group shadow-[8px_8px_0_0_#000] hover:shadow-[4px_4px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
            >
              <span className="tracking-widest">{isSubmitting ? "PROCESSING..." : "COMPLETE ORDER"}</span>
              <ArrowRight className="w-6 h-6 transform group-hover:translate-x-2 transition-transform" />
            </button>
            <p className="text-center font-mono text-[10px] sm:text-xs font-bold text-secondary uppercase mt-2">
              By completing this order, you agree to our Terms & Conditions.
            </p>
          </div>
        </section>
      </main>
    </div>
  );
}
