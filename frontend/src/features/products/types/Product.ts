export interface PaginatedList<T> {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  items: T[] | null;
}

export interface Product {
  id: string;
  name: string;
  description?: string | null;
  basePrice: number;
  discount?: number | null;
  categoryId: string;
  variants: Variant[];
  images: Image[];
}

export interface Variant {
  id: string;
  size: string;
  color: string;
  stockQuantity: number;
}

export interface Image {
  imageUrl: string;
  isMain: boolean;
}
