import { type Category } from "../types/Category";
import { Link } from "react-router-dom";

interface CategoryItemProps {
  category: Category;
}

export default function CategoryItem({ category }: CategoryItemProps) {
  function createSlug(name: string) {
    return name.toLowerCase().trim().replace(/\s+/g, "-");
  }

  const backEndUrl: string = import.meta.env.VITE_API_URL ?? "";

  return (
    <Link
      to={`/categories/${createSlug(category.categoryName)}`}
      className="group relative block aspect-4/5 border border-primary bg-surface overflow-hidden transition-all duration-300 hover:shadow-2xl hover:shadow-black/50 hover:-translate-y-1 active:scale-95 active:shadow-md"
    >
      <div
        className="absolute inset-0 bg-cover bg-center w-full h-full grayscale contrast-125 transition-all duration-500 ease-cubic-bezier(0.4,0,0.2,1) group-hover:grayscale-0 group-hover:contrast-110 group-hover:scale-105"
        style={{ backgroundImage: `url("${backEndUrl}${category.imageUrl}")` }}
      />
      <div className="absolute inset-0 bg-black/50 mix-blend-multiply group-hover:bg-black/30 transition-colors duration-300" />
      <div className="absolute inset-0 flex items-center justify-center p-4">
        <h2 className="font-display text-lg md:text-xl font-bold text-white uppercase text-center tracking-widest drop-shadow-lg z-10 relative">
          {category.categoryName}
        </h2>
      </div>
    </Link>
  );
}
