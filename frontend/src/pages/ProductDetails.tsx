import { useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import ProductGallery from "../components/products/ProductGallery";
import ProductInfo from "../components/products/ProductInfo";
import ProductSelectors, { type SelectionState } from "../components/products/ProductSelectors";
import ProductActions from "../components/products/ProductActions";
import ErrorState from "../components/ui/ErrorState";
import { useProduct } from "../features/products/hooks/useProduct";
import { resolveImageUrl } from "../lib/imageUrl";

export default function ProductDetails() {
  const { id } = useParams<{ id: string }>();
  const [selection, setSelection] = useState<SelectionState>({
    variant: undefined,
    quantity: 1,
    selectedColor: "",
    selectedSize: "",
  });
  const { data: product, isPending, isError, refetch } = useProduct(id);

  const galleryImages = useMemo(() => {
    if (!product) return [];
    return [...product.images]
      .sort((first, second) => Number(second.isMain) - Number(first.isMain))
      .map((image) => resolveImageUrl(image.imageUrl))
      .filter((url): url is string => Boolean(url));
  }, [product]);

  if (isPending) {
    return <div className="grow flex items-center justify-center p-12 font-mono uppercase">Loading product...</div>;
  }

  if (isError || !product) {
    return (
      <ErrorState
        label="Error 503 / Product unavailable"
        message="We could not load this product. Please try again in a moment."
        onRetry={refetch}
      />
    );
  }

  const { variant: selectedVariant } = selection;

  return (
    <div className="grow w-full max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
      <div className="grid grid-cols-1 md:grid-cols-12 items-start gap-0 border-2 border-primary bg-white shadow-[8px_8px_0_0_#000]">
        <ProductGallery key={product.id} images={galleryImages} productName={product.name} />
        <div className="col-span-1 md:col-span-5 flex flex-col bg-surface md:border-l-2 border-primary">
          <ProductInfo
            title={product.name}
            sku={product.id}
            basePrice={product.basePrice}
            discount={product.discount ?? null}
            description={product.description ?? undefined}
          />
          <ProductSelectors
            key={product.id}
            variants={product.variants}
            value={selection}
            onChange={setSelection}
          />
          <ProductActions
            variantId={selectedVariant?.id}
            quantity={selection.quantity}
            disabled={!selectedVariant || selectedVariant.stockQuantity < 1}
            selectedColor={selection.selectedColor}
            selectedSize={selection.selectedSize}
            productId={product.id}
            selectedVariant={selectedVariant}
          />
        </div>
      </div>
    </div>
  );
}
