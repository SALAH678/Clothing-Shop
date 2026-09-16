import { Link, useParams } from "react-router-dom";
import { type Product } from "../types/Product";
import { resolveImageUrl } from "../../../lib/imageUrl";
import { formatPriceDA, getPrice } from "../../../lib/pricing";

export default function ProductItem({ product }: { product: Product }) {
  const { categoryName } = useParams<{ categoryName: string }>();
  const productPath = `/categories/${encodeURIComponent(categoryName ?? "all")}/${product.id}`;
  const mainImage = product.images.find((image) => image.isMain) ?? product.images[0];
  const mainImageUrl = resolveImageUrl(mainImage?.imageUrl);

  return (
    <Link
      to={productPath}
      className="group relative bg-surface border border-primary flex flex-col h-full transition-all duration-300 hover:shadow-2xl hover:shadow-black/50 hover:-translate-y-1 active:scale-95 active:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
    >
      <div className="relative aspect-[0.8] overflow-hidden border-b border-primary bg-surface-container">
        {mainImageUrl ? (
          <img
            src={mainImageUrl}
            alt={product.name}
            loading="lazy"
            decoding="async"
            className="w-full h-full object-cover transition-transform duration-500 ease-in-out group-hover:scale-105"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center font-mono text-[10px] text-zinc-400">
            NO IMG
          </div>
        )}
      </div>
      <div className="p-4 flex flex-col grow justify-between">
        <h3 className="font-mono text-sm uppercase line-clamp-2 font-bold">{product.name}</h3>
        <span className="font-mono text-sm mt-2 text-secondary">
          {formatPriceDA(getPrice(product.basePrice, product.discount))}
        </span>
      </div>
    </Link>
  );
}
