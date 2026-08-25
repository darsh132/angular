import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.token();
  const request = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
  return next(request).pipe(catchError(error => {
    const isAuthEndpoint = request.url.includes('/auth/login') || request.url.includes('/auth/refresh') || request.url.includes('/auth/revoke');
    if (error.status !== 401 || isAuthEndpoint) return throwError(() => error);
    return auth.refresh().pipe(
      switchMap(response => next(request.clone({ setHeaders: { Authorization: `Bearer ${response.token}` } }))),
      catchError(refreshError => { auth.logout(); void router.navigate(['/login']); return throwError(() => refreshError); })
    );
  }));
};
