import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JiraApiService, Issue } from '../../core/jira-api.service';

@Component({ selector: 'app-sprints', standalone: true, imports: [CommonModule], templateUrl: './sprints.component.html' })
export class SprintsComponent implements OnInit {
  private readonly api = inject(JiraApiService);
  readonly issues = signal<Issue[]>([]);
  ngOnInit(): void { this.api.issues().subscribe(x => this.issues.set(x.filter(i => i.status !== 'Backlog'))); }
  done(): number { return this.issues().filter(i => i.status === 'Done').length; }
  progress(): number { return Math.round(this.done() * 100 / (this.issues().length || 1)); }
}
