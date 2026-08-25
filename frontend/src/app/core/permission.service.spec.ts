import { TestBed } from '@angular/core/testing';
import { PermissionService } from './permission.service';
import { AuthService } from './auth.service';

describe('PermissionService', () => {
  let service: PermissionService;
  const auth = { user: () => ({ id: 1, name: 'Test', email: 'test@example.com', role: 'User' }) };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [PermissionService, { provide: AuthService, useValue: auth }] });
    service = TestBed.inject(PermissionService);
  });

  it('allows viewers to view but not edit or manage', () => {
    expect(service.canView('Viewer')).toBeTrue();
    expect(service.canEdit('Viewer')).toBeFalse();
    expect(service.canManage('Viewer')).toBeFalse();
  });

  it('allows members to edit but not manage', () => {
    expect(service.canView('Member')).toBeTrue();
    expect(service.canEdit('Member')).toBeTrue();
    expect(service.canManage('Member')).toBeFalse();
  });

  it('allows managers to manage', () => {
    expect(service.canView('Manager')).toBeTrue();
    expect(service.canEdit('Manager')).toBeTrue();
    expect(service.canManage('Manager')).toBeTrue();
  });
});
