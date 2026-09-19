export const COLOR_HEXES: Record<string, string> = {
  black: "#000000",
  "noir charcoal": "#36454f",
  "bleu brut": "#1f3a5f",
  "bleu clair": "#7fa8c9",
  "vintage wash": "#8a9a5b",
  white: "#ffffff",
  grey: "#9ca3af",
  gray: "#9ca3af",
  beige: "#d4c5a9",
  brown: "#7c4a21",
  green: "#2f5233",
  red: "#b91c1c",
  blue: "#1d4ed8",
  "navy blue": "#000080",
  "light blue": "#93c5fd",
  "vintage blue": "#5a7b9c",
  burgundy: "#800020",
  cream: "#fffdd0",
  "light gray": "#d1d5db",
  "dark gray": "#374151",
  "military green": "#4b5320",
  pink: "#e91e63",
  purple: "#800080",
};

/** Fallback size suggestions for the variant editor's datalist. */
export const SIZE_SUGGESTIONS = ["XS", "S", "M", "L", "XL", "XXL", "XXXL", "36", "38", "40", "42", "44", "46"];

/** Fallback color suggestions, kept in sync with the storefront filter palette. */
export const COLOR_SUGGESTIONS = [
  "BLACK",
  "WHITE",
  "GRAY",
  "LIGHT GRAY",
  "DARK GRAY",
  "BEIGE",
  "CREAM",
  "BROWN",
  "BLUE",
  "NAVY BLUE",
  "LIGHT BLUE",
  "VINTAGE BLUE",
  "GREEN",
  "MILITARY GREEN",
  "RED",
  "BURGUNDY",
  "PINK",
  "PURPLE",
];

export function getColorHex(color: string) {
  return COLOR_HEXES[color.trim().toLowerCase()] ?? "#d1d5db";
}