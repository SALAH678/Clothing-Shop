import Skeleton from "../../../components/ui/Skeleton";

export default function CategoriesSkeleton() {
  return (
    <main aria-busy="true" aria-label="Loading categories" className="w-full">
      <section className="relative h-[80vh] min-h-105 overflow-hidden border-b border-primary bg-surface-container">
        <div className="absolute inset-0 animate-pulse bg-surface-container-high" />

        <div className="relative z-10 flex h-full flex-col items-center justify-center gap-6 px-6">
          <Skeleton className="h-16 w-64 md:h-24 md:w-105" />
          <Skeleton className="h-12 w-40" />
        </div>
      </section>

      <section className="px-6 py-16 md:px-16 md:py-24">
        <div className="mb-12 flex items-end justify-between border-b border-primary pb-4">
          <Skeleton className="h-12 w-56 md:h-14 md:w-72" />
          <Skeleton className="hidden h-4 w-24 md:block" />
        </div>

        <div className="grid grid-cols-1 gap-6 md:grid-cols-12">
          <Skeleton className="h-100 md:col-span-8" />
          <Skeleton className="h-100 md:col-span-4" />
          <Skeleton className="h-80 md:col-span-6" />
          <Skeleton className="h-80 md:col-span-6" />
        </div>
      </section>

      <section className="grid grid-cols-1 border-y border-primary md:grid-cols-3">
        {Array.from({ length: 3 }, (_, index) => (
          <div
            key={index}
            className="flex h-52 items-center justify-center border-b border-primary bg-surface-container-lowest last:border-b-0 md:border-b-0 md:border-r md:last:border-r-0"
          >
            <Skeleton className="h-16 w-48" />
          </div>
        ))}
      </section>
    </main>
  );
}