import { Category } from './category.model';

export interface AdvertisementRequest {
  title: string;
  description: string;
  category_id: number;
  price?: number | null;
  currency?: string | null;
  location: string;
  contact: string;
}

export interface Advertisement {
  id: number;
  user_id: number;
  title: string;
  description: string;
  category: Category;
  price?: number;
  currency?: string;
  last_update: Date;
  location: string;
  contact: string;
}
