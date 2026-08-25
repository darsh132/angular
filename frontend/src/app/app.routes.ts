import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { AppShellComponent } from './features/shell/app-shell.component';
export const routes: Routes = [
 { path:'login',loadComponent:()=>import('./features/auth/login.component').then(m=>m.LoginComponent) },
 { path:'',component:AppShellComponent,canActivate:[authGuard],children:[
  {path:'',pathMatch:'full',redirectTo:'dashboard'},
  {path:'dashboard',loadComponent:()=>import('./features/dashboard/project-dashboard.component').then(m=>m.ProjectDashboardComponent)},
  {path:'board',loadComponent:()=>import('./features/board/board.component').then(m=>m.BoardComponent)},
  {path:'backlog',loadComponent:()=>import('./features/backlog/backlog.component').then(m=>m.BacklogComponent)},
  {path:'sprints',loadComponent:()=>import('./features/sprints/sprints.component').then(m=>m.SprintsComponent)},
  {path:'members',loadComponent:()=>import('./features/project-members/project-members.component').then(m=>m.ProjectMembersComponent)},
  {path:'issues/:id',loadComponent:()=>import('./features/issue-detail/issue-detail.component').then(m=>m.IssueDetailComponent)}
 ]}, {path:'**',redirectTo:''}
];
