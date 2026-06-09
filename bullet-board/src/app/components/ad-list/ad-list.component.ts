import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdvertisementStore } from '../../store/advertisement.store';
import { UserStore } from '../../store/user.store';
import { AdCardComponent } from '../ad-card/ad-card.component';
import { AdFormComponent } from '../ad-form/ad-form.component';
import { LocationMapComponent } from '../location-map/location-map.component';
import { Category } from '../../models/category.model';

@Component({
  selector: 'app-ad-list',
  imports: [FormsModule, AdCardComponent, AdFormComponent, LocationMapComponent],
  templateUrl: './ad-list.component.html',
  styleUrl: './ad-list.component.scss',
})
export class AdListComponent {
  store = inject(AdvertisementStore);
  userStore = inject(UserStore);

  readonly ads = this.store.filteredAds;
  readonly categories = this.store.categories;
  readonly searchQuery = this.store.searchQuery;
  readonly selectedCategory = this.store.selectedCategory;
  readonly loading = this.store.loading;
  readonly error = this.store.error;

  badgeHue(id: number): string {
    return `--hue: ${(id * 47) % 360}deg`;
  }

  selectCategory(category: Category | null) {
    this.selectedCategory.set(category);
  }

  clearFilters() {
    this.searchQuery.set('');
    this.selectedCategory.set(null);
  }

  retry() {
    this.store.loadAds();
  }

  get hasActiveFilters(): boolean {
    return !!this.searchQuery() || !!this.selectedCategory();
  }
}
