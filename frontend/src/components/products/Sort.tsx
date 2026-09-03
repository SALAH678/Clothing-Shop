interface SortProps {
  count: number;
  value: string;
  onChange: (value: string) => void;
}

export default function Sort({ count, value, onChange }: SortProps) {
  return (
    <section className="flex justify-between items-center p-4 md:p-6 border-b border-primary bg-surface-container-low">
      <span aria-live="polite" className="font-mono text-sm font-bold">
        {count} ITEMS FOUND
      </span>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="bg-transparent border border-primary font-mono text-sm font-bold rounded-none focus:outline-none focus:ring-1 focus:ring-primary py-1.5 pl-2 pr-8 cursor-pointer hover:bg-white transition-colors"
      >
        <option value="newest">SORT: NEWEST</option>
        <option value="price-low-high">PRICE: LOW TO HIGH</option>
        <option value="price-high-low">PRICE: HIGH TO LOW</option>
      </select>
    </section>
  );
}