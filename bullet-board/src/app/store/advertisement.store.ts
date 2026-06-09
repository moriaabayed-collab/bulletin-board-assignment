import { Injectable, PLATFORM_ID, inject, signal, computed } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { tap } from 'rxjs';
import { AdvertisementApi } from '../api/advertisement.api';
import { Advertisement, AdvertisementRequest } from '../models/advertisement.model';
import { Category } from '../models/category.model';
import { UserStore } from './user.store';

const OWNERSHIP_KEY = 'bb_owned_ads';

@Injectable({ providedIn: 'root' })
export class AdvertisementStore {
  private api = inject(AdvertisementApi);
  private userStore = inject(UserStore);
  private platformId = inject(PLATFORM_ID);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly searchQuery = signal('');
  readonly selectedCategory = signal<Category | null>(null);
  readonly showForm = signal(false);
  readonly editingAd = signal<Advertisement | null>(null);

  private readonly _ads = signal<Advertisement[]>([]);
  readonly categories = signal<Category[]>([]);

  readonly filteredAds = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const category = this.selectedCategory();

    return this._ads().filter((ad) => {
      const matchesSearch =
        !query ||
        ad.title.toLowerCase().includes(query) ||
        ad.description.toLowerCase().includes(query) ||
        ad.location.toLowerCase().includes(query);
      const matchesCategory = !category || ad.category.id === category.id;
      return matchesSearch && matchesCategory;
    });
  });

  constructor() {
    this.loadCategories();
    this.loadAds();
  }

  loadAds() {
    this.loading.set(true);
    this.error.set(null);

    this.api.getAdvertisements().subscribe({
      next: (ads) => {
        this._ads.set(ads);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load advertisements. Please try again.');
        this.loading.set(false);
      },
    });
  }

  openCreate() {
    this.editingAd.set(null);
    this.showForm.set(true);
  }

  openEdit(ad: Advertisement) {
    this.editingAd.set(ad);
    this.showForm.set(true);
  }

  closeForm() {
    this.showForm.set(false);
    this.editingAd.set(null);
  }

  createAd(req: AdvertisementRequest) {
    return this.api.createAdvertisement(req).pipe(
      tap((ad) => {
        this._ads.update((ads) => [ad, ...ads]);
        this.saveOwnership(ad.id);
        this.closeForm();
      }),
    );
  }

  updateAd(id: number, req: AdvertisementRequest) {
    return this.api.updateAdvertisement(id, req).pipe(
      tap((updated) => {
        this._ads.update((ads) => ads.map((a) => (a.id === id ? updated : a)));
        this.closeForm();
      }),
    );
  }

  deleteAd(id: number) {
    return this.api.deleteAdvertisement(id).pipe(
      tap(() => {
        this._ads.update((ads) => ads.filter((a) => a.id !== id));
        this.removeOwnership(id);
      }),
    );
  }

  isOwned(ad: Advertisement): boolean {
    const user = this.userStore.currentUser();
    if (!user) return false;
    // Authoritative check: backend tells us who owns the ad
    if (ad.user_id && user.id) return ad.user_id === user.id;
    // Fallback: email-keyed localStorage (covers cases where user_id isn't in the API response)
    return this.getOwnershipMap()[user.email]?.includes(ad.id) ?? false;
  }

  private loadCategories() {
    this.api.getCategories().subscribe({
      next: (cats) => this.categories.set(cats),
      error: () => {},
    });
  }

  private getOwnershipMap(): Record<string, number[]> {
    if (!isPlatformBrowser(this.platformId)) return {};
    try {
      return JSON.parse(localStorage.getItem(OWNERSHIP_KEY) ?? '{}');
    } catch {
      return {};
    }
  }

  private saveOwnership(adId: number) {
    const user = this.userStore.currentUser();
    if (!user || !isPlatformBrowser(this.platformId)) return;
    const map = this.getOwnershipMap();
    map[user.email] = [...(map[user.email] ?? []), adId];
    localStorage.setItem(OWNERSHIP_KEY, JSON.stringify(map));
  }

  private removeOwnership(adId: number) {
    const user = this.userStore.currentUser();
    if (!user || !isPlatformBrowser(this.platformId)) return;
    const map = this.getOwnershipMap();
    map[user.email] = (map[user.email] ?? []).filter((id) => id !== adId);
    localStorage.setItem(OWNERSHIP_KEY, JSON.stringify(map));
  }
}
