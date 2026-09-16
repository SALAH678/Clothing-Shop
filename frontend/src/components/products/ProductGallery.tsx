import { useState } from "react";

interface ProductGalleryProps {
  images: string[];
  productName?: string;
}

export default function ProductGallery({ images, productName = "Product" }: ProductGalleryProps) {
  const [mainImage, setMainImage] = useState(images[0]);
  const [prevImages, setPrevImages] = useState(images);

  // Reset the selected image when the gallery source changes (e.g. navigating
  // between products reuses this component). Adjusting state during render is the
  // React-recommended alternative to a sync effect: no extra commit, no flash.
  if (prevImages !== images) {
    setPrevImages(images);
    setMainImage(images[0]);
  }

  if (images.length === 0) {
    return (
      <div className="col-span-1 md:col-span-7 flex items-center justify-center min-h-96 bg-surface-container font-mono uppercase">
        No images available
      </div>
    );
  }

  return (
    <div className="col-span-1 md:col-span-7 flex flex-col md:flex-row-reverse border-b-2 md:border-b-0 border-primary gap-0 md:h-[80vh]">
      {/* Primary Image */}
      <div className="w-full md:w-4/5 h-[60vh] md:h-full bg-surface-container relative md:border-l-2 border-primary shrink-0 flex items-center justify-center">
        {mainImage ? (
          <img
            alt={productName}
            className="w-full h-full object-contain p-4"
            src={mainImage}
            loading="eager"
            decoding="async"
          />
        ) : (
          <span className="font-mono uppercase">No image</span>
        )}
        <div className="absolute top-4 left-4 border-2 border-primary bg-surface px-3 py-1 font-mono text-sm font-bold z-10 uppercase">
          FW24 // DROP 01
        </div>
      </div>
      {/* Thumbnails */}
      <div
        className="w-full md:w-1/5 md:h-full flex flex-row md:flex-col overflow-x-auto md:overflow-y-auto bg-surface-container gap-4 p-4 border-t-2 md:border-t-0 border-primary"
        style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
      >
        {images.map((img, idx) => (
          <button
            key={`${img}-${idx}`}
            type="button"
            aria-label={`View image ${idx + 1} of ${productName}`}
            aria-pressed={mainImage === img}
            className={`shrink-0 w-32 md:w-full aspect-[0.8] p-0 group border-2 border-primary ${mainImage === img ? "opacity-100" : "opacity-60 hover:opacity-100"} transition-opacity`}
            onClick={() => setMainImage(img)}
          >
            <img
              alt={`${productName} thumbnail ${idx + 1}`}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
              src={img}
              loading="lazy"
              decoding="async"
            />
          </button>
        ))}
      </div>
    </div>
  );
}
