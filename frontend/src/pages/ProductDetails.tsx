import { useParams } from "react-router-dom";
import { useState } from "react";
import ProductGallery from "../components/products/ProductGallery";
import ProductInfo from "../components/products/ProductInfo";
import ProductSelectors from "../components/products/ProductSelectors";
import ProductActions from "../components/products/ProductActions";
import ErrorState from "../components/ui/ErrorState";
import { useProduct } from "../features/products/hooks/useProduct";
import type { Variant } from "../features/products/types/Product";

function getImageUrl(imageUrl: string) {
  if (/^https?:\/\//i.test(imageUrl)) return imageUrl;

  const backEndUrl = import.meta.env.VITE_API_URL ?? "";
  return `${backEndUrl.replace(/\/$/, "")}/${imageUrl.replace(/^\//, "")}`;
}

export default function ProductDetails() {
  const { id } = useParams<{ id: string }>();
  const [selectedVariant, setSelectedVariant] = useState<Variant>();
  const [selectedQuantity, setSelectedQuantity] = useState(1);
  const { data: product, isPending, isError, refetch } = useProduct(id);

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

  const sortedImages = [...product.images].sort((first, second) => Number(second.isMain) - Number(first.isMain));

  return (
    <div className="grow w-full max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
      <div className="grid grid-cols-1 md:grid-cols-12 items-start gap-0 border-2 border-primary bg-white shadow-[8px_8px_0_0_#000]">
        <ProductGallery images={sortedImages.map((image) => getImageUrl(image.imageUrl))} />
        <div className="col-span-1 md:col-span-5 flex flex-col bg-surface md:border-l-2 border-primary">
          <ProductInfo
            title={product.name}
            sku={product.id}
            price={`${product.basePrice} DA`}
            description={product.description ?? undefined}
          />
          <ProductSelectors
            variants={product.variants}
            onSelectionChange={(variant, quantity) => {
              setSelectedVariant(variant);
              setSelectedQuantity(quantity);
            }}
          />
          <ProductActions
            variantId={selectedVariant?.id}
            quantity={selectedQuantity}
            disabled={!selectedVariant || selectedVariant.stockQuantity < 1}
          />
        </div>
      </div>
    </div>
  );
}
