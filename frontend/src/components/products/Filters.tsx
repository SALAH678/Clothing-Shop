import { useState } from "react";

export interface FilterValues {
  sizes: string[] | null;
  colors: string[] | null;
  minPrice: number | null;
  maxPrice: number | null;
}

interface FiltersProps {
  /** The filters currently applied on the server (URL is their source of truth). */
  values: FilterValues;
  /** Called only when the user commits with "Apply Filter" / "Clear all". */
  onApply: (values: FilterValues) => void;
}

const EMPTY_FILTERS: FilterValues = { sizes: null, colors: null, minPrice: null, maxPrice: null };

/** Stable string used to compare filter sets regardless of object identity. */
function filterSignature(values: FilterValues): string {
  return [
    (values.sizes ?? []).join(","),
    (values.colors ?? []).join(","),
    values.minPrice ?? "",
    values.maxPrice ?? "",
  ].join("|");
}

export default function Filters({ values, onApply }: FiltersProps) {
  // Sizes/colors/price are held as a *draft*. Clicking the chips only edits the
  // draft — nothing is applied (and no request is made) until "Apply Filter" is
  // pressed. This is what makes the button meaningful.
  const [draftSizes, setDraftSizes] = useState<string[]>(values.sizes ?? []);
  const [draftColors, setDraftColors] = useState<string[]>(values.colors ?? []);
  const [minPrice, setMinPrice] = useState<string>(values.minPrice != null ? String(values.minPrice) : "");
  const [maxPrice, setMaxPrice] = useState<string>(values.maxPrice != null ? String(values.maxPrice) : "");

  // Re-seed the draft only when the *applied* filter set changes (back/forward
  // navigation, or our own Apply). Comparing a signature instead of object
  // identity means unrelated URL changes — sorting, searching, pagination —
  // can never wipe out a draft the user is still editing.
  const appliedSignature = filterSignature(values);
  const [syncedSignature, setSyncedSignature] = useState(appliedSignature);

  if (syncedSignature !== appliedSignature) {
    setSyncedSignature(appliedSignature);
    setDraftSizes(values.sizes ?? []);
    setDraftColors(values.colors ?? []);
    setMinPrice(values.minPrice != null ? String(values.minPrice) : "");
    setMaxPrice(values.maxPrice != null ? String(values.maxPrice) : "");
  }

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
    { name: "GRAY", hex: "#9CA3AF" },
    { name: "LIGHT GRAY", hex: "#D1D5DB" },
    { name: "DARK GRAY", hex: "#374151" },
    { name: "BROWN", hex: "#5D4037" },
    { name: "BLACK", hex: "#000000" },
    { name: "PINK", hex: "#E91E63" },
    { name: "RED", hex: "#B71C1C" },
    { name: "GREEN", hex: "#16A34A" },
    { name: "MILITARY GREEN", hex: "#4B5320" },
    { name: "PURPLE", hex: "#800080" },
  ];

  const buildDraftValues = (): FilterValues => ({
    sizes: draftSizes.length ? draftSizes : null,
    colors: draftColors.length ? draftColors : null,
    minPrice: minPrice === "" ? null : Number(minPrice),
    maxPrice: maxPrice === "" ? null : Number(maxPrice),
  });

  const draftSignature = filterSignature(buildDraftValues());
  const hasDraftFilters = draftSignature !== "|||";
  const isDirty = draftSignature !== appliedSignature;

  const handleApply = () => {
    // Already in sync: re-applying would only churn the URL and reset paging.
    if (!isDirty) return;
    onApply(buildDraftValues());
  };

  const handleClear = () => {
    setDraftSizes([]);
    setDraftColors([]);
    setMinPrice("");
    setMaxPrice("");
    setSyncedSignature(filterSignature(EMPTY_FILTERS));
    onApply(EMPTY_FILTERS);
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
            const isSelected = draftSizes.includes(size);
            return (
              <button
                key={size}
                type="button"
                aria-pressed={isSelected}
                onClick={() =>
                  setDraftSizes((current) =>
                    current.includes(size) ? current.filter((s) => s !== size) : [...current, size],
                  )
                }
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
            const isSelected = draftColors.includes(color.name);
            return (
              <div key={color.name} className="flex flex-col items-center gap-2 group cursor-pointer">
                <button
                  type="button"
                  onClick={() =>
                    setDraftColors((current) =>
                      current.includes(color.name)
                        ? current.filter((c) => c !== color.name)
                        : [...current, color.name],
                    )
                  }
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
        type="button"
        onClick={handleApply}
        disabled={!isDirty}
        aria-disabled={!isDirty}
        className={`w-full font-mono text-sm uppercase py-4 px-4 border border-primary tracking-widest font-bold transition-all duration-300 ease-out mt-4 ${
          isDirty
            ? "bg-primary text-white shadow-xl hover:-translate-y-2 hover:shadow-2xl hover:bg-white hover:text-black active:translate-y-1 active:scale-95 active:shadow-md cursor-pointer"
            : "bg-surface-container text-secondary cursor-not-allowed opacity-60"
        }`}
      >
        {isDirty ? "Apply Filter" : "Filters Applied"}
      </button>
      {isDirty && (
        <p
          role="status"
          className="mt-3 text-center font-mono text-[10px] uppercase font-bold tracking-widest text-secondary"
        >
          Selection not applied yet — press Apply
        </p>
      )}
      {hasDraftFilters && (
        <button
          type="button"
          onClick={handleClear}
          className="w-full mt-2 font-mono text-xs uppercase font-bold tracking-widest border border-primary px-4 py-3 hover:bg-primary hover:text-white transition-colors"
        >
          Clear all
        </button>
      )}
    </aside>
  );
}
