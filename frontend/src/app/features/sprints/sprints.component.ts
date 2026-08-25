import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { JiraApiService, Issue, Project, Sprint, SprintStatus } from '../../core/jira-api.service';
import { PermissionService } from '../../core/permission.service';

@Component({ selector: 'app-sprints', standalone: true, imports: [CommonModule, FormsModule, RouterLink], templateUrl: './sprints.component.html' })
export class SprintsComponent implements OnInit {
  private readonly api = inject(JiraApiService); readonly permissions = inject(PermissionService);
  readonly projects = signal<Project[]>([]); readonly sprints = signal<Sprint[]>([]); readonly issues = signal<Issue[]>([]); readonly loading = signal(true); readonly saving = signal(false); readonly action = signal<number | null>(null);
  projectId = 0; showCreate = false; name = ''; goal = ''; startDate = ''; endDate = '';
  ngOnInit(): void { this.api.projects().subscribe({ next: projects => { this.projects.set(projects); if (projects.length) { this.projectId = projects[0].id; this.load(); } }, complete: () => this.loading.set(false) }); }
  load(): void { if (!this.projectId) return; this.loading.set(true); this.api.sprints(this.projectId).subscribe({ next: sprints => this.sprints.set(sprints), error: e => console.error(e), complete: () => this.loading.set(false) }); this.api.issues({ projectId: this.projectId }).subscribe({ next: x => this.issues.set(x), error: e => console.error(e) }); }
  create(): void { if (!this.name.trim() || !this.startDate || !this.endDate || !this.permissions.canManage()) return; this.saving.set(true); this.api.createSprint(this.projectId, { name: this.name.trim(), goal: this.goal.trim() || undefined, startDate: this.startDate, endDate: this.endDate }).subscribe({ next: () => { this.reset(); this.load(); }, error: e => console.error(e), complete: () => this.saving.set(false) }); }
  start(sprint: Sprint): void { if (!this.permissions.canManage()) return; this.action.set(sprint.id); this.api.startSprint(this.projectId, sprint.id).subscribe({ next: () => this.load(), error: e => console.error(e), complete: () => this.action.set(null) }); }
  complete(sprint: Sprint): void { if (!this.permissions.canManage()) return; this.action.set(sprint.id); this.api.completeSprint(this.projectId, sprint.id).subscribe({ next: () => this.load(), error: e => console.error(e), complete: () => this.action.set(null) }); }
  assign(issue: Issue, sprint: Sprint): void { if (!this.permissions.canEdit()) return; this.action.set(issue.id); this.api.assignIssueToSprint(this.projectId, sprint.id, issue.id).subscribe({ next: () => this.load(), error: e => console.error(e), complete: () => this.action.set(null) }); }
  remove(issue: Issue): void { if (!this.permissions.canEdit()) return; this.action.set(issue.id); this.api.removeIssueFromSprint(this.projectId, issue.id).subscribe({ next: () => this.load(), error: e => console.error(e), complete: () => this.action.set(null) }); }
  sprintIssues(sprintId: number): Issue[] { return this.issues().filter(i => i.sprintId === sprintId); }
  committedPoints(items: Issue[]): number { return items.reduce((sum, item) => sum + Math.max(0, item.storyPoints), 0); }
  completedPoints(items: Issue[]): number { return items.filter(i => i.status === 'Done').reduce((sum, item) => sum + Math.max(0, item.storyPoints), 0); }
  remainingPoints(items: Issue[]): number { return Math.max(0, this.committedPoints(items) - this.completedPoints(items)); }
  progress(items: Issue[]): number { const committed = this.committedPoints(items); return committed ? Math.min(100, Math.round(this.completedPoints(items) * 100 / committed)) : 0; }
  statusLabel(status: SprintStatus): string { return status === 'Active' ? 'Active' : status; }
  private reset(): void { this.showCreate = false; this.name = ''; this.goal = ''; this.startDate = ''; this.endDate = ''; }
}
