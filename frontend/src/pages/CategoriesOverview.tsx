import CategoryItem from "../features/categories/components/CategoryItem";
import { useCategories } from "../features/categories/hooks/useCategories";
import EmptyState from "../components/ui/EmptyState";
import ErrorState from "../components/ui/ErrorState";
import Skeleton from "../components/ui/Skeleton";

export default function CategoriesOverview() {
  const { data: categories, isPending, isError, refetch } = useCategories();

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

      {isPending ? (
        <div
          aria-busy="true"
          aria-label="Loading categories"
          className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-4 lg:gap-6"
        >
          {Array.from({ length: 10 }, (_, index) => (
            <Skeleton key={index} className="aspect-4/5 border border-primary" />
          ))}
        </div>
      ) : isError ? (
        <ErrorState
          label="Error 503 / Categories unavailable"
          message="We could not load the collections. Please try again in a moment."
          onRetry={refetch}
        />
      ) : !categories || categories.length === 0 ? (
        <EmptyState title="No collections yet" message="New collections will appear here as soon as they are added." />
      ) : (
        <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-4 lg:gap-6">
          {categories.map((category) => (
            <CategoryItem key={category.id} category={category} />
          ))}
        </div>
      )}
    </section>
  );
}
