import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { JiraApiService, Issue } from '../../core/jira-api.service';

@Component({ selector: 'app-backlog', standalone: true, imports: [CommonModule, RouterLink], templateUrl: './backlog.component.html' })
export class BacklogComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  readonly issues = signal<Issue[]>([]);
  ngOnInit(): void { this.api.issues().subscribe(x => this.issues.set(x.filter(i => i.status === 'Backlog' || i.status === 'Todo'))); }
}
