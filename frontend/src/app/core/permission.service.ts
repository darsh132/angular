import { Injectable, computed, inject, signal } from '@angular/core';
import { AuthService } from './auth.service';
export type ProjectRole = 'Viewer' | 'Member' | 'Manager';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly auth = inject(AuthService);
  private readonly projectRoleState = signal<ProjectRole | undefined>(undefined);
  readonly projectRole = this.projectRoleState.asReadonly();
  readonly isAdmin = computed(() => this.auth.user()?.role === 'Admin');

  setProjectRole(role: ProjectRole | undefined): void { this.projectRoleState.set(role); }
  currentUserId(): number | null { return this.auth.user()?.id ?? null; }
  canManageGlobally(): boolean { return this.isAdmin(); }
  canView(role?: ProjectRole): boolean { return this.canManageGlobally() || (role ?? this.projectRoleState()) !== undefined; }
  canEdit(role?: ProjectRole): boolean { const effective = role ?? this.projectRoleState(); return this.canManageGlobally() || effective === 'Member' || effective === 'Manager'; }
  canManage(role?: ProjectRole): boolean { const effective = role ?? this.projectRoleState(); return this.canManageGlobally() || effective === 'Manager'; }
}
