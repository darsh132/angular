import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JiraApiService, Issue, IssuePriority, IssueStatus, IssueType } from '../../core/jira-api.service';

@Component({ selector: 'app-board', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './board.component.html' })
export class BoardComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  readonly issues = signal<Issue[]>([]);
  readonly search = signal('');
  readonly statuses: IssueStatus[] = ['Backlog', 'Todo', 'InProgress', 'InReview', 'Done'];
  readonly priorities: IssuePriority[] = ['Lowest', 'Low', 'Medium', 'High', 'Highest'];
  readonly types: IssueType[] = ['Story', 'Task', 'Bug', 'Epic'];
  readonly loading = signal(true);
  readonly createOpen = signal(false);
  readonly saving = signal(false);
  newIssue = { projectId: 1, title: '', description: '', status: 'Todo' as IssueStatus, priority: 'Medium' as IssuePriority, type: 'Task' as IssueType, storyPoints: 0 };

  ngOnInit(): void { this.load(); }
  load(): void { this.loading.set(true); this.api.issues(this.search()).subscribe({ next: x => this.issues.set(x), error: e => console.error(e), complete: () => this.loading.set(false) }); }
  byStatus(status: IssueStatus): Issue[] { return this.issues().filter(i => i.status === status); }
  move(issue: Issue, status: IssueStatus): void { if (issue.status === status) return; this.api.move(issue.id, status).subscribe(() => this.load()); }
  openCreate(): void { this.newIssue = { projectId: 1, title: '', description: '', status: 'Todo', priority: 'Medium', type: 'Task', storyPoints: 0 }; this.createOpen.set(true); }
  closeCreate(): void { if (!this.saving()) this.createOpen.set(false); }
  createIssue(): void {
    if (!this.newIssue.title.trim()) return;
    this.saving.set(true);
    this.api.create({ ...this.newIssue, title: this.newIssue.title.trim() }).subscribe({ next: () => { this.createOpen.set(false); this.load(); }, error: e => console.error(e), complete: () => this.saving.set(false) });
  }
  theme(theme: string): void { document.documentElement.setAttribute('data-theme', theme); }
  priorityClass(priority: string): string { return ({ Highest:'badge-error', High:'badge-warning', Medium:'badge-info', Low:'badge-success' } as Record<string,string>)[priority] ?? 'badge-ghost'; }
}
