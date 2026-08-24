import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { JiraApiService, Issue, Project, Sprint, SprintStatus } from '../../core/jira-api.service';

@Component({ selector: 'app-sprints', standalone: true, imports: [CommonModule, FormsModule, RouterLink], templateUrl: './sprints.component.html' })
export class SprintsComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  readonly projects = signal<Project[]>([]);
  readonly sprints = signal<Sprint[]>([]);
  readonly issues = signal<Issue[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  projectId = 0;
  showCreate = false;
  name = '';
  goal = '';
  startDate = '';
  endDate = '';

  ngOnInit(): void {
    this.api.projects().subscribe({ next: projects => { this.projects.set(projects); if (projects.length) { this.projectId = projects[0].id; this.load(); } }, complete: () => this.loading.set(false) });
  }

  load(): void {
    if (!this.projectId) return;
    this.loading.set(true);
    this.api.sprints(this.projectId).subscribe({ next: sprints => this.sprints.set(sprints), complete: () => this.loading.set(false) });
    this.api.issues().subscribe(x => this.issues.set(x));
  }

  create(): void {
    if (!this.name.trim() || !this.startDate || !this.endDate) return;
    this.saving.set(true);
    this.api.createSprint(this.projectId, { name: this.name.trim(), goal: this.goal.trim() || undefined, startDate: this.startDate, endDate: this.endDate })
      .subscribe({ next: () => { this.reset(); this.load(); }, complete: () => this.saving.set(false) });
  }

  start(sprint: Sprint): void { this.api.startSprint(this.projectId, sprint.id).subscribe(() => this.load()); }
  complete(sprint: Sprint): void { this.api.completeSprint(this.projectId, sprint.id).subscribe(() => this.load()); }
  assign(issue: Issue, sprint: Sprint): void { this.api.assignIssueToSprint(this.projectId, sprint.id, issue.id).subscribe(() => this.load()); }
  remove(issue: Issue): void { this.api.removeIssueFromSprint(this.projectId, issue.id).subscribe(() => this.load()); }

  sprintIssues(sprintId:number): Issue[] { return this.issues().filter(i => i.sprintId === sprintId); }
  points(items:Issue[]): number { return items.reduce((sum, item) => sum + item.storyPoints, 0); }
  done(items:Issue[]): number { return items.filter(i => i.status === 'Done').length; }
  progress(items:Issue[]): number { return Math.round(this.done(items) * 100 / (items.length || 1)); }
  statusLabel(status:SprintStatus): string { return status === 'InProgress' ? 'In Progress' : status; }

  private reset(): void { this.showCreate = false; this.name = ''; this.goal = ''; this.startDate = ''; this.endDate = ''; }
}
