export function getPrice(basePrice: number, discount?: number | null): number {
  return basePrice - (discount ?? 0);
}

export function formatPriceDA(value: number): string {
  return `${new Intl.NumberFormat("fr-DZ").format(value)} DA`;
}