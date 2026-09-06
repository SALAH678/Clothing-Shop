import Sort from "../../../components/products/Sort";
import type { Product } from "../types/Product";
import ProductItem from "./ProductItem";

export default function Products({
  products,
  productsCount,
  currentPage,
  totalPages,
  sort,
  onSortChange,
  onShowMore,
}: {
  products: Product[];
  productsCount: number;
  currentPage: number;
  totalPages: number;
  sort: string;
  onSortChange: (value: string) => void;
  onShowMore?: () => void;
}) {
  return (
    <section className="grow flex flex-col">
      <Sort count={productsCount} value={sort} onChange={onSortChange} />
      {products.length === 0 ? (
        <div className="flex-1 flex flex-col items-center justify-center p-12 text-center bg-surface-container-lowest">
          <p className="font-mono text-sm uppercase text-secondary tracking-widest mb-2">No products found</p>
          <p className="font-body text-xs text-secondary/70">Try adjusting your search terms or filters.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 p-4 md:p-6 bg-surface-container-lowest">
          {products.map((product) => (
            <ProductItem key={product.id} product={product} />
          ))}
        </div>
      )}
      {currentPage < totalPages && (
        <div className="flex justify-center p-6 md:p-8 bg-surface-container-lowest">
          <button
            onClick={onShowMore}
            className="bg-primary text-on-primary font-mono text-sm uppercase py-4 px-10 border border-primary tracking-widest font-bold shadow-xl transition-all duration-300 ease-out hover:-translate-y-2 hover:shadow-2xl hover:bg-white hover:text-black active:translate-y-1 active:scale-95 active:shadow-md"
          >
            Show More
          </button>
        </div>
      )}
    </section>
  );
}
