import { Component, inject, signal, OnInit, AfterViewInit, ElementRef, ViewChild, PLATFORM_ID, NgZone } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AdvertisementStore } from '../../store/advertisement.store';
import { Category } from '../../models/category.model';

@Component({
  selector: 'app-ad-form',
  imports: [ReactiveFormsModule],
  templateUrl: './ad-form.component.html',
  styleUrl: './ad-form.component.scss',
})
export class AdFormComponent implements OnInit, AfterViewInit {
  store = inject(AdvertisementStore);
  private fb = inject(FormBuilder);
  private platformId = inject(PLATFORM_ID);
  private zone = inject(NgZone);

  @ViewChild('locationContainer') locationContainerRef!: ElementRef<HTMLDivElement>;
  @ViewChild('mapDiv') mapDivRef?: ElementRef<HTMLDivElement>;

  private placeAuto: google.maps.places.PlaceAutocompleteElement | null = null;

  saving = signal(false);
  saveError = signal<string | null>(null);
  categoryOpen = signal(false);
  hasLocation = signal(false);
  autocompleteReady = signal(false);

  private mapInstance: google.maps.Map | null = null;
  private markerInstance: google.maps.Marker | null = null;

  get selectedCategory(): Category | null {
    const id = this.form.controls.category_id.value;
    return this.store.categories().find(c => c.id === id) ?? null;
  }

  get isEditing() {
    return !!this.store.editingAd();
  }

  selectCategory(cat: Category) {
    this.form.controls.category_id.setValue(cat.id);
    this.form.controls.category_id.markAsTouched();
    this.categoryOpen.set(false);
  }

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    category_id: [null as number | null, Validators.required],
    price: [null as number | null],
    currency: ['', [Validators.maxLength(30)]],
    location: ['', [Validators.maxLength(200)]],
    contact: ['', [Validators.required, Validators.maxLength(50)]],
  });

  ngOnInit() {
    const ad = this.store.editingAd();
    if (ad) {
      this.form.patchValue({
        title: ad.title,
        description: ad.description,
        category_id: ad.category.id,
        price: ad.price ?? null,
        currency: ad.currency ?? '',
        location: ad.location,
        contact: ad.contact,
      });
    }
  }

  ngAfterViewInit() {
    if (!isPlatformBrowser(this.platformId)) return;
    if (window.google?.maps?.places) {
      this.initAutocomplete();
    } else {
      window.addEventListener('google-maps-ready', () => this.initAutocomplete(), { once: true });
    }
  }

  private initAutocomplete() {
    if (!window.google?.maps?.places?.PlaceAutocompleteElement) return;

    this.placeAuto = new google.maps.places.PlaceAutocompleteElement({
      placeholder: 'Search an address…',
    });
    this.locationContainerRef.nativeElement.appendChild(this.placeAuto);

    const existing = this.form.controls.location.value;
    if (existing) this.placeAuto.value = existing;

    this.autocompleteReady.set(true);

    this.placeAuto.addEventListener('gmp-select', async (event: google.maps.places.PlacePredictionSelectEvent) => {
      const place = event.placePrediction.toPlace();
      await place.fetchFields({ fields: ['formattedAddress', 'location'] });
      this.zone.run(() => {
        this.form.controls.location.setValue(place.formattedAddress ?? '');
        this.hasLocation.set(true);
        if (place.location) {
          setTimeout(() => this.renderMap(place.location!.lat(), place.location!.lng()));
        }
      });
    });
  }

  private renderMap(lat: number, lng: number) {
    const el = this.mapDivRef?.nativeElement;
    if (!el || !window.google?.maps) return;

    if (this.mapInstance) {
      this.mapInstance.setCenter({ lat, lng });
      this.markerInstance?.setPosition({ lat, lng });
      return;
    }

    this.mapInstance = new google.maps.Map(el, {
      center: { lat, lng },
      zoom: 13,
      disableDefaultUI: true,
      zoomControl: true,
      clickableIcons: false,
    });

    google.maps.event.addListenerOnce(this.mapInstance, 'idle', () => {
      this.markerInstance = new google.maps.Marker({
        position: { lat, lng },
        map: this.mapInstance!,
      });
    });
  }

  submit() {
    if (this.placeAuto) {
      this.form.controls.location.setValue(this.placeAuto.value ?? '');
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.value;
    const req = {
      title: v.title!,
      description: v.description!,
      category_id: v.category_id!,
      price: v.price ?? null,
      currency: v.currency || null,
      location: v.location!,
      contact: v.contact!,
    };

    this.saving.set(true);
    this.saveError.set(null);

    const ad = this.store.editingAd();
    const action$ = ad ? this.store.updateAd(ad.id, req) : this.store.createAd(req);

    action$.subscribe({
      error: () => {
        this.saving.set(false);
        this.saveError.set('Failed to save. Please try again.');
      },
      complete: () => this.saving.set(false),
    });
  }
}
