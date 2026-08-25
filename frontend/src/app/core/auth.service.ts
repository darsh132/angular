import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, finalize, shareReplay, tap } from 'rxjs';

export interface AuthUser { id: number; name: string; email: string; avatar: string; role: string; }
export interface LoginResponse { token: string; user: AuthUser; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:7001/api';
  private readonly userState = signal<AuthUser | null>(this.readUser());
  private refreshInFlight?: Observable<LoginResponse>;
  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userState());

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, { email, password }, { withCredentials: true }).pipe(
      tap(response => this.storeSession(response))
    );
  }

  refresh(): Observable<LoginResponse> {
    if (!this.refreshInFlight) {
      this.refreshInFlight = this.http.post<LoginResponse>(`${this.baseUrl}/auth/refresh`, {}, { withCredentials: true }).pipe(
        tap(response => this.storeSession(response)),
        finalize(() => { this.refreshInFlight = undefined; }),
        shareReplay(1)
      );
    }
    return this.refreshInFlight;
  }

  logout(): void {
    this.http.post(`${this.baseUrl}/auth/revoke`, {}, { withCredentials: true }).subscribe({ error: () => undefined });
    localStorage.removeItem('jira_token'); localStorage.removeItem('jira_user'); this.userState.set(null);
  }

  token(): string | null { return localStorage.getItem('jira_token'); }
  private storeSession(response: LoginResponse): void { localStorage.setItem('jira_token', response.token); localStorage.setItem('jira_user', JSON.stringify(response.user)); this.userState.set(response.user); }
  private readUser(): AuthUser | null { try { const value = localStorage.getItem('jira_user'); return value ? JSON.parse(value) as AuthUser : null; } catch { return null; } }
}
