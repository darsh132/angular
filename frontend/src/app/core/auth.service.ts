import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

export interface AuthUser { id: number; name: string; email: string; avatar: string; }
export interface LoginResponse { token: string; user: AuthUser; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:7001/api';
  private readonly userState = signal<AuthUser | null>(this.readUser());
  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userState());

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, { email, password }).pipe(
      tap(response => { localStorage.setItem('jira_token', response.token); localStorage.setItem('jira_user', JSON.stringify(response.user)); this.userState.set(response.user); })
    );
  }

  logout(): void { localStorage.removeItem('jira_token'); localStorage.removeItem('jira_user'); this.userState.set(null); }
  token(): string | null { return localStorage.getItem('jira_token'); }
  private readUser(): AuthUser | null { try { const value = localStorage.getItem('jira_user'); return value ? JSON.parse(value) as AuthUser : null; } catch { return null; } }
}
