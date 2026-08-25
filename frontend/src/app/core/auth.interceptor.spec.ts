import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let testing: HttpTestingController;
  const auth = { token: () => 'test-jwt', logout: jasmine.createSpy('logout') };
  const router = { navigate: jasmine.createSpy('navigate') };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting(), { provide: AuthService, useValue: auth }, { provide: Router, useValue: router }] });
    http = TestBed.inject(HttpClient); testing = TestBed.inject(HttpTestingController);
  });

  afterEach(() => testing.verify());

  it('adds bearer token', () => {
    http.get('/api/issues').subscribe();
    const request = testing.expectOne('/api/issues');
    expect(request.request.headers.get('Authorization')).toBe('Bearer test-jwt');
    request.flush([]);
  });

  it('logs out and navigates to login on 401', () => {
    http.get('/api/issues').subscribe({ error: () => undefined });
    const request = testing.expectOne('/api/issues');
    request.flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
