import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { AppShellComponent } from './features/shell/app-shell.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { path: '', component: AppShellComponent, canActivate: [authGuard], children: [
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
    { path: 'board', loadComponent: () => import('./features/board/board.component').then(m => m.BoardComponent) },
    { path: 'backlog', loadComponent: () => import('./features/backlog/backlog.component').then(m => m.BacklogComponent) },
    { path: 'sprints', loadComponent: () => import('./features/sprints/sprints.component').then(m => m.SprintsComponent) },
    { path: 'issues/:id', loadComponent: () => import('./features/issue-detail/issue-detail.component').then(m => m.IssueDetailComponent) },
    { path: 'issues/:id/edit', loadComponent: () => import('./features/issue-editor/issue-editor-page.component').then(m => m.IssueEditorPageComponent) }
  ]},
  { path: '**', redirectTo: '' }
];
