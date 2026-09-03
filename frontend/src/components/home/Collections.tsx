import type { Category } from "../../features/categories/types/Category";
import { Link } from "react-router-dom";

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

  const backEndUrl = import.meta.env.VITE_API_URL ?? "";

  const oversizeTShirtCategory: Category | undefined = categories?.find(
    (category) => category.categoryName === "T-Shirt Oversize",
  );
  const ultraBaggyCategory: Category | undefined = categories?.find(
    (category) => category.categoryName === "Ultra Baggy",
  );
  const shirtsCategory: Category | undefined = categories?.find(
    (category) => category.categoryName === "Old Money Shirts",
  );
  const joggersAndJoggingCategory: Category | undefined = categories?.find(
    (category) => category.categoryName === "Jogger And Jogging",
  );

  const collectionCards = [
    { category: oversizeTShirtCategory, className: "md:col-span-8 h-150", alt: "Oversize T-Shirt" },
    { category: ultraBaggyCategory, className: "md:col-span-4 h-150", alt: "Ultra Baggy" },
    { category: shirtsCategory, className: "md:col-span-6 h-125", alt: "Shirts" },
    { category: joggersAndJoggingCategory, className: "md:col-span-6 h-125", alt: "Joggers and Jogging" },
  ].filter((card) => card.category?.imageUrl);

  const getImageUrl = (imageUrl: string): string => {
    if (/^https?:\/\//i.test(imageUrl)) return imageUrl;

    return `${backEndUrl.replace(/\/$/, "")}/${imageUrl.replace(/^\//, "")}`;
  };

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
        {collectionCards.map(({ category, className, alt }) => (
          <Link
            to={`/categories/${category?.categoryName.toLowerCase().replace(/\s+/g, "-")}`}
            key={category?.id}
            className={`group relative block ${className} border border-primary overflow-hidden bg-surface-container transition-all duration-300 hover:shadow-2xl hover:shadow-black/50 hover:-translate-y-1`}
          >
            <img
              src={getImageUrl(category?.imageUrl ?? "")}
              alt={alt}
              className="w-full h-full object-cover grayscale group-hover:grayscale-0 transition-all duration-700 scale-100 group-hover:scale-105"
            />
            <div className="absolute inset-0 bg-linear-to-t from-black/80 via-transparent to-transparent opacity-90" />
            <div className="absolute bottom-8 left-8">
              <h3 className="font-display text-2xl md:text-3xl font-bold text-white uppercase mb-2 tracking-tight">
                {category?.categoryName}
              </h3>
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}
