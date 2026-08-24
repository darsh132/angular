import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routesV2: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login.component.fixed').then(m => m.LoginComponent) },
  { path: 'board', canActivate: [authGuard], loadComponent: () => import('./features/board/board.component').then(m => m.BoardComponent) },
  { path: '', pathMatch: 'full', redirectTo: 'board' },
  { path: '**', redirectTo: 'board' }
];
