import CategoryItem from "../features/categories/components/CategoryItem";
import { useCategories } from "../features/categories/Hooks/useCategories";

export default function CategoriesOverview() {
  const { data: categories } = useCategories();

  return (
    <section className="grow w-full max-w-[1920px] mx-auto px-4 md:px-6 lg:px-8 py-12 lg:py-20">
      <div className="mb-12 lg:mb-20 text-center border-b border-primary pb-8">
        <h1 className="font-display text-4xl md:text-5xl lg:text-7xl font-extrabold uppercase text-primary mb-4 tracking-tighter drop-shadow-[4px_4px_0_rgba(0,0,0,0.2)]">
          CATEGORIES OVERVIEW
        </h1>
        <p className="font-mono text-xs md:text-sm text-secondary uppercase tracking-widest drop-shadow-[2px_2px_0_rgba(0,0,0,0.2)]">
          Explore all collections
        </p>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-4 lg:gap-6">
        {categories?.map((category) => (
          <CategoryItem key={category.id} category={category} />
        ))}
      </div>
    </section>
  );
}
