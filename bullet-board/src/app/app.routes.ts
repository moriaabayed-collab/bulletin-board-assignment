import { Routes } from '@angular/router';
import { AdListComponent } from './components/ad-list/ad-list.component';

export const routes: Routes = [
  { path: '', component: AdListComponent },
  { path: '**', redirectTo: '' },
];
