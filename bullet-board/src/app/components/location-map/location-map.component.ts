import { Component, signal, PLATFORM_ID, inject, ViewChild, ElementRef } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-location-map',
  templateUrl: './location-map.component.html',
  styleUrl: './location-map.component.scss',
})
export class LocationMapComponent {
  private platformId = inject(PLATFORM_ID);

  visible = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);
  position = signal<{ lat: number; lng: number } | null>(null);

  @ViewChild('mapDiv') mapDivRef?: ElementRef<HTMLDivElement>;
  private mapInstance: google.maps.Map | null = null;
  private markerInstance: google.maps.Marker | null = null;

  toggle() {
    if (this.visible()) {
      this.visible.set(false);
      return;
    }
    if (this.position()) {
      this.visible.set(true);
      requestAnimationFrame(() => this.refreshMap());
      return;
    }
    this.locate();
  }

  locate() {
    if (!isPlatformBrowser(this.platformId) || !navigator.geolocation) {
      this.error.set('Geolocation is not supported by your browser.');
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    const init = () => {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          const lat = pos.coords.latitude;
          const lng = pos.coords.longitude;
          this.position.set({ lat, lng });
          this.visible.set(true);
          this.loading.set(false);
          requestAnimationFrame(() => this.renderMap(lat, lng));
        },
        () => {
          this.error.set('Could not get your location. Please allow location access.');
          this.loading.set(false);
        },
      );
    };

    if (window.google?.maps) {
      init();
    } else {
      window.addEventListener('google-maps-ready', init, { once: true });
    }
  }

  private refreshMap() {
    const pos = this.position();
    if (!pos || !this.mapInstance) return;
    google.maps.event.trigger(this.mapInstance, 'resize');
    this.mapInstance.setCenter(pos);
  }

  private renderMap(lat: number, lng: number) {
    const el = this.mapDivRef?.nativeElement;
    if (!el || !window.google?.maps) return;

    if (this.mapInstance) {
      google.maps.event.trigger(this.mapInstance, 'resize');
      this.mapInstance.setCenter({ lat, lng });
      this.markerInstance?.setPosition({ lat, lng });
      return;
    }

    this.mapInstance = new google.maps.Map(el, {
      center: { lat, lng },
      zoom: 14,
      disableDefaultUI: true,
      zoomControl: true,
      clickableIcons: false,
    });

    google.maps.event.addListenerOnce(this.mapInstance, 'idle', () => {
      this.markerInstance = new google.maps.Marker({
        position: { lat, lng },
        map: this.mapInstance!,
        title: 'You are here',
      });
    });
  }
}
