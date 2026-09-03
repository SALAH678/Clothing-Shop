export interface Category {
  id: string;
  categoryName: string;
  imageUrl?: string | null;
  subcategories?: Category[] | null;
}
