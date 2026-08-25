import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { CdkDrag, CdkDropList, CdkDropListGroup, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { JiraApiService, Issue, IssueFilters, Sprint, Project } from '../../core/jira-api.service';
import { IssueQueryService } from '../../core/issue-query.service';
import { IssueQueryFilterComponent } from './issue-query-filter.component';

@Component({ selector: 'app-backlog', standalone: true, imports: [CommonModule, FormsModule, RouterLink, CdkDrag, CdkDropList, CdkDropListGroup, IssueQueryFilterComponent], templateUrl: './backlog.component.html' })
export class BacklogComponent implements OnInit {
  private readonly api = inject(JiraApiService); private readonly query = inject(IssueQueryService); private readonly route = inject(ActivatedRoute); private readonly router = inject(Router);
  readonly issues = signal<Issue[]>([]); readonly sprints = signal<Sprint[]>([]); readonly projects = signal<Project[]>([]); readonly filters = signal<IssueFilters>({}); readonly loading = signal(true); readonly busy = signal<number | null>(null);
  projectId = 0;
  ngOnInit(): void { this.route.queryParamMap.subscribe(params => { const projectId = Number(params.get('projectId')); this.projectId = projectId || this.projectId; this.query.setFilters(this.readFilters(params)); this.filters.set(this.query.filters()); this.loadProjects(); }); }
  private readFilters(params: any): IssueFilters { const f: IssueFilters = {}; const keys = ['sprintId','assigneeId']; for (const k of keys) { const n = Number(params.get(k)); if (n) (f as any)[k] = n; } for (const k of ['status','priority','type','search']) { const v = params.get(k); if (v) (f as any)[k] = v; } return f; }
  loadProjects(): void { this.api.projects().subscribe({ next: projects => { this.projects.set(projects); if (!this.projectId && projects.length) this.projectId = projects[0].id; this.load(); }, error: () => this.loading.set(false) }); }
  load(): void { if (!this.projectId) return; this.loading.set(true); const f = { ...this.query.filters(), projectId: this.projectId }; this.query.setFilters(f); this.filters.set(f); this.api.issues(f).subscribe({ next: items => this.issues.set(items), error: e => console.error(e), complete: () => this.loading.set(false) }); this.api.sprints(this.projectId).subscribe(items => this.sprints.set(items)); }
  setFilters(filters: IssueFilters): void { const next = { ...filters, projectId: this.projectId }; this.query.setFilters(next); this.filters.set(next); void this.router.navigate([], { relativeTo: this.route, queryParams: next, replaceUrl: true }); }
  sprintIssues(sprintId: number): Issue[] { return this.issues().filter(i => i.sprintId === sprintId); }
  backlogIssues(): Issue[] { return this.issues().filter(i => !i.sprintId); }
  drop(event: CdkDragDrop<Issue[]>, targetSprintId?: number): void { if (event.previousContainer === event.container && !targetSprintId) { moveItemInArray(event.container.data, event.previousIndex, event.currentIndex); this.issues.set([...this.issues()]); return; } const issue = event.previousContainer.data[event.previousIndex]; if (!issue || !this.projectId || this.busy() === issue.id) return; this.busy.set(issue.id); const previousSprint = issue.sprintId; if (targetSprintId) { issue.sprintId = targetSprintId; this.issues.set([...this.issues()]); this.api.assignIssueToSprint(this.projectId, targetSprintId, issue.id).subscribe({ error: () => { issue.sprintId = previousSprint; this.issues.set([...this.issues()]); this.busy.set(null); }, complete: () => this.busy.set(null) }); } else { issue.sprintId = undefined; this.issues.set([...this.issues()]); this.api.removeIssueFromSprint(this.projectId, issue.id).subscribe({ error: () => { issue.sprintId = previousSprint; this.issues.set([...this.issues()]); this.busy.set(null); }, complete: () => this.busy.set(null) }); } }
}
