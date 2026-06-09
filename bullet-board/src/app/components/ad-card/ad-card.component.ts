import { Component, input, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Advertisement } from '../../models/advertisement.model';
import { AdvertisementStore } from '../../store/advertisement.store';
import { UserStore } from '../../store/user.store';

@Component({
  selector: 'app-ad-card',
  imports: [DatePipe],
  templateUrl: './ad-card.component.html',
  styleUrl: './ad-card.component.scss',
})
export class AdCardComponent {
  ad = input.required<Advertisement>();
  store = inject(AdvertisementStore);
  userStore = inject(UserStore);

  expanded = signal(false);
  confirmingDelete = signal(false);
  deleting = signal(false);

  toggle() { this.expanded.update((v) => !v); }

  badgeHue(id: number): string {
    return `--hue: ${(id * 47) % 360}deg`;
  }

  get isOwned() {
    return this.store.isOwned(this.ad());
  }

  onEdit(event: Event) {
    event.stopPropagation();
    this.store.openEdit(this.ad());
  }

  onDeleteClick(event: Event) {
    event.stopPropagation();
    this.confirmingDelete.set(true);
  }

  cancelDelete(event: Event) {
    event.stopPropagation();
    this.confirmingDelete.set(false);
  }

  confirmDelete(event: Event) {
    event.stopPropagation();
    this.deleting.set(true);
    this.store.deleteAd(this.ad().id).subscribe({
      error: () => {
        this.deleting.set(false);
        this.confirmingDelete.set(false);
      },
    });
  }

  formatPrice(ad: Advertisement): string {
    if (ad.price == null) return 'Contact for price';
    return `${ad.price.toLocaleString()}${ad.currency ?? ''}`.trim();
  }
}
