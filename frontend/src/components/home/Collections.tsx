import type { Category } from "../../features/categories/types/Category";
import { Link } from "react-router-dom";
import { resolveImageUrl } from "../../lib/imageUrl";
import { createSlug } from "../ui/Slug";

interface CollectionsProps {
  categories: Category[] | undefined;
}

export default function Collections({ categories }: CollectionsProps) {
  if (!categories || categories.length === 0)
    return (
      <div className="w-full py-12 text-center">
        <p className="font-mono text-xs uppercase tracking-widest text-zinc-500">No collections available.</p>
      </div>
    );

  const featuredLayout: Record<string, string> = {
    "T-Shirt Oversize": "md:col-span-8 h-150",
    "Ultra Baggy": "md:col-span-4 h-150",
    "Old Money Shirts": "md:col-span-6 h-125",
    "Jogger And Jogging": "md:col-span-6 h-125",
  };

  const collectionCards = Object.keys(featuredLayout)
    .map((name) => categories.find((category) => category.categoryName === name))
    .filter((category): category is Category => Boolean(category?.imageUrl));

  return (
    <section id="collections" className="py-24 px-6 md:px-16 bg-surface max-w-[1920px] mx-auto">
      <div className="mb-12 flex flex-col md:flex-row justify-between items-start md:items-end border-b border-primary pb-4 gap-4">
        <h2 className="font-display text-4xl md:text-5xl font-bold uppercase text-primary tracking-tighter">
          Collections
        </h2>
        <Link
          to="/categories"
          className="font-mono text-sm font-bold tracking-widest uppercase hover:opacity-70 transition-opacity"
        >
          Shop All
        </Link>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-12 gap-6 auto-rows-min">
        {collectionCards.map((category) => {
          const slug = createSlug(category.categoryName);
          return (
            <Link
              to={`/categories/${slug}`}
              key={category.id}
              className={`group relative block ${featuredLayout[category.categoryName]} border border-primary overflow-hidden bg-surface-container transition-all duration-300 hover:shadow-2xl hover:shadow-black/50 hover:-translate-y-1`}
            >
              <img
                src={resolveImageUrl(category.imageUrl)}
                alt={category.categoryName}
                loading="lazy"
                decoding="async"
                className="w-full h-full object-cover grayscale group-hover:grayscale-0 transition-all duration-700 scale-100 group-hover:scale-105"
              />
              <div className="absolute inset-0 bg-linear-to-t from-black/80 via-transparent to-transparent opacity-90" />
              <div className="absolute bottom-8 left-8">
                <h3 className="font-display text-2xl md:text-3xl font-bold text-white uppercase mb-2 tracking-tight">
                  {category.categoryName}
                </h3>
              </div>
            </Link>
          );
        })}
      </div>
    </section>
  );
}
