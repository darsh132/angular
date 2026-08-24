import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { JiraApiService, IssueDetails, IssueStatus } from '../../core/jira-api.service';

@Component({ selector: 'app-issue-detail', standalone: true, imports: [CommonModule, FormsModule, RouterLink], templateUrl: './issue-detail.component.html' })
export class IssueDetailComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  private readonly route = inject(ActivatedRoute);
  readonly issue = signal<IssueDetails | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly statuses: IssueStatus[] = ['Backlog','Todo','InProgress','InReview','Done'];
  comment = '';
  id = 0;
  ngOnInit(): void { this.id = Number(this.route.snapshot.paramMap.get('id')); this.load(); }
  load(): void { this.loading.set(true); this.api.issue(this.id).subscribe({ next: x => this.issue.set(x), complete: () => this.loading.set(false), error: () => this.loading.set(false) }); }
  move(status: IssueStatus): void { this.saving.set(true); this.api.move(this.id, status).subscribe(() => { this.saving.set(false); this.load(); }); }
  addComment(): void { if (!this.comment.trim()) return; this.saving.set(true); this.api.comment(this.id, this.comment.trim()).subscribe(() => { this.comment = ''; this.saving.set(false); this.load(); }); }
}
