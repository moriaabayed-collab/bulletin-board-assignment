import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { tap } from 'rxjs';
import { User } from '../models/user.model';
import { AuthApi } from '../api/auth.api';

const USER_KEY = 'bb_current_user';
const TOKEN_KEY = 'bb_token';

function decodePayload(token: string): Record<string, unknown> | null {
  try {
    return JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
  } catch {
    return null;
  }
}

function tokenExpiresAt(token: string): number | null {
  const p = decodePayload(token);
  return p && typeof p['exp'] === 'number' ? (p['exp'] as number) * 1000 : null;
}

function tokenUserId(token: string): number {
  const p = decodePayload(token);
  if (!p) return 0;
  // .NET puts the user ID in one of these claims
  const raw = p['sub']
    ?? p['nameid']
    ?? p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
  const id = Number(raw);
  return Number.isFinite(id) ? id : 0;
}

@Injectable({ providedIn: 'root' })
export class UserStore {
  private platformId = inject(PLATFORM_ID);
  private authApi = inject(AuthApi);

  readonly currentUser = signal<User | null>(null);
  readonly token = signal<string | null>(null);

  private expiryTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    if (!isPlatformBrowser(this.platformId)) return;
    const token = localStorage.getItem(TOKEN_KEY);
    const user = this.readUser();
    if (token && user) {
      const expiresAt = tokenExpiresAt(token);
      if (expiresAt && Date.now() >= expiresAt) {
        this.clearStorage();
      } else {
        this.token.set(token);
        this.currentUser.set(user);
        this.scheduleExpiry(token);
      }
    }
  }

  private readUser(): User | null {
    try {
      return JSON.parse(localStorage.getItem(USER_KEY) ?? 'null');
    } catch {
      return null;
    }
  }

  private clearStorage() {
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(TOKEN_KEY);
  }

  private scheduleExpiry(token: string) {
    if (this.expiryTimer) clearTimeout(this.expiryTimer);
    const expiresAt = tokenExpiresAt(token);
    if (!expiresAt) return;
    const delay = expiresAt - Date.now();
    if (delay <= 0) { this.logout(); return; }
    this.expiryTimer = setTimeout(() => this.logout(), delay);
  }

  private persist(user: User, token: string) {
    this.currentUser.set(user);
    this.token.set(token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    localStorage.setItem(TOKEN_KEY, token);
    this.scheduleExpiry(token);
  }

  signIn(email: string, password: string) {
    return this.authApi.login(email, password).pipe(
      tap(res => this.persist({
        id: tokenUserId(res.token), email: res.email,
        firstName: res.first_name, lastName: res.last_name,
      }, res.token)),
    );
  }

  register(email: string, firstName: string, lastName: string, password: string) {
    return this.authApi.register(email, firstName, lastName, password).pipe(
      tap(res => this.persist({
        id: tokenUserId(res.token), email: res.email,
        firstName: res.first_name, lastName: res.last_name,
      }, res.token)),
    );
  }

  logout() {
    if (this.expiryTimer) { clearTimeout(this.expiryTimer); this.expiryTimer = null; }
    this.currentUser.set(null);
    this.token.set(null);
    if (isPlatformBrowser(this.platformId)) this.clearStorage();
  }
}
