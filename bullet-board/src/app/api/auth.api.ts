import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface AuthResponse {
  token: string;
  email: string;
  first_name: string;
  last_name: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApi {
  private http = inject(HttpClient);

  login(email: string, password: string) {
    return this.http.post<AuthResponse>('/api/Auth/login', { email, password });
  }

  register(email: string, firstName: string, lastName: string, password: string) {
    return this.http.post<AuthResponse>('/api/Auth/register', {
      email,
      first_name: firstName,
      last_name: lastName,
      password,
    });
  }
}
