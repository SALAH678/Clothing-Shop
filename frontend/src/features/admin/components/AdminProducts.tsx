import React, { useMemo, useState, type FormEvent } from "react";
import {
  X,
  Layers,
  RefreshCw,
  Search,
  ChevronLeft,
  ChevronRight,
  Image as ImageIcon,
  PackageOpen,
  Plus,
  Edit,
  Trash2,
} from "lucide-react";
import { useAdminProducts } from "../hooks/useAdminQueries";
import { useCreateVariant, useDeleteVariant, useUpdateVariant } from "../hooks/useAdminVariantMutations";
import { useDeleteProduct, useUpdateProduct } from "../hooks/useAdminProductMutations";
import type { ProductInput } from "../api/adminProductApi";
import type { AdminProductFilters } from "../api/adminApi";
import { useCategories } from "../../categories/hooks/useCategories";
import { formatPriceDA, getPrice } from "../../../lib/pricing";
import { resolveImageUrl } from "../../../lib/imageUrl";
import ConfirmDialog from "../../../components/ui/ConfirmDialog";
import type { Product, Variant } from "../../products/types/Product";
import { getApiErrorMessage } from "../utils/apiErrors";
import { SIZE_SUGGESTIONS, COLOR_SUGGESTIONS, getColorHex } from "../constants/variantOptions";
import CreateProductForm from "./CreateProductForm";

/** Form draft for the variant editor — stock is a string so the input can be empty. */
interface VariantDraft {
  size: string;
  color: string;
  stockQuantity: string;
}

const EMPTY_VARIANT_DRAFT: VariantDraft = { size: "", color: "", stockQuantity: "0" };

const shortId = (id: string) => id.slice(0, 8).toUpperCase();

const sumStock = (variants: Variant[]) => variants.reduce((sum, variant) => sum + variant.stockQuantity, 0);

interface ProductDraft {
  name: string;
  description: string;
  basePrice: string;
  discount: string;
  categoryId: string;
}

const EMPTY_PRODUCT_DRAFT: ProductDraft = {
  name: "",
  description: "",
  basePrice: "",
  discount: "",
  categoryId: "",
};

/** Sort dropdown options mapped to the API's sortBy / descending params.
 * With no sortBy, the API orders by creation date: descending = newest first. */
type SortOption = "newest" | "oldest" | "price-low-high" | "price-high-low";

const SORT_OPTIONS: Record<SortOption, { sortBy?: string; descending: boolean }> = {
  newest: { sortBy: undefined, descending: true },
  oldest: { sortBy: undefined, descending: false },
  "price-low-high": { sortBy: "price", descending: false },
  "price-high-low": { sortBy: "price", descending: true },
};

export const AdminProducts: React.FC = () => {
  const [page, setPage] = useState(1);

  // Filter drafts (what the user is typing/selecting) vs applied filters (what
  // the query uses). Nothing hits the API until the Apply button is clicked.
  const [searchInput, setSearchInput] = useState("");
  const [sortInput, setSortInput] = useState<SortOption>("newest");
  const [minPriceInput, setMinPriceInput] = useState("");
  const [maxPriceInput, setMaxPriceInput] = useState("");
  const [search, setSearch] = useState("");
  const [sort, setSort] = useState<SortOption>("newest");
  const [minPrice, setMinPrice] = useState<number | undefined>(undefined);
  const [maxPrice, setMaxPrice] = useState<number | undefined>(undefined);
  const [priceError, setPriceError] = useState<string | null>(null);

  // Applied filters for the query; sortBy/descending derived from the applied sort.
  const sortConfig = SORT_OPTIONS[sort];
  const filters = useMemo<AdminProductFilters>(
    () => ({
      search: search || undefined,
      minPrice,
      maxPrice,
      sortBy: sortConfig.sortBy,
      descending: sortConfig.descending,
    }),
    [search, minPrice, maxPrice, sortConfig],
  );

  const { data, isPending, isError, isFetching, refetch } = useAdminProducts(page, filters);
  const { data: categories } = useCategories();

  // Applies the search / sort / price drafts to the query
  // (Apply button, or Enter in any filter field).
  const handleApplyFilters = (event: FormEvent) => {
    event.preventDefault();

    const parsePrice = (raw: string) => {
      const trimmed = raw.trim();
      if (!trimmed) return undefined;
      const value = Number(trimmed);
      return Number.isFinite(value) && value >= 0 ? value : NaN;
    };

    const nextMin = parsePrice(minPriceInput);
    const nextMax = parsePrice(maxPriceInput);
    if (Number.isNaN(nextMin) || Number.isNaN(nextMax)) {
      setPriceError("Prices must be numbers of 0 or more.");
      return;
    }
    if (nextMin !== undefined && nextMax !== undefined && nextMin > nextMax) {
      setPriceError("Min price cannot be greater than max price.");
      return;
    }

    setPriceError(null);
    setSearch(searchInput.trim());
    setSort(sortInput);
    setMinPrice(nextMin);
    setMaxPrice(nextMax);
    setPage(1);
  };

  // Resets the drafts and applied filters back to their defaults.
  const clearFilters = () => {
    setSearchInput("");
    setSortInput("newest");
    setMinPriceInput("");
    setMaxPriceInput("");
    setSearch("");
    setSort("newest");
    setMinPrice(undefined);
    setMaxPrice(undefined);
    setPriceError(null);
    setPage(1);
  };

  const hasActiveFilters =
    Boolean(search) || minPrice !== undefined || maxPrice !== undefined || sort !== "newest";

  const createVariant = useCreateVariant();
  const updateVariant = useUpdateVariant();
  const deleteVariant = useDeleteVariant();
  const updateProduct = useUpdateProduct();
  const deleteProduct = useDeleteProduct();

  const [isCreateOpen, setIsCreateOpen] = useState(false);

  // Store the id (not the product object) so the modal always renders the
  // freshest data from the query cache after a variant mutation invalidates it.
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [variantEditor, setVariantEditor] = useState<{ mode: "create" } | { mode: "edit"; variant: Variant } | null>(
    null,
  );
  const [variantDraft, setVariantDraft] = useState<VariantDraft>(EMPTY_VARIANT_DRAFT);
  const [variantError, setVariantError] = useState<string | null>(null);
  const [variantToDelete, setVariantToDelete] = useState<Variant | null>(null);
  const [productEditor, setProductEditor] = useState<"edit" | null>(null);
  const [productEditorId, setProductEditorId] = useState<string | null>(null);
  const [productDraft, setProductDraft] = useState<ProductDraft>(EMPTY_PRODUCT_DRAFT);
  const [productError, setProductError] = useState<string | null>(null);
  const [productToDelete, setProductToDelete] = useState<Product | null>(null);

  const products = data?.items ?? [];
  const totalPages = Math.max(1, data?.totalPages ?? 1);
  const totalCount = data?.totalCount ?? 0;

  const selectedProduct = products.find((product) => product.id === selectedProductId) ?? null;
  const isVariantMutating = createVariant.isPending || updateVariant.isPending || deleteVariant.isPending;
  const isProductMutating = updateProduct.isPending || deleteProduct.isPending;

  const categoryName = (categoryId: string) =>
    categories?.find((category) => category.id === categoryId)?.categoryName ?? "Uncategorized";

  const openProductEditor = (product: Product) => {
    setProductError(null);
    setProductEditor("edit");
    setProductEditorId(product.id);
    setProductDraft({
      name: product.name,
      description: product.description ?? "",
      basePrice: String(product.basePrice),
      discount: product.discount == null ? "" : String(product.discount),
      categoryId: product.categoryId,
    });
  };

  const handleProductSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!productEditorId) return;

    const basePrice = Number(productDraft.basePrice);
    const discount = productDraft.discount.trim() ? Number(productDraft.discount) : undefined;
    const input: ProductInput = {
      name: productDraft.name.trim(),
      description: productDraft.description.trim() || undefined,
      basePrice,
      discount,
      categoryId: productDraft.categoryId,
    };

    if (!input.name || !input.categoryId || !Number.isFinite(basePrice) || basePrice < 0) {
      setProductError("Name, category, and a valid price are required.");
      return;
    }
    if (discount !== undefined && (!Number.isFinite(discount) || discount < 0 || discount > basePrice)) {
      setProductError("Discount must be between 0 and the base price.");
      return;
    }

    setProductError(null);
    try {
      await updateProduct.mutateAsync({ productId: productEditorId, input });
      setProductEditor(null);
      setProductEditorId(null);
      setProductDraft(EMPTY_PRODUCT_DRAFT);
      setSelectedProductId(null);
    } catch (error) {
      setProductError(getApiErrorMessage(error, "Failed to update product."));
    }
  };

  const handleConfirmDeleteProduct = async () => {
    if (!productToDelete) return;
    try {
      await deleteProduct.mutateAsync({ productId: productToDelete.id });
      setProductToDelete(null);
    } catch (error) {
      setProductToDelete(null);
      setProductError(getApiErrorMessage(error, "Failed to delete product."));
    }
  };

  // Suggestions: sizes/colors already used by this product first, then the defaults.
  const variantSizeSuggestions = Array.from(
    new Set([...(selectedProduct?.variants ?? []).map((variant) => variant.size), ...SIZE_SUGGESTIONS]),
  );
  const variantColorSuggestions = Array.from(
    new Set([...(selectedProduct?.variants ?? []).map((variant) => variant.color), ...COLOR_SUGGESTIONS]),
  );

  const closeVariantsModal = () => {
    setSelectedProductId(null);
    setVariantEditor(null);
    setVariantDraft(EMPTY_VARIANT_DRAFT);
    setVariantError(null);
    setVariantToDelete(null);
  };

  const openVariantEditor = (variant?: Variant) => {
    setVariantError(null);
    if (variant) {
      setVariantEditor({ mode: "edit", variant });
      setVariantDraft({
        size: variant.size,
        color: variant.color,
        stockQuantity: String(variant.stockQuantity),
      });
      return;
    }
    setVariantEditor({ mode: "create" });
    setVariantDraft(EMPTY_VARIANT_DRAFT);
  };

  const handleVariantSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!selectedProduct || !variantEditor) return;

    const size = variantDraft.size.trim();
    const color = variantDraft.color.trim();
    const stockQuantity = Number(variantDraft.stockQuantity);
    setVariantError(null);

    if (!size || !color) {
      setVariantError("Size and color are required.");
      return;
    }
    if (!Number.isInteger(stockQuantity) || stockQuantity < 0) {
      setVariantError("Stock quantity must be a whole number of 0 or more.");
      return;
    }

    try {
      if (variantEditor.mode === "create") {
        await createVariant.mutateAsync({ productId: selectedProduct.id, size, color, stockQuantity });
      } else {
        await updateVariant.mutateAsync({
          variantId: variantEditor.variant.id,
          size,
          color,
          stockQuantity,
        });
      }
      setVariantEditor(null);
      setVariantDraft(EMPTY_VARIANT_DRAFT);
    } catch (error) {
      setVariantError(
        getApiErrorMessage(
          error,
          variantEditor.mode === "create" ? "Failed to add variant." : "Failed to update variant.",
        ),
      );
    }
  };

  const requestDeleteVariant = (variant: Variant) => {
    setVariantError(null);
    setVariantToDelete(variant);
  };

  const handleConfirmDeleteVariant = async () => {
    if (!variantToDelete) return;
    setVariantError(null);
    try {
      await deleteVariant.mutateAsync({ variantId: variantToDelete.id });
    } catch (error) {
      setVariantError(getApiErrorMessage(error, "Failed to delete variant."));
    } finally {
      setVariantToDelete(null);
    }
  };

  return (
    <div className="flex flex-col gap-6">
      {/* Products Tab Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white border-4 border-primary p-4 shadow-[6px_6px_0_0_#000]">
        <div className="flex items-center gap-3">
          <span className="font-mono text-sm font-bold uppercase text-secondary">Total Products:</span>
          <span className="font-mono font-bold text-xs bg-primary text-white px-3 py-1 border border-primary">
            {isPending ? "…" : `${totalCount} items`}
          </span>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setIsCreateOpen(true)}
            className="bg-primary text-white font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-primary flex items-center gap-2 shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white hover:text-primary transition-all cursor-pointer"
          >
            <Plus className="w-4 h-4" /> Add Product
          </button>
          <button
            type="button"
            onClick={() => void refetch()}
            disabled={isFetching}
            className="bg-surface text-primary font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-primary flex items-center gap-2 shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white transition-all cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
          >
            <RefreshCw className={`w-4 h-4 ${isFetching ? "animate-spin" : ""}`} /> Refresh
          </button>
        </div>
      </div>

      {/* Filters: search, sort, price range */}
      <form
        onSubmit={handleApplyFilters}
        className="bg-white border-4 border-primary p-4 shadow-[6px_6px_0_0_#000] flex flex-col lg:flex-row gap-4 lg:items-end"
      >
        <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase text-secondary lg:flex-1">
          Search
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-secondary pointer-events-none" />
            <input
              type="text"
              maxLength={100}
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder="Search by product name…"
              className="w-full border-2 border-primary bg-surface p-3 pl-9 pr-9 font-mono text-sm text-black focus:outline-none focus:bg-primary focus:text-white transition-colors placeholder:text-secondary"
            />
            {searchInput && (
              <button
                type="button"
                onClick={() => {
                  setSearchInput("");
                  setSearch("");
                  setPage(1);
                }}
                className="absolute right-2 top-1/2 -translate-y-1/2 p-1 text-secondary hover:text-primary transition-colors cursor-pointer"
                title="Clear search"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
        </label>

        <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase text-secondary">
          Sort
          <select
            value={sortInput}
            onChange={(event) => setSortInput(event.target.value as SortOption)}
            className="border-2 border-primary bg-surface p-3 font-mono text-sm font-bold text-black focus:outline-none focus:bg-primary focus:text-white transition-colors cursor-pointer"
          >
            <option value="newest">SORT: NEWEST</option>
            <option value="oldest">SORT: OLDEST</option>
            <option value="price-low-high">PRICE: LOW TO HIGH</option>
            <option value="price-high-low">PRICE: HIGH TO LOW</option>
          </select>
        </label>

        <div className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase text-secondary">
          Price (DA)
          <div className="flex items-center gap-2">
            <input
              type="number"
              min={0}
              placeholder="Min"
              value={minPriceInput}
              onChange={(event) => setMinPriceInput(event.target.value)}
              className="w-28 border-2 border-primary bg-surface p-3 font-mono text-sm text-black focus:outline-none focus:bg-primary focus:text-white transition-colors placeholder:text-secondary"
            />
            <span className="font-black">-</span>
            <input
              type="number"
              min={0}
              placeholder="Max"
              value={maxPriceInput}
              onChange={(event) => setMaxPriceInput(event.target.value)}
              className="w-28 border-2 border-primary bg-surface p-3 font-mono text-sm text-black focus:outline-none focus:bg-primary focus:text-white transition-colors placeholder:text-secondary"
            />
            <button
              type="submit"
              className="bg-primary text-white font-mono text-xs font-bold uppercase py-3 px-4 border-2 border-primary shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white hover:text-primary transition-all cursor-pointer"
            >
              Apply
            </button>
          </div>
        </div>

        <button
          type="button"
          onClick={clearFilters}
          disabled={!hasActiveFilters}
          className="bg-surface text-primary font-mono text-xs font-bold uppercase py-3 px-4 border-2 border-primary shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white transition-all cursor-pointer disabled:opacity-40 disabled:pointer-events-none self-start lg:self-auto"
        >
          Clear Filters
        </button>
      </form>
      {priceError && (
        <p className="bg-red-50 border-2 border-red-600 px-3 py-2 font-mono text-xs font-bold uppercase text-red-700">
          {priceError}
        </p>
      )}
      {isError && (
        <div className="bg-red-50 border-4 border-red-600 p-4 flex items-center justify-between gap-4">
          <p className="font-mono text-xs font-bold uppercase text-red-700">Failed to load products.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="bg-red-600 text-white font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-red-600 flex items-center gap-2 hover:bg-white hover:text-red-700 transition-colors cursor-pointer"
          >
            <RefreshCw className="w-4 h-4" /> Retry
          </button>
        </div>
      )}
      {productError && !productEditor && (
        <div className="bg-red-50 border-4 border-red-600 p-4 flex items-center justify-between gap-4">
          <p className="font-mono text-xs font-bold uppercase text-red-700">{productError}</p>
          <button
            type="button"
            onClick={() => setProductError(null)}
            className="p-1 border-2 border-red-600 text-red-700 hover:bg-red-600 hover:text-white transition-colors cursor-pointer"
            title="Dismiss"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}
      {/* Products Table */}
      <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] overflow-x-auto w-full">
        <table className="w-full text-left font-mono border-collapse min-w-237.5">
          <thead className="bg-primary text-white">
            <tr>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-20">ID</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-24">Image</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Product Name</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-40">Price</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-40">Discount</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-44">Variants</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-24">Stock</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-28 text-right">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white text-sm font-bold divide-y-2 divide-primary">
            {isPending ? (
              <tr>
                <td colSpan={8} className="p-6 text-center text-secondary font-mono animate-pulse">
                  Loading products…
                </td>
              </tr>
            ) : products.length === 0 ? (
              <tr>
                <td colSpan={8} className="p-6 text-center text-secondary font-mono">
                  <PackageOpen className="w-8 h-8 mx-auto mb-2" />
                  {hasActiveFilters ? (
                    <>
                      <p>No products match your filters.</p>
                      <button
                        type="button"
                        onClick={clearFilters}
                        className="mt-3 bg-surface text-primary font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-primary shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white transition-all cursor-pointer"
                      >
                        Clear Filters
                      </button>
                    </>
                  ) : (
                    <p>No products found.</p>
                  )}
                </td>
              </tr>
            ) : (
              products.map((product) => {
                const mainImage = product.images.find((img) => img.isMain) ?? product.images[0];
                const imageUrl = resolveImageUrl(mainImage?.imageUrl);
                return (
                  <tr key={product.id} className="hover:bg-surface-container transition-colors group">
                    <td className="p-4 border-r-2 border-primary text-secondary">#{shortId(product.id)}</td>
                    <td className="p-2 border-r-2 border-primary">
                      <div className="w-16 h-16 border-2 border-primary bg-surface overflow-hidden">
                        {imageUrl ? (
                          <img src={imageUrl} alt={product.name} className="w-full h-full object-cover" />
                        ) : (
                          <div className="w-full h-full flex items-center justify-center">
                            <ImageIcon className="w-5 h-5 text-secondary" />
                          </div>
                        )}
                      </div>
                    </td>
                    <td className="p-4 border-r-2 border-primary font-bold text-sm sm:text-base">
                      <div>{product.name}</div>
                      <div className="flex items-center gap-2 mt-1.5 flex-wrap">
                        <span className="px-2 py-0.5 border border-primary bg-surface text-[10px] uppercase font-bold text-secondary">
                          {categoryName(product.categoryId)}
                        </span>
                        {product.images.length > 1 && (
                          <span className="inline-flex items-center gap-1 text-[10px] text-secondary font-mono">
                            <ImageIcon className="w-3 h-3" />
                            {product.images.length} images
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="p-4 border-r-2 border-primary font-mono text-base font-black">
                      {formatPriceDA(product.basePrice)}
                    </td>
                    <td className="p-4 border-r-2 border-primary font-mono">
                      {product.discount ? (
                        <span
                          className="px-2.5 py-1 border-2 border-primary bg-yellow-300 text-black text-xs font-black tracking-wider inline-block shadow-[2px_2px_0_0_#000] whitespace-nowrap"
                          title={`Final price: ${formatPriceDA(getPrice(product.basePrice, product.discount))}`}
                        >
                          -{formatPriceDA(product.discount)}
                        </span>
                      ) : (
                        <span className="font-mono text-xs text-secondary font-bold">—</span>
                      )}
                    </td>
                    <td className="p-4 border-r-2 border-primary">
                      <button
                        type="button"
                        onClick={() => setSelectedProductId(product.id)}
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-surface hover:bg-black hover:text-white border-2 border-primary shadow-[2px_2px_0_0_#000] hover:shadow-none hover:translate-x-0.5 hover:translate-y-0.5 active:translate-x-1 active:translate-y-1 transition-all font-mono text-xs font-black uppercase cursor-pointer whitespace-nowrap"
                        title="Click to view variants"
                      >
                        <Layers className="w-3.5 h-3.5" />
                        <span>{product.variants.length} Variants</span>
                        <span className="text-[10px] underline ml-0.5">&rarr;</span>
                      </button>
                    </td>
                    <td className="p-4 font-mono">
                      <span
                        className={`px-2 py-1 border-2 border-primary text-xs font-black ${
                          sumStock(product.variants) > 0 ? "bg-surface" : "bg-red-100 text-red-700"
                        }`}
                      >
                        {sumStock(product.variants)}
                      </span>
                    </td>
                    <td className="p-4 text-right">
                      <div className="flex justify-end gap-2">
                        <button
                          type="button"
                          onClick={() => openProductEditor(product)}
                          disabled={isProductMutating || isVariantMutating}
                          className="p-2 bg-surface text-primary border-2 border-primary hover:bg-black hover:text-white transition-colors cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
                          title="Edit Product"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            setProductError(null);
                            setProductToDelete(product);
                          }}
                          disabled={isProductMutating || isVariantMutating}
                          className="p-2 bg-red-100 text-red-600 border-2 border-red-600 hover:bg-red-600 hover:text-white transition-colors cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
                          title="Delete Product"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
        <div className="min-w-237.5 p-4 border-t-4 border-primary bg-surface flex items-center justify-between gap-4">
          <span className="font-mono text-xs font-bold uppercase text-secondary">
            Page {data?.pageNumber ?? page} of {totalPages} — {totalCount} products
            {isFetching && <span className="ml-2 animate-pulse">loading…</span>}
          </span>
          <div className="sticky right-0 z-10 flex shrink-0 gap-2 bg-surface pl-4">
            <button
              type="button"
              disabled={page <= 1 || isFetching}
              onClick={() => setPage((prev) => Math.max(1, prev - 1))}
              className="bg-primary text-white font-mono font-bold uppercase py-2 px-4 border-2 border-primary shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all active:translate-x-2 active:translate-y-2 active:shadow-none disabled:opacity-40 disabled:pointer-events-none flex items-center gap-1"
            >
              <ChevronLeft className="w-4 h-4" /> Prev
            </button>
            <button
              type="button"
              disabled={page >= totalPages || isFetching}
              onClick={() => setPage((prev) => Math.min(totalPages, prev + 1))}
              className="bg-primary text-white font-mono font-bold uppercase py-2 px-4 border-2 border-primary shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all active:translate-x-2 active:translate-y-2 active:shadow-none disabled:opacity-40 disabled:pointer-events-none flex items-center gap-1"
            >
              Next <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Create Product */}
      {isCreateOpen && <CreateProductForm onClose={() => setIsCreateOpen(false)} />}

      {/* Edit Product */}
      {productEditor && (
        <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4 backdrop-blur-sm">
          <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-6 sm:p-8 w-full max-w-xl max-h-[90vh] overflow-y-auto relative animate-in fade-in zoom-in duration-200">
            <button
              type="button"
              onClick={() => {
                setProductEditor(null);
                setProductEditorId(null);
                setProductError(null);
              }}
              className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all cursor-pointer"
              title="Close"
            >
              <X className="w-6 h-6" />
            </button>
            <h2 className="font-display text-3xl font-black uppercase tracking-tighter mb-6">Edit Product</h2>
            {productError && (
              <div className="bg-red-50 border-4 border-red-600 p-3 mb-4 font-mono text-xs font-bold uppercase text-red-700">
                {productError}
              </div>
            )}
            <form onSubmit={handleProductSubmit} className="flex flex-col gap-4">
              <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
                Product Name
                <input
                  required
                  type="text"
                  maxLength={200}
                  value={productDraft.name}
                  onChange={(event) => setProductDraft((prev) => ({ ...prev, name: event.target.value }))}
                  className="border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                />
              </label>
              <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
                Description
                <textarea
                  rows={3}
                  value={productDraft.description}
                  onChange={(event) => setProductDraft((prev) => ({ ...prev, description: event.target.value }))}
                  className="border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors resize-y"
                />
              </label>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
                  Base Price
                  <input
                    required
                    type="number"
                    min={0}
                    step="0.01"
                    value={productDraft.basePrice}
                    onChange={(event) => setProductDraft((prev) => ({ ...prev, basePrice: event.target.value }))}
                    className="border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                  />
                </label>
                <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
                  Discount
                  <input
                    type="number"
                    min={0}
                    step="0.01"
                    value={productDraft.discount}
                    onChange={(event) => setProductDraft((prev) => ({ ...prev, discount: event.target.value }))}
                    className="border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                  />
                </label>
              </div>
              <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
                Category
                <select
                  required
                  value={productDraft.categoryId}
                  onChange={(event) => setProductDraft((prev) => ({ ...prev, categoryId: event.target.value }))}
                  className="border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                >
                  <option value="">Select a category</option>
                  {(categories ?? []).map((category) => (
                    <option key={category.id} value={category.id}>
                      {category.categoryName}
                    </option>
                  ))}
                </select>
              </label>
              <button
                type="submit"
                disabled={isProductMutating}
                className="mt-2 w-full bg-primary text-white font-mono font-black uppercase py-3 border-2 border-primary shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all cursor-pointer disabled:opacity-50 disabled:pointer-events-none"
              >
                {isProductMutating ? "Saving…" : "Save Changes"}
              </button>
            </form>
          </div>
        </div>
      )}
      <ConfirmDialog
        isOpen={productToDelete !== null}
        title="Delete Product"
        message={
          productToDelete
            ? `Are you sure you want to delete "${productToDelete.name}"? This action cannot be undone.`
            : ""
        }
        confirmLabel="Yes, Delete"
        pendingLabel="Deleting…"
        isConfirming={deleteProduct.isPending}
        onConfirm={() => void handleConfirmDeleteProduct()}
        onCancel={() => setProductToDelete(null)}
      />
      {/* Product Variants Modal (view / add / edit / delete variants) */}
      {selectedProduct && (
        <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4 backdrop-blur-sm">
          <div className="bg-white border-4 border-primary shadow-[16px_16px_0_0_#000] p-6 sm:p-8 w-full max-w-3xl max-h-[90vh] overflow-y-auto relative animate-in fade-in zoom-in duration-200">
            <button
              onClick={closeVariantsModal}
              className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all cursor-pointer"
              title="Close"
            >
              <X className="w-6 h-6" />
            </button>

            {/* Header with Product Info */}
            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4 pb-6 border-b-4 border-primary">
              <div className="w-20 h-20 border-2 border-primary bg-surface overflow-hidden shrink-0 shadow-[4px_4px_0_0_#000]">
                {(() => {
                  const mainImage = selectedProduct.images.find((img) => img.isMain) ?? selectedProduct.images[0];
                  const imageUrl = resolveImageUrl(mainImage?.imageUrl);
                  return imageUrl ? (
                    <img src={imageUrl} alt={selectedProduct.name} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center">
                      <ImageIcon className="w-6 h-6 text-secondary" />
                    </div>
                  );
                })()}
              </div>
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <span className="font-mono text-xs font-bold uppercase px-2 py-0.5 bg-primary text-white">
                    {categoryName(selectedProduct.categoryId)}
                  </span>
                  <span className="font-mono text-xs font-bold text-secondary">ID: #{shortId(selectedProduct.id)}</span>
                </div>
                <h2 className="font-display text-2xl sm:text-3xl font-black uppercase tracking-tight">
                  {selectedProduct.name}
                </h2>
                <p className="font-mono text-sm font-bold text-secondary mt-1">
                  Price: <span className="text-black font-black">{formatPriceDA(selectedProduct.basePrice)}</span> •
                  Total Stock: <span className="text-black font-black">{sumStock(selectedProduct.variants)}</span>
                </p>
              </div>
            </div>
            {/* Summary Chips */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 my-6">
              <div className="p-3 bg-surface border-2 border-primary shadow-[3px_3px_0_0_#000]">
                <span className="block font-mono text-[10px] uppercase font-bold text-secondary">Total Variants</span>
                <span className="font-display text-xl sm:text-2xl font-black">
                  {selectedProduct.variants.length} Variants
                </span>
              </div>
              <div className="p-3 bg-surface border-2 border-primary shadow-[3px_3px_0_0_#000]">
                <span className="block font-mono text-[10px] uppercase font-bold text-secondary">Colors Available</span>
                <span className="font-display text-xl sm:text-2xl font-black">
                  {new Set(selectedProduct.variants.map((v) => v.color)).size} Colors
                </span>
              </div>
            </div>

            {/* Variants: editable list with add / edit / delete */}
            <div className="mb-6">
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-3">
                <h3 className="font-display text-lg font-black uppercase tracking-wider flex items-center gap-2">
                  <Layers className="w-5 h-5" /> Variants (Size, Color, Stock)
                </h3>
                <button
                  type="button"
                  onClick={() => openVariantEditor()}
                  disabled={isVariantMutating || variantEditor?.mode === "create"}
                  className="bg-primary text-white font-mono text-xs font-black uppercase py-2 px-4 border-2 border-primary flex items-center justify-center gap-2 shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white hover:text-primary transition-all cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
                >
                  <Plus className="w-4 h-4" /> Add Variant
                </button>
              </div>

              {variantError && (
                <div className="bg-red-50 border-4 border-red-600 p-3 mb-3 flex items-center justify-between gap-3">
                  <p className="font-mono text-xs font-bold uppercase text-red-700">{variantError}</p>
                  <button
                    type="button"
                    onClick={() => setVariantError(null)}
                    className="p-1 border-2 border-red-600 text-red-700 hover:bg-red-600 hover:text-white transition-colors cursor-pointer"
                    title="Dismiss"
                  >
                    <X className="w-4 h-4" />
                  </button>
                </div>
              )}

              {/* Add / Edit Variant Form */}
              {variantEditor && (
                <form
                  onSubmit={handleVariantSubmit}
                  className="border-4 border-primary bg-surface p-4 mb-4 flex flex-col gap-4 shadow-[6px_6px_0_0_#000]"
                >
                  <h4 className="font-display text-base font-black uppercase tracking-wider">
                    {variantEditor.mode === "create"
                      ? "New Variant"
                      : `Edit Variant #${shortId(variantEditor.variant.id)}`}
                  </h4>

                  <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                    <div className="flex flex-col gap-1.5">
                      <label className="font-mono text-[10px] font-bold uppercase" htmlFor="variant-size">
                        Size
                      </label>
                      <input
                        id="variant-size"
                        required
                        type="text"
                        list="variant-size-options"
                        maxLength={50}
                        value={variantDraft.size}
                        onChange={(e) => setVariantDraft((prev) => ({ ...prev, size: e.target.value }))}
                        placeholder="e.g. M"
                        className="w-full border-2 border-primary bg-white p-2.5 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                      />
                    </div>
                    <div className="flex flex-col gap-1.5">
                      <label className="font-mono text-[10px] font-bold uppercase" htmlFor="variant-color">
                        Color
                      </label>
                      <input
                        id="variant-color"
                        required
                        type="text"
                        list="variant-color-options"
                        maxLength={50}
                        value={variantDraft.color}
                        onChange={(e) => setVariantDraft((prev) => ({ ...prev, color: e.target.value }))}
                        placeholder="e.g. BLACK"
                        className="w-full border-2 border-primary bg-white p-2.5 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                      />
                    </div>
                    <div className="flex flex-col gap-1.5">
                      <label className="font-mono text-[10px] font-bold uppercase" htmlFor="variant-stock">
                        Stock Quantity
                      </label>
                      <input
                        id="variant-stock"
                        required
                        type="number"
                        min={0}
                        step={1}
                        value={variantDraft.stockQuantity}
                        onChange={(e) => setVariantDraft((prev) => ({ ...prev, stockQuantity: e.target.value }))}
                        className="w-full border-2 border-primary bg-white p-2.5 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                      />
                    </div>
                  </div>

                  {variantDraft.color.trim() && (
                    <div className="flex items-center gap-2 font-mono text-[10px] font-bold uppercase text-secondary">
                      Preview swatch:
                      <span
                        className="w-5 h-5 rounded-full border-2 border-black inline-block shadow-[1px_1px_0_0_#000]"
                        style={{ backgroundColor: getColorHex(variantDraft.color) }}
                      />
                      {variantDraft.color.trim()}
                    </div>
                  )}

                  <div className="flex flex-col-reverse sm:flex-row gap-3 sm:justify-end">
                    <button
                      type="button"
                      onClick={() => {
                        setVariantEditor(null);
                        setVariantDraft(EMPTY_VARIANT_DRAFT);
                        setVariantError(null);
                      }}
                      disabled={isVariantMutating}
                      className="bg-white text-primary font-mono text-xs font-black uppercase py-2.5 px-6 border-2 border-primary shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-primary hover:text-white transition-all cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
                    >
                      Cancel
                    </button>
                    <button
                      type="submit"
                      disabled={isVariantMutating}
                      className="bg-primary text-white font-mono text-xs font-black uppercase py-2.5 px-6 border-2 border-primary shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white hover:text-primary transition-all cursor-pointer disabled:opacity-50 disabled:pointer-events-none"
                    >
                      {createVariant.isPending || updateVariant.isPending
                        ? "Saving…"
                        : variantEditor.mode === "create"
                          ? "Add Variant"
                          : "Save Changes"}
                    </button>
                  </div>
                </form>
              )}

              {/* Autocomplete suggestions for the size / color inputs above. */}
              <datalist id="variant-size-options">
                {variantSizeSuggestions.map((size) => (
                  <option key={size} value={size} />
                ))}
              </datalist>
              <datalist id="variant-color-options">
                {variantColorSuggestions.map((color) => (
                  <option key={color} value={color} />
                ))}
              </datalist>

              <div className="border-4 border-primary shadow-[6px_6px_0_0_#000] overflow-x-auto bg-white">
                <table className="w-full text-left font-mono border-collapse min-w-140">
                  <thead className="bg-primary text-white">
                    <tr>
                      <th className="p-3 border-b-2 border-primary uppercase text-xs w-28">Size</th>
                      <th className="p-3 border-b-2 border-primary uppercase text-xs">Color</th>
                      <th className="p-3 border-b-2 border-primary uppercase text-xs text-center w-24">Stock</th>
                      <th className="p-3 border-b-2 border-primary uppercase text-xs text-right w-28">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y-2 divide-primary text-sm">
                    {selectedProduct.variants.length > 0 ? (
                      selectedProduct.variants.map((variant) => (
                        <tr key={variant.id} className="hover:bg-surface-container transition-colors group">
                          <td className="p-3 border-r-2 border-primary font-black">
                            <span className="inline-block px-2.5 py-1 bg-black text-white font-mono font-bold text-xs border border-black shadow-[2px_2px_0_0_#888]">
                              {variant.size}
                            </span>
                          </td>
                          <td className="p-3 border-r-2 border-primary font-bold">
                            <div className="flex items-center gap-2">
                              <span
                                className="w-4 h-4 rounded-full border-2 border-black inline-block shrink-0 shadow-[1px_1px_0_0_#000]"
                                style={{ backgroundColor: getColorHex(variant.color) }}
                              />
                              <span className="font-bold">{variant.color}</span>
                            </div>
                          </td>
                          <td className="p-3 border-r-2 border-primary text-center">
                            <span
                              className={`px-2.5 py-1 border-2 border-primary font-black ${
                                variant.stockQuantity > 0 ? "bg-surface" : "bg-red-100 text-red-700"
                              }`}
                            >
                              {variant.stockQuantity}
                            </span>
                          </td>
                          <td className="p-3 text-right">
                            <div className="flex justify-end gap-1.5">
                              <button
                                type="button"
                                onClick={() => openVariantEditor(variant)}
                                disabled={isVariantMutating}
                                className="p-2 bg-surface text-primary border-2 border-primary hover:bg-black hover:text-white transition-colors cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
                                title="Edit Variant"
                              >
                                <Edit className="w-3.5 h-3.5" />
                              </button>
                              <button
                                type="button"
                                onClick={() => requestDeleteVariant(variant)}
                                disabled={isVariantMutating}
                                className="p-2 bg-red-100 text-red-600 border-2 border-red-600 hover:bg-red-600 hover:text-white transition-colors cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
                                title="Delete Variant"
                              >
                                <Trash2 className="w-3.5 h-3.5" />
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={4} className="p-6 text-center text-secondary font-mono">
                          No variants defined for this product.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Done Button */}
            <div className="mt-6 flex justify-end">
              <button
                type="button"
                onClick={closeVariantsModal}
                disabled={isVariantMutating}
                className="bg-primary text-white font-mono font-bold uppercase py-2.5 px-8 border-2 border-primary shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white hover:text-primary transition-all cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
              >
                Done / Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Variant Confirmation */}
      <ConfirmDialog
        isOpen={Boolean(variantToDelete)}
        title="Delete Variant"
        message={
          variantToDelete
            ? `Are you sure you want to delete the ${variantToDelete.size} / ${variantToDelete.color} variant? This action cannot be undone.`
            : ""
        }
        confirmLabel="Yes, Delete"
        pendingLabel="Deleting…"
        isConfirming={deleteVariant.isPending}
        onConfirm={() => void handleConfirmDeleteVariant()}
        onCancel={() => setVariantToDelete(null)}
      />
    </div>
  );
};

export default AdminProducts;
