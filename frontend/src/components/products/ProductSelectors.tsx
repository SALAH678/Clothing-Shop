import { useEffect, useState } from "react";
import type { Variant } from "../../features/products/types/Product";

interface ProductSelectorsProps {
  variants: Variant[];
  onSelectionChange?: (
    variant: Variant | undefined,
    quantity: number,
    selection: { selectedColor: string; selectedSize: string }
  ) => void;
}

const colorHexes: Record<string, string> = {
  black: "#000000",
  blanc: "#ffffff",
  blue: "#3b5998",
  bleu: "#3b5998",
  gray: "#808080",
  gris: "#808080",
  green: "#228b22",
  red: "#c0392b",
  rouge: "#c0392b",
  white: "#ffffff",
  yellow: "#f1c40f",
  jaune: "#f1c40f",
};

function getColorHex(color: string) {
  return colorHexes[color.trim().toLowerCase()] ?? "#d1d5db";
}

export default function ProductSelectors({ variants, onSelectionChange }: ProductSelectorsProps) {
  const colors = [...new Set(variants.map((variant) => variant.color))];
  const sizes = [...new Set(variants.map((variant) => variant.size))];
  const [selectedColor, setSelectedColor] = useState("");
  const [selectedSize, setSelectedSize] = useState("");
  const [quantity, setQuantity] = useState(1);
  const selectedVariant = variants.find((variant) => variant.color === selectedColor && variant.size === selectedSize);
  const stockQuantity = selectedVariant?.stockQuantity ?? 0;
  const hasStock = stockQuantity > 0;
  const quantityError =
    selectedColor && selectedSize && !selectedVariant
      ? "This size and color combination is unavailable."
      : selectedVariant && !hasStock
        ? "This product is out of stock."
        : selectedVariant && quantity >= stockQuantity
          ? "This product is out of stock."
          : null;

  useEffect(() => {
    onSelectionChange?.(selectedVariant, quantity, { selectedColor, selectedSize });
  }, [onSelectionChange, quantity, selectedVariant, selectedColor, selectedSize]);

  const selectColor = (color: string) => {
    setSelectedColor((prev) => (prev === color ? "" : color));
    setQuantity(1);
  };

  const selectSize = (size: string) => {
    setSelectedSize((prev) => (prev === size ? "" : size));
    setQuantity(1);
  };

  return (
    <div className="p-8 border-b-2 border-primary flex flex-col gap-8 bg-surface">
      {/* Color Selector */}
      <div>
        <div className="flex justify-between items-center mb-4">
          <span className="font-mono text-sm font-bold uppercase">Color</span>
          <span className="font-mono text-sm text-primary font-black uppercase">
            {selectedColor || "None selected"}
          </span>
        </div>
        <div className="flex gap-4">
          {colors.map((color) => (
            <button
              key={color}
              onClick={() => selectColor(color)}
              className={`w-12 h-12 border-2 border-primary relative transition-all duration-200 ${selectedColor !== color ? "grayscale hover:grayscale-0" : ""}`}
              style={{
                backgroundColor: getColorHex(color),
                filter: selectedColor !== color ? "grayscale(100%)" : "grayscale(0%)",
              }}
              title={color}
              aria-label={`Select ${color}`}
            >
              {selectedColor === color && (
                <span className="absolute inset-0 border-2 border-primary -m-1 pointer-events-none"></span>
              )}
            </button>
          ))}
        </div>
      </div>

      {/* Size Selector */}
      <div>
        <div className="flex justify-between items-center mb-4">
          <span className="font-mono text-sm font-bold uppercase">Size</span>
        </div>
        <div className="grid grid-cols-4 sm:grid-cols-5 gap-0 border-t-2 border-l-2 border-primary">
          {sizes.map((size) => (
            <button
              key={size}
              onClick={() => selectSize(size)}
              className={`aspect-square border-r-2 border-b-2 border-primary font-mono text-sm font-bold transition-colors ${selectedSize === size ? "bg-primary text-white" : "bg-transparent hover:bg-primary hover:text-white"}`}
            >
              {size}
            </button>
          ))}
        </div>
      </div>

      {/* Quantity Selector */}
      <div>
        <div className="flex justify-between items-center mb-4">
          <span className="font-mono text-sm font-bold uppercase">Quantity</span>
        </div>
        <div className="flex w-32 border-2 border-primary bg-surface h-12">
          <button
            type="button"
            onClick={() => setQuantity(Math.max(1, quantity - 1))}
            className="w-10 h-full flex justify-center items-center font-mono text-xl font-bold hover:bg-primary hover:text-white transition-colors"
          >
            -
          </button>
          <div className="flex-1 h-full flex justify-center items-center font-mono font-bold text-lg border-l-2 border-r-2 border-primary">
            {quantity}
          </div>
          <button
            type="button"
            disabled={!hasStock || quantity >= stockQuantity}
            onClick={() => setQuantity(Math.min(stockQuantity, quantity + 1))}
            className="w-10 h-full flex justify-center items-center font-mono text-xl font-bold hover:bg-primary hover:text-white transition-colors"
          >
            +
          </button>
        </div>
        {quantityError && (
          <p className="text-red-600 font-mono text-xs font-bold uppercase mt-2" role="alert">
            {quantityError}
          </p>
        )}
      </div>
    </div>
  );
}
