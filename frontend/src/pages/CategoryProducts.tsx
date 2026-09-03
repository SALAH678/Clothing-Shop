import { useSearchParams } from "react-router-dom";
import { useParams } from "react-router-dom";
import Filter from "../components/products/Filters";
import Products from "../features/products/components/Products";
import { useProducts } from "../features/products/hooks/useProducts";
import { useCategories } from "../features/categories/Hooks/useCategories";
import EmptyState from "../components/ui/EmptyState";
import ErrorState from "../components/ui/ErrorState";
import ProductsSkeleton from "../features/products/components/ProductsSkeleton";

export default function CategoryProducts() {
  const { categoryName } = useParams<{ categoryName: string }>();
  const {
    data: categories,
    isPending: categoriesPending,
    isError: categoriesError,
    refetch: refetchCategories,
  } = useCategories();
  const categorySlug = decodeURIComponent(categoryName ?? "").toLowerCase();
  const categoryId = categories?.find(
    (category) => category.categoryName.toLowerCase().replace(/\s+/g, "-") === categorySlug,
  )?.id;

  const [searchParams, setSearchParams] = useSearchParams();

  const sort = searchParams.get("sort") ?? "newest";
  const sizes = searchParams.get("sizes");
  const colors = searchParams.get("colors");
  const minPrice = searchParams.get("minPrice");
  const maxPrice = searchParams.get("maxPrice");

  const updateFilters = (values: {
    sizes: string[] | null;
    colors: string[] | null;
    minPrice: number | null;
    maxPrice: number | null;
  }) => {
    const next = new URLSearchParams(searchParams);
    if (values.sizes?.length) next.set("sizes", values.sizes.join(","));
    else next.delete("sizes");
    if (values.colors?.length) next.set("colors", values.colors.join(","));
    else next.delete("colors");
    if (values.minPrice != null) next.set("minPrice", String(values.minPrice));
    else next.delete("minPrice");
    if (values.maxPrice != null) next.set("maxPrice", String(values.maxPrice));
    else next.delete("maxPrice");
    setSearchParams(next);
  };

  const onSortChange = (value: string) => {
    const next = new URLSearchParams(searchParams);
    next.set("sort", value);
    setSearchParams(next);
  };

  const {
    data: products,
    isPending,
    isError,
    refetch,
  } = useProducts(
    {
      categoryId,
      minPrice: minPrice ? Number(minPrice) : undefined,
      maxPrice: maxPrice ? Number(maxPrice) : undefined,
      sizes: sizes ? sizes.split(",") : undefined,
      colors: colors ? colors.split(",") : undefined,
      sortBy: sort === "price-low-high" || sort === "price-high-low" ? "price" : undefined,
      descending: sort === "price-high-low",
      pageNumber: 1,
      pageSize: 10,
    },
    Boolean(categoryId),
  );

  if (categoriesPending) return <ProductsSkeleton />;

  if (categoriesError)
    return (
      <ErrorState
        label="Error 503 / Categories unavailable"
        message="We could not determine this collection. Please try again in a moment."
        onRetry={refetchCategories}
      />
    );

  if (!categoryId)
    return <EmptyState title="Collection not found" message="The requested collection does not exist." />;

  if (isPending) return <ProductsSkeleton />;

  if (isError)
    return (
      <ErrorState
        label="Error 503 / Products unavailable"
        message="We could not load the latest products. Please try again in a moment."
        onRetry={refetch}
      />
    );

  const displayedProducts = products?.items ?? [];
  const filteredProductsCount = products?.totalCount ?? displayedProducts.length;

  return (
    <div className="flex flex-col md:flex-row min-h-[calc(100vh-80px)]">
      <Filter
        key={`${sizes ?? ""}-${colors ?? ""}-${minPrice ?? ""}-${maxPrice ?? ""}`}
        initialValues={{
          sizes: sizes ? sizes.split(",") : null,
          colors: colors ? colors.split(",") : null,
          minPrice: minPrice ? Number(minPrice) : null,
          maxPrice: maxPrice ? Number(maxPrice) : null,
        }}
        onApply={updateFilters}
      />
      <Products
        products={displayedProducts}
        productsCount={filteredProductsCount}
        pageSize={10}
        sort={sort}
        onSortChange={onSortChange}
      />
    </div>
  );
}
