import Skeleton from "../../../components/ui/Skeleton";

export default function ProductsSkeleton() {
  return (
    <main aria-busy="true" aria-label="Loading products" className="w-full grow flex flex-col">
      <section className="flex items-center justify-between border-b border-primary px-4 md:px-6 py-4">
        <Skeleton className="h-6 w-40" />
        <Skeleton className="h-4 w-24 hidden md:block" />
      </section>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 p-4 md:p-6 bg-surface-container-lowest">
        {Array.from({ length: 8 }, (_, index) => (
          <article key={index} className="flex flex-col h-full border border-primary bg-surface">
            <Skeleton className="aspect-[0.8] w-full" />
            <div className="p-4 flex flex-col grow justify-between gap-2">
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-4 w-1/3" />
            </div>
          </article>
        ))}
      </div>
    </main>
  );
}
