export interface CartProduct {
  id: string;
  name: string;
  imageUrl: string | null;
  basePrice: number;
  discount: number;
}

export interface CartVariant {
  id: string;
  size: string;
  color: string;
  stockQuantity: number;
  product: CartProduct;
}

export interface CartItem {
  id: string;
  quantity: number;
  variant: CartVariant;
}

export interface Cart {
  id: string;
  totalAmount: number;
  items: CartItem[];
}
