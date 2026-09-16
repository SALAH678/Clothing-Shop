import { useDeferredValue, useMemo, type ReactNode } from "react";
import { useSearchParams } from "react-router-dom";
import { useParams } from "react-router-dom";
import Filters, { type FilterValues } from "../components/products/Filters";
import Products from "../features/products/components/Products";
import { MAX_PAGES, useInfiniteProducts } from "../features/products/hooks/useProducts";
import { useCategories } from "../features/categories/hooks/useCategories";
import { createSlug } from "../components/ui/Slug";
import EmptyState from "../components/ui/EmptyState";
import ErrorState from "../components/ui/ErrorState";
import ProductsSkeleton from "../features/products/components/ProductsSkeleton";

const PAGE_SIZE = 12;

export default function CategoryProducts() {
  const { categoryName } = useParams<{ categoryName: string }>();
  const {
    data: categories,
    isPending: categoriesPending,
    isError: categoriesError,
    refetch: refetchCategories,
  } = useCategories();
  const categorySlug = decodeURIComponent(categoryName ?? "").toLowerCase();
  const isAllProducts = categorySlug === "all";

  const categoryId = categories?.find(
    (category) => createSlug(category.categoryName) === categorySlug,
  )?.id;

  const [searchParams, setSearchParams] = useSearchParams();

  const rawSearch = searchParams.get("search")?.trim() || undefined;
  // Defer query while user types so each keystroke doesn't fire a request.
  const search = useDeferredValue(rawSearch);
  const effectiveCategoryId = isAllProducts ? undefined : categoryId;

  const sort = searchParams.get("sort") ?? "newest";
  const sizesParam = searchParams.get("sizes");
  const colorsParam = searchParams.get("colors");
  const minPriceParam = searchParams.get("minPrice");
  const maxPriceParam = searchParams.get("maxPrice");

  const filterValues: FilterValues = useMemo(
    () => ({
      sizes: sizesParam ? sizesParam.split(",") : null,
      colors: colorsParam ? colorsParam.split(",") : null,
      minPrice: minPriceParam ? Number(minPriceParam) : null,
      maxPrice: maxPriceParam ? Number(maxPriceParam) : null,
    }),
    [sizesParam, colorsParam, minPriceParam, maxPriceParam],
  );

  const updateFilters = (values: FilterValues) => {
    const next = new URLSearchParams(searchParams);
    if (values.sizes?.length) next.set("sizes", values.sizes.join(","));
    else next.delete("sizes");
    if (values.colors?.length) next.set("colors", values.colors.join(","));
    else next.delete("colors");
    if (values.minPrice != null && Number.isFinite(values.minPrice)) next.set("minPrice", String(values.minPrice));
    else next.delete("minPrice");
    if (values.maxPrice != null && Number.isFinite(values.maxPrice)) next.set("maxPrice", String(values.maxPrice));
    else next.delete("maxPrice");
    setSearchParams(next);
  };

  const onSortChange = (value: string) => {
    const next = new URLSearchParams(searchParams);
    next.set("sort", value);
    setSearchParams(next);
  };

  const enabled = isAllProducts || Boolean(categoryId);

  const { data, isPending, isError, isFetchingNextPage, fetchNextPage, hasNextPage, refetch } =
    useInfiniteProducts(
      {
        categoryId: effectiveCategoryId,
        search,
        minPrice: minPriceParam ? Number(minPriceParam) : undefined,
        maxPrice: maxPriceParam ? Number(maxPriceParam) : undefined,
        sizes: sizesParam ? sizesParam.split(",") : undefined,
        colors: colorsParam ? colorsParam.split(",") : undefined,
        sortBy: sort === "price-low-high" || sort === "price-high-low" ? "price" : undefined,
        descending: sort === "price-high-low",
        pageSize: PAGE_SIZE,
      },
      enabled,
    );

  const pages = data?.pages ?? [];
  const products = pages.flatMap((page) => page.items ?? []);
  const totalCount = pages[pages.length - 1]?.totalCount ?? products.length;
  // Backend stops us at MAX_PAGES even if more pages exist server-side.
  const hasReachedCap = !hasNextPage && pages.length >= MAX_PAGES && totalCount > products.length;
  const showSkeleton = isPending && products.length === 0;

  // Resolve the results column separately so the <Filters> sidebar can be
  // rendered unconditionally. It used to be skipped by three early returns,
  // which unmounted the sidebar and silently discarded the user's selection
  // whenever categories were still loading or failing.
  let results: ReactNode;
  if (categoriesError && !isAllProducts) {
    results = (
      <div className="grow flex items-center justify-center">
        <ErrorState
          label="Error 503 / Categories unavailable"
          message="We could not determine this collection. Please try again in a moment."
          onRetry={refetchCategories}
        />
      </div>
    );
  } else if (!isAllProducts && !categoryId) {
    results = (
      <div className="grow flex items-center justify-center">
        <EmptyState title="Collection not found" message="The requested collection does not exist." />
      </div>
    );
  } else if ((categoriesPending || showSkeleton) && products.length === 0) {
    results = <ProductsSkeleton />;
  } else if (isError) {
    results = (
      <div className="grow flex items-center justify-center">
        <ErrorState
          label="Error 503 / Products unavailable"
          message="We could not load the latest products. Please try again in a moment."
          onRetry={refetch}
        />
      </div>
    );
  } else {
    results = (
      <Products
        products={products}
        productsCount={totalCount}
        sort={sort}
        onSortChange={onSortChange}
        onShowMore={() => fetchNextPage()}
        isLoadingMore={isFetchingNextPage}
        hasNextPage={hasNextPage}
        hasReachedCap={hasReachedCap}
      />
    );
  }

  return (
    <div className="flex flex-col md:flex-row min-h-[calc(100vh-80px)]">
      {/* Always mounted, so applied filters survive "Show More", refetches and
          loading/error states. "Show More" only extends the query cache and
          never rewrites the URL, so the applied filters stay in place. */}
      <Filters values={filterValues} onApply={updateFilters} />
      {results}
    </div>
  );
}
