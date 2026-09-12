interface ProductInfoProps {
  title: string;
  sku: string;
  price: string;
  description?: string; // Keep prop signature just in case it's passed, but make optional
}

export default function ProductInfo({ title, sku, price, description }: ProductInfoProps) {
  return (
    <div className="p-8 border-b-2 border-primary bg-surface h-full flex flex-col justify-center">
      <h1 className="font-display text-4xl md:text-5xl font-black uppercase mb-4 wrap-break-word tracking-tighter">
        {title}
      </h1>
      <p className="font-mono text-sm text-secondary font-bold mb-6">SKU: {sku}</p>
      <div className="font-display text-3xl font-bold tracking-tight">{price}</div>
      {description && <p className="font-body text-base text-secondary mt-6">{description}</p>}
    </div>
  );
}
