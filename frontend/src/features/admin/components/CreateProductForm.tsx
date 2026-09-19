import React, { useEffect, useRef, useState, type FormEvent } from "react";
import { X, Star, Trash2, Upload, ImageIcon, Plus } from "lucide-react";
import { useCategories } from "../../categories/hooks/useCategories";
import { useCreateProduct } from "../hooks/useAdminProductMutations";
import type { CreateVariantInput } from "../api/adminProductApi";
import { getApiErrorMessage } from "../utils/apiErrors";
import { SIZE_SUGGESTIONS, COLOR_SUGGESTIONS, getColorHex } from "../constants/variantOptions";

interface ImageDraft {
  file: File;
  url: string;
}

interface VariantRow {
  size: string;
  color: string;
  stockQuantity: string;
}

const EMPTY_VARIANT_ROW: VariantRow = { size: "", color: "", stockQuantity: "0" };

interface CreateProductFormProps {
  onClose: () => void;
}

export const CreateProductForm: React.FC<CreateProductFormProps> = ({ onClose }) => {
  const { data: categories } = useCategories();
  const createProduct = useCreateProduct();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [name, setName] = useState("");
  const [basePrice, setBasePrice] = useState("");
  const [discount, setDiscount] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [images, setImages] = useState<ImageDraft[]>([]);
  const [mainImageIndex, setMainImageIndex] = useState(0);
  const [variants, setVariants] = useState<VariantRow[]>([{ ...EMPTY_VARIANT_ROW }]);
  const [formError, setFormError] = useState<string | null>(null);

  // Revoke object URLs on unmount so we don't leak memory.
  useEffect(() => {
    return () => {
      images.forEach((image) => URL.revokeObjectURL(image.url));
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleFilesSelected = (fileList: FileList | null) => {
    if (!fileList || fileList.length === 0) return;
    const newDrafts: ImageDraft[] = Array.from(fileList).map((file) => ({
      file,
      url: URL.createObjectURL(file),
    }));
    setImages((prev) => [...prev, ...newDrafts]);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const removeImage = (index: number) => {
    setImages((prev) => {
      URL.revokeObjectURL(prev[index].url);
      const next = prev.filter((_, i) => i !== index);
      return next;
    });
    setMainImageIndex((prev) => {
      if (index === prev) return 0;
      if (index < prev) return prev - 1;
      return prev;
    });
  };

  const addVariantRow = () => setVariants((prev) => [...prev, { ...EMPTY_VARIANT_ROW }]);
  const removeVariantRow = (index: number) => setVariants((prev) => prev.filter((_, i) => i !== index));
  const updateVariantRow = (index: number, field: keyof VariantRow, value: string) => {
    setVariants((prev) => prev.map((row, i) => (i === index ? { ...row, [field]: value } : row)));
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setFormError(null);

    const trimmedName = name.trim();
    const parsedBasePrice = Number(basePrice);
    const parsedDiscount = discount.trim() ? Number(discount) : undefined;

    if (!trimmedName || !categoryId || !Number.isFinite(parsedBasePrice) || parsedBasePrice < 0) {
      setFormError("Name, category, and a valid price are required.");
      return;
    }
    if (
      parsedDiscount !== undefined &&
      (!Number.isFinite(parsedDiscount) || parsedDiscount < 0 || parsedDiscount > parsedBasePrice)
    ) {
      setFormError("Discount must be between 0 and the base price.");
      return;
    }
    if (images.length === 0) {
      setFormError("At least one product image is required.");
      return;
    }

    const parsedVariants: CreateVariantInput[] = [];
    for (const row of variants) {
      const size = row.size.trim();
      const color = row.color.trim();
      const stockQuantity = Number(row.stockQuantity);
      if (!size || !color) {
        setFormError("Every variant needs a size and a color.");
        return;
      }
      if (!Number.isInteger(stockQuantity) || stockQuantity < 0) {
        setFormError("Stock quantity must be a whole number of 0 or more.");
        return;
      }
      parsedVariants.push({ size, color, stockQuantity });
    }
    if (parsedVariants.length === 0) {
      setFormError("At least one variant is required.");
      return;
    }

    try {
      await createProduct.mutateAsync({
        name: trimmedName,
        description: description.trim() || undefined,
        basePrice: parsedBasePrice,
        discount: parsedDiscount,
        categoryId,
        variants: parsedVariants,
        images: images.map((image) => image.file),
        mainImageIndex,
      });
      onClose();
    } catch (error) {
      setFormError(getApiErrorMessage(error, "Failed to create product."));
    }
  };

  return (
    <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4 backdrop-blur-sm">
      <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-6 sm:p-8 w-full max-w-2xl min-w-0 max-h-[90vh] overflow-x-hidden overflow-y-auto relative animate-in fade-in zoom-in duration-200">
        <button
          type="button"
          onClick={onClose}
          className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all cursor-pointer"
          title="Close"
        >
          <X className="w-6 h-6" />
        </button>

        <h2 className="font-display text-3xl font-black uppercase tracking-tighter">Add Product</h2>
        <p className="font-mono text-xs text-secondary mt-1 mb-6">
          Enter product info, pick a category, and upload multiple images.
        </p>

        {formError && (
          <div className="bg-red-50 border-4 border-red-600 p-3 mb-4 font-mono text-xs font-bold uppercase text-red-700">
            {formError}
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex flex-col gap-5">
          <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
            Product Name *
            <input
              required
              type="text"
              maxLength={200}
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. BAGGY VINTAGE DENIM"
              className="min-w-0 border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
            />
          </label>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
              Price (DA) *
              <input
                required
                type="number"
                min={0}
                step="0.01"
                value={basePrice}
                onChange={(e) => setBasePrice(e.target.value)}
                placeholder="6500"
                className="min-w-0 border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
              />
            </label>
            <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
              Category *
              <select
                required
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value)}
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
          </div>

          <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
            <span className="flex items-center justify-between">
              Discount (Optional)
              <span className="text-[10px] text-secondary normal-case">Can be empty</span>
            </span>
            <input
              type="number"
              min={0}
              step="0.01"
              value={discount}
              onChange={(e) => setDiscount(e.target.value)}
              placeholder="e.g. 2000.00 DA"
              className="border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
            />
          </label>

          <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
            <span className="flex items-center justify-between">
              Description (Optional)
              <span className="text-[10px] text-secondary normal-case">Can be empty</span>
            </span>
            <textarea
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Enter product details, cut, fit, fabric composition... (optional)"
              className="border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors resize-y"
            />
          </label>

          {/* Images */}
          <div className="border-t-2 border-primary pt-4">
            <div className="flex items-center justify-between gap-3 mb-3">
              <div>
                <h3 className="font-mono text-xs font-black uppercase flex items-center gap-1.5">
                  <ImageIcon className="w-4 h-4" /> Product Images ({images.length})
                </h3>
                <p className="font-mono text-[10px] text-secondary">
                  Upload multiple images and designate which one is the Main Image.
                </p>
              </div>
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                className="bg-primary text-white font-mono text-xs font-black uppercase py-2 px-4 border-2 border-primary flex items-center gap-2 shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white hover:text-primary transition-all cursor-pointer shrink-0"
              >
                <Upload className="w-4 h-4" /> Upload Images
              </button>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                multiple
                hidden
                onChange={(e) => handleFilesSelected(e.target.files)}
              />
            </div>

            {images.length === 0 ? (
              <div className="border-2 border-dashed border-primary p-8 flex flex-col items-center gap-2 text-center bg-surface">
                <ImageIcon className="w-8 h-8 text-secondary" />
                <span className="font-mono text-xs font-bold uppercase">No images uploaded yet</span>
                <span className="font-mono text-[10px] text-secondary max-w-xs">
                  Click "Upload Images" to select one or multiple photos from your device. The first image will be set
                  as Main automatically.
                </span>
              </div>
            ) : (
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                {images.map((image, index) => {
                  const isMain = index === mainImageIndex;
                  return (
                    <div
                      key={image.url}
                      className={`border-2 p-2 flex flex-col gap-2 ${
                        isMain ? "border-primary bg-yellow-50" : "border-primary bg-surface"
                      }`}
                    >
                      <div className="w-full aspect-square border-2 border-primary overflow-hidden bg-white">
                        <img src={image.url} alt={`Image ${index + 1}`} className="w-full h-full object-cover" />
                      </div>
                      <span className="font-mono text-[10px] font-bold uppercase">Image #{index + 1}</span>
                      <div className="flex items-center justify-between gap-2">
                        {isMain ? (
                          <span className="inline-flex items-center gap-1 px-2 py-1 bg-yellow-300 border-2 border-primary text-[10px] font-black uppercase">
                            <Star className="w-3 h-3 fill-current" /> Main
                          </span>
                        ) : (
                          <button
                            type="button"
                            onClick={() => setMainImageIndex(index)}
                            className="inline-flex items-center gap-1 px-2 py-1 bg-white border-2 border-primary text-[10px] font-black uppercase hover:bg-primary hover:text-white transition-colors cursor-pointer"
                          >
                            <Star className="w-3 h-3" /> Set as Main
                          </button>
                        )}
                        <button
                          type="button"
                          onClick={() => removeImage(index)}
                          className="p-1.5 bg-red-100 text-red-600 border-2 border-red-600 hover:bg-red-600 hover:text-white transition-colors cursor-pointer"
                          title="Remove image"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                      {isMain && (
                        <span className="font-mono text-[10px] font-bold text-green-700 uppercase">
                          Primary Display
                        </span>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* Variants */}
          <div className="border-t-2 border-primary pt-4">
            <div className="flex items-center justify-between gap-3 mb-3">
              <h3 className="font-mono text-xs font-black uppercase">Variants (Size, Color, Stock)</h3>
              <button
                type="button"
                onClick={addVariantRow}
                className="bg-primary text-white font-mono text-xs font-black uppercase py-2 px-4 border-2 border-primary flex items-center gap-2 shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white hover:text-primary transition-all cursor-pointer"
              >
                <Plus className="w-4 h-4" /> Add Variant
              </button>
            </div>

            <datalist id="create-variant-size-options">
              {SIZE_SUGGESTIONS.map((size) => (
                <option key={size} value={size} />
              ))}
            </datalist>
            <datalist id="create-variant-color-options">
              {COLOR_SUGGESTIONS.map((color) => (
                <option key={color} value={color} />
              ))}
            </datalist>

            <div className="flex flex-col gap-3">
              {variants.map((row, index) => (
                <div
                  key={index}
                  className="min-w-0 border-2 border-primary bg-surface p-3 grid grid-cols-1 sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_auto] gap-3 items-end"
                >
                  <div className="min-w-0 flex flex-col gap-1">
                    <label className="font-mono text-[10px] font-bold uppercase">Size</label>
                    <input
                      required
                      type="text"
                      list="create-variant-size-options"
                      maxLength={50}
                      value={row.size}
                      onChange={(e) => updateVariantRow(index, "size", e.target.value)}
                      placeholder="e.g. M"
                      className="w-full min-w-0 border-2 border-primary bg-white p-2 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                    />
                  </div>
                  <div className="min-w-0 flex flex-col gap-1">
                    <label className="font-mono text-[10px] font-bold uppercase">Color</label>
                    <div className="flex items-center gap-2">
                      {row.color.trim() && (
                        <span
                          className="w-4 h-4 rounded-full border-2 border-black shrink-0"
                          style={{ backgroundColor: getColorHex(row.color) }}
                        />
                      )}
                      <input
                        required
                        type="text"
                        list="create-variant-color-options"
                        maxLength={50}
                        value={row.color}
                        onChange={(e) => updateVariantRow(index, "color", e.target.value)}
                        placeholder="e.g. BLACK"
                        className="min-w-0 flex-1 border-2 border-primary bg-white p-2 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                      />
                    </div>
                  </div>
                  <div className="min-w-0 flex flex-col gap-1">
                    <label className="font-mono text-[10px] font-bold uppercase">Stock</label>
                    <input
                      required
                      type="number"
                      min={0}
                      step={1}
                      value={row.stockQuantity}
                      onChange={(e) => updateVariantRow(index, "stockQuantity", e.target.value)}
                      className="w-full min-w-0 border-2 border-primary bg-white p-2 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors"
                    />
                  </div>
                  <button
                    type="button"
                    onClick={() => removeVariantRow(index)}
                    disabled={variants.length === 1}
                    className="p-2 bg-red-100 text-red-600 border-2 border-red-600 hover:bg-red-600 hover:text-white transition-colors cursor-pointer disabled:opacity-30 disabled:pointer-events-none h-fit"
                    title="Remove variant"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          </div>

          <button
            type="submit"
            disabled={createProduct.isPending}
            className="mt-2 w-full bg-primary text-white font-mono font-black uppercase py-3 border-2 border-primary shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all cursor-pointer disabled:opacity-50 disabled:pointer-events-none"
          >
            {createProduct.isPending ? "Creating…" : "Create Product"}
          </button>
        </form>
      </div>
    </div>
  );
};

export default CreateProductForm;
