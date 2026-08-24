import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CdkDrag, CdkDropList, CdkDropListGroup, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { JiraApiService, Issue, Sprint, Project } from '../../core/jira-api.service';

@Component({ selector: 'app-backlog', standalone: true, imports: [CommonModule, FormsModule, RouterLink, CdkDrag, CdkDropList, CdkDropListGroup], templateUrl: './backlog.component.html' })
export class BacklogComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  readonly issues = signal<Issue[]>([]);
  readonly sprints = signal<Sprint[]>([]);
  readonly projects = signal<Project[]>([]);
  readonly loading = signal(true);
  readonly busy = signal<number | null>(null);
  projectId = 0;

  ngOnInit(): void {
    this.api.projects().subscribe({ next: projects => { this.projects.set(projects); if (projects.length) { this.projectId = projects[0].id; this.load(); } }, complete: () => this.loading.set(false) });
  }

  load(): void {
    if (!this.projectId) return;
    this.loading.set(true);
    this.api.issues().subscribe({ next: items => this.issues.set(items), complete: () => this.loading.set(false) });
    this.api.sprints(this.projectId).subscribe(items => this.sprints.set(items));
  }

  sprintIssues(sprintId: number): Issue[] { return this.issues().filter(i => i.sprintId === sprintId); }
  backlogIssues(): Issue[] { return this.issues().filter(i => !i.sprintId); }

  drop(event: CdkDragDrop<Issue[]>, targetSprintId?: number): void {
    if (event.previousContainer === event.container && !targetSprintId) { moveItemInArray(event.container.data, event.previousIndex, event.currentIndex); this.issues.set([...this.issues()]); return; }
    const issue = event.previousContainer.data[event.previousIndex];
    if (!issue || !this.projectId || this.busy() === issue.id) return;
    this.busy.set(issue.id);
    const previousSprint = issue.sprintId;
    if (targetSprintId) {
      issue.sprintId = targetSprintId;
      this.issues.set([...this.issues()]);
      this.api.assignIssueToSprint(this.projectId, targetSprintId, issue.id).subscribe({ error: () => { issue.sprintId = previousSprint; this.issues.set([...this.issues()]); this.busy.set(null); }, complete: () => this.busy.set(null) });
    } else {
      issue.sprintId = undefined;
      this.issues.set([...this.issues()]);
      this.api.removeIssueFromSprint(this.projectId, issue.id).subscribe({ error: () => { issue.sprintId = previousSprint; this.issues.set([...this.issues()]); this.busy.set(null); }, complete: () => this.busy.set(null) });
    }
  }
}
