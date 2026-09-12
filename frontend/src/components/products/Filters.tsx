import { useState } from "react";

interface FilterValues {
  sizes: string[] | null;
  colors: string[] | null;
  minPrice: number | null;
  maxPrice: number | null;
}

interface FiltersProps {
  initialValues: FilterValues;
  onApply: (values: FilterValues) => void;
}

export default function Filters({ initialValues, onApply }: FiltersProps) {
  const [selectedSizes, setSelectedSizes] = useState<string[]>(initialValues.sizes ?? []);
  const [selectedColors, setSelectedColors] = useState<string[]>(initialValues.colors ?? []);
  const [minPrice, setMinPrice] = useState<string>(
    initialValues.minPrice != null ? String(initialValues.minPrice) : "",
  );
  const [maxPrice, setMaxPrice] = useState<string>(
    initialValues.maxPrice != null ? String(initialValues.maxPrice) : "",
  );

  const sizes = [
    "27",
    "28",
    "29",
    "30",
    "31",
    "32",
    "33",
    "34",
    "36",
    "38",
    "39",
    "40",
    "40.5",
    "41",
    "42",
    "42.5",
    "43",
    "44",
    "45",
    "46",
    "48",
    "L",
    "M",
    "S",
    "XL",
    "XXL",
    "XXXL",
  ];
  const colors = [
    { name: "BEIGE", hex: "#F5F5DC" },
    { name: "WHITE", hex: "#FFFFFF" },
    { name: "BLUE", hex: "#2563EB" },
    { name: "LIGHT BLUE", hex: "#93C5FD" },
    { name: "NAVY BLUE", hex: "#000080" },
    { name: "VINTAGE BLUE", hex: "#5A7B9C" },
    { name: "BURGUNDY", hex: "#800020" },
    { name: "CREAM", hex: "#FFFDD0" },
    { name: "GREY", hex: "#9CA3AF" },
    { name: "LIGHT GREY", hex: "#D1D5DB" },
    { name: "DARK GREY", hex: "#374151" },
    { name: "BROWN", hex: "#5D4037" },
    { name: "BLACK", hex: "#000000" },
    { name: "PINK", hex: "#E91E63" },
    { name: "RED", hex: "#B71C1C" },
    { name: "GREEN", hex: "#16A34A" },
    { name: "MILITARY GREEN", hex: "#4B5320" },
    { name: "PURPLE", hex: "#800080" },
  ];

  const toggleSize = (size: string) => {
    setSelectedSizes((prev) => (prev.includes(size) ? prev.filter((s) => s !== size) : [...prev, size]));
  };

  const toggleColor = (colorName: string) => {
    setSelectedColors((prev) =>
      prev.includes(colorName) ? prev.filter((c) => c !== colorName) : [...prev, colorName],
    );
  };

  return (
    <aside className="w-full md:w-64 border-b md:border-b-0 md:border-r border-primary p-4 md:p-6 shrink-0 bg-surface">
      <h2 className="font-display text-2xl font-bold mb-6 border-b border-primary pb-2 tracking-tight">FILTERS</h2>

      <div className="mb-8">
        <h3 className="font-mono text-sm font-bold mb-3">PRICE (DA)</h3>
        <div className="flex items-center gap-2">
          <input
            type="number"
            placeholder="Min"
            value={minPrice}
            onChange={(e) => setMinPrice(e.target.value)}
            className="w-full bg-transparent border border-primary p-2 font-mono text-sm focus:outline-none focus:ring-1 focus:ring-primary rounded-none placeholder:text-zinc-400 transition-shadow"
          />
          <span className="font-mono font-bold">-</span>
          <input
            type="number"
            placeholder="Max"
            value={maxPrice}
            onChange={(e) => setMaxPrice(e.target.value)}
            className="w-full bg-transparent border border-primary p-2 font-mono text-sm focus:outline-none focus:ring-1 focus:ring-primary rounded-none placeholder:text-zinc-400 transition-shadow"
          />
        </div>
      </div>

      <div className="mb-8">
        <h3 className="font-mono text-sm font-bold mb-3">SIZE</h3>
        <div className="grid grid-cols-4 gap-2">
          {sizes.map((size) => {
            const isSelected = selectedSizes.includes(size);
            return (
              <button
                key={size}
                type="button"
                onClick={() => toggleSize(size)}
                className={`py-1.5 border border-primary font-mono text-xs transition-colors active:scale-95 text-center w-full ${
                  isSelected ? "bg-primary text-white" : "bg-transparent text-primary hover:bg-primary hover:text-white"
                }`}
              >
                {size}
              </button>
            );
          })}
        </div>
      </div>

      <div className="mb-8">
        <h3 className="font-mono text-sm font-bold mb-3">COLOR</h3>
        <div className="grid grid-cols-3 gap-4">
          {colors.map((color) => {
            const isSelected = selectedColors.includes(color.name);
            return (
              <div key={color.name} className="flex flex-col items-center gap-2 group cursor-pointer">
                <button
                  type="button"
                  onClick={() => toggleColor(color.name)}
                  aria-pressed={isSelected}
                  className={`w-6 h-6 rounded-full border border-primary transition-transform focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary shadow-sm active:scale-95 ${
                    isSelected ? "ring-2 ring-offset-2 ring-primary scale-125" : "group-hover:scale-125"
                  }`}
                  style={{ backgroundColor: color.hex }}
                  title={color.name}
                />
                <span
                  className={`text-[9px] font-mono uppercase text-center leading-tight transition-colors ${isSelected ? "font-bold" : ""}`}
                >
                  {color.name}
                </span>
              </div>
            );
          })}
        </div>
      </div>

      <button
        onClick={() =>
          onApply({
            sizes: selectedSizes.length ? selectedSizes : null,
            colors: selectedColors.length ? selectedColors : null,
            minPrice: minPrice === "" ? null : Number(minPrice),
            maxPrice: maxPrice === "" ? null : Number(maxPrice),
          })
        }
        className="w-full bg-primary text-white font-mono text-sm uppercase py-4 px-4 border border-primary tracking-widest font-bold shadow-xl transition-all duration-300 ease-out hover:-translate-y-2 hover:shadow-2xl hover:bg-white hover:text-black active:translate-y-1 active:scale-95 active:shadow-md mt-4"
      >
        Apply Filter
      </button>
    </aside>
  );
}
