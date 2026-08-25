import { Injectable, inject } from '@angular/core';
import { AuthService } from './auth.service';

export type ProjectRole = 'Viewer' | 'Member' | 'Manager';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly auth = inject(AuthService);

  canManageGlobally(): boolean { return this.auth.user()?.role === 'Admin'; }
  canView(role?: ProjectRole): boolean { return this.canManageGlobally() || role !== undefined; }
  canEdit(role?: ProjectRole): boolean { return this.canManageGlobally() || role === 'Member' || role === 'Manager'; }
  canManage(role?: ProjectRole): boolean { return this.canManageGlobally() || role === 'Manager'; }
  currentUserId(): number | null { return this.auth.user()?.id ?? null; }
}
