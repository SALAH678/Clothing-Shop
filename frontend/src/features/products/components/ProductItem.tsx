import { type Product } from "../types/Product";

export default function ProductItem({ product }: { product: Product }) {
  const backEndUrl: string = import.meta.env.VITE_API_URL ?? "";
  const mainImage = product.images.find((image) => image.isMain);
  console.log("ProductItem mainImage:", mainImage); // Debugging line

  const mainImageUrl = mainImage?.imageUrl
    ? /^https?:\/\//i.test(mainImage.imageUrl)
      ? mainImage.imageUrl
      : `${backEndUrl.replace(/\/$/, "")}/${mainImage.imageUrl.replace(/^\//, "")}`
    : undefined;

  console.log("ProductItem mainImageUrl:", mainImageUrl); // Debugging line

  return (
    <article className="group relative bg-surface border border-primary flex flex-col h-full transition-all duration-300 hover:shadow-2xl hover:shadow-black/50 hover:-translate-y-1 active:scale-95 active:shadow-md cursor-pointer">
      <div className="relative aspect-[0.8] overflow-hidden border-b border-primary bg-surface-container">
        <img
          src={mainImageUrl}
          alt={product.name}
          className="w-full h-full object-cover grayscale contrast-125 transition-all duration-500 ease-in-out group-hover:grayscale-0 group-hover:contrast-110 group-hover:scale-105"
        />
      </div>
      <div className="p-4 flex flex-col grow justify-between">
        <h3 className="font-mono text-sm uppercase line-clamp-2 font-bold">{product.name}</h3>
        <span className="font-mono text-sm mt-2 text-secondary">{product.basePrice}</span>
      </div>
    </article>
  );
}
