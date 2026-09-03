export interface GetProductsParams {
  categoryId?: string;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  sizes?: string[];
  colors?: string[];
  sortBy?: string;
  descending: boolean;
  pageNumber: number;
  pageSize: number;
}
