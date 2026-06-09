import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Advertisement, AdvertisementRequest } from '../models/advertisement.model';
import { Category } from '../models/category.model';

const BASE_URL = '';

@Injectable({ providedIn: 'root' })
export class AdvertisementApi {
  private http = inject(HttpClient);

  getAdvertisements() {
    return this.http.get<Advertisement[]>(`${BASE_URL}/api/advertisements`);
  }

  createAdvertisement(req: AdvertisementRequest) {
    return this.http.post<Advertisement>(`${BASE_URL}/api/advertisements`, req);
  }

  updateAdvertisement(id: number, req: AdvertisementRequest) {
    return this.http.put<Advertisement>(`${BASE_URL}/api/advertisements/${id}`, req);
  }

  deleteAdvertisement(id: number) {
    return this.http.delete<void>(`${BASE_URL}/api/advertisements/${id}`);
  }

  getCategories() {
    return this.http.get<Category[]>(`${BASE_URL}/api/categories`);
  }
}
