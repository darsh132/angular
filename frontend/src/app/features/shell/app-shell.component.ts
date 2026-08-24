import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app-shell.component.html'
})
export class AppShellComponent {
  readonly auth = inject(AuthService);
  theme(value: string): void { document.documentElement.setAttribute('data-theme', value); }
}
