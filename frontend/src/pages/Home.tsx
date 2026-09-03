import Hero from "../components/home/Hero";
import Collections from "../components/home/Collections";
import Features from "../components/home/Features";
import StoreLocation from "../components/home/StoreLocation";
import { useCategories } from "../features/categories/Hooks/useCategories";
import CategoriesSkeleton from "../features/categories/components/CategoriesSkeleton";
import ErrorState from "../components/ui/ErrorState";
import EmptyState from "../components/ui/EmptyState";

export default function Home() {
  const { data: categories, isPending, isError, refetch } = useCategories();

  if (isPending) {
    return <CategoriesSkeleton />;
  }

  if (isError) {
    return (
      <ErrorState
        label="Error 503 / Categories unavailable"
        message="We could not load the latest collections. Please try again in a moment."
        onRetry={refetch}
      />
    );
  }

  if (!categories?.length) {
    return <EmptyState title="No collections available" message="There are no collections available at the moment." />;
  }

  return (
    <>
      <Hero />
      <Collections categories={categories} />
      <Features />
      <StoreLocation />
    </>
  );
}
