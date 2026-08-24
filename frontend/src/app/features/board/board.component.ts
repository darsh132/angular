import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JiraApiService, Issue, IssueStatus } from '../../core/jira-api.service';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './board.component.html'
})
export class BoardComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  readonly issues = signal<Issue[]>([]);
  readonly search = signal('');
  readonly statuses: IssueStatus[] = ['Backlog', 'Todo', 'InProgress', 'InReview', 'Done'];
  readonly loading = signal(true);

  ngOnInit(): void { this.load(); }
  load(): void { this.loading.set(true); this.api.issues(this.search()).subscribe({ next: x => this.issues.set(x), error: e => console.error(e), complete: () => this.loading.set(false) }); }
  byStatus(status: IssueStatus): Issue[] { return this.issues().filter(i => i.status === status); }
  move(issue: Issue, status: IssueStatus): void { if (issue.status === status) return; this.api.move(issue.id, status).subscribe(() => this.load()); }
  theme(theme: string): void { document.documentElement.setAttribute('data-theme', theme); }
  priorityClass(priority: string): string { return ({ Highest:'badge-error', High:'badge-warning', Medium:'badge-info', Low:'badge-success' } as Record<string,string>)[priority] ?? 'badge-ghost'; }
}
