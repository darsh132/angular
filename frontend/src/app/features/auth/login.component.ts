import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({ selector: 'app-login', standalone: true, imports: [FormsModule], templateUrl: './login.component.html' })
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  email = 'darshan@example.com';
  password = 'demo123';
  readonly submitting = signal(false);
  readonly error = signal('');

  submit(): void {
    this.error.set(''); this.submitting.set(true);
    this.auth.login(this.email, this.password).subscribe({ next: () => this.router.navigateByUrl('/board'), error: () => { this.error.set('Invalid email or password.'); this.submitting.set(false); }, complete: () => this.submitting.set(false) });
  }
}
