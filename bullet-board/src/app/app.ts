import { Component, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { UserSelectorComponent } from './components/user-selector/user-selector.component';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, UserSelectorComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private platformId = inject(PLATFORM_ID);

  constructor() {
    if (isPlatformBrowser(this.platformId) && environment.googleMapsApiKey !== 'YOUR_API_KEY_HERE') {
      const script = document.createElement('script');
      script.src = `https://maps.googleapis.com/maps/api/js?key=${environment.googleMapsApiKey}&libraries=places`;
      script.async = true;
      script.onload = () => window.dispatchEvent(new Event('google-maps-ready'));
      document.head.appendChild(script);
    }
  }
}
