import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { JiraApiService, Issue, IssueStatus } from '../../core/jira-api.service';

@Component({ selector: 'app-dashboard', standalone: true, imports: [CommonModule, RouterLink], templateUrl: './dashboard.component.html' })
export class DashboardComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  readonly issues = signal<Issue[]>([]);
  readonly loading = signal(true);
  readonly statuses: IssueStatus[] = ['Backlog','Todo','InProgress','InReview','Done'];
  ngOnInit(): void { this.api.issues().subscribe({ next: x => this.issues.set(x), complete: () => this.loading.set(false), error: () => this.loading.set(false) }); }
  count(status: IssueStatus): number { return this.issues().filter(x => x.status === status).length; }
  percent(status: IssueStatus): number { const total = this.issues().length || 1; return Math.round(this.count(status) * 100 / total); }
}
