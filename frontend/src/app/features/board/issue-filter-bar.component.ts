import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IssueFilters, IssuePriority, IssueStatus, IssueType } from '../../core/jira-api.service';

@Component({ selector: 'app-issue-filter-bar', standalone: true, imports: [FormsModule], template: `
<section class="rounded-box border border-base-300 bg-base-100 p-3 shadow-sm">
  <div class="flex flex-wrap items-end gap-3">
    <label class="form-control min-w-56 flex-1"><span class="label-text mb-1 text-xs font-semibold uppercase opacity-60">Search</span><input class="input input-bordered w-full" [(ngModel)]="draft.search" (ngModelChange)="changed()" placeholder="Search issues or keys" /></label>
    <label class="form-control w-40"><span class="label-text mb-1 text-xs font-semibold uppercase opacity-60">Status</span><select class="select select-bordered" [(ngModel)]="draft.status" (ngModelChange)="changed()"><option [ngValue]="undefined">All</option><option *ngFor="let x of statuses" [ngValue]="x">{{x}}</option></select></label>
    <label class="form-control w-40"><span class="label-text mb-1 text-xs font-semibold uppercase opacity-60">Priority</span><select class="select select-bordered" [(ngModel)]="draft.priority" (ngModelChange)="changed()"><option [ngValue]="undefined">All</option><option *ngFor="let x of priorities" [ngValue]="x">{{x}}</option></select></label>
    <label class="form-control w-36"><span class="label-text mb-1 text-xs font-semibold uppercase opacity-60">Type</span><select class="select select-bordered" [(ngModel)]="draft.type" (ngModelChange)="changed()"><option [ngValue]="undefined">All</option><option *ngFor="let x of types" [ngValue]="x">{{x}}</option></select></label>
    <button class="btn btn-ghost" type="button" (click)="clear()">Clear</button>
  </div>
</section>` })
export class IssueFilterBarComponent {
  @Input() value: IssueFilters = {};
  @Output() valueChange = new EventEmitter<IssueFilters>();
  readonly statuses: IssueStatus[] = ['Backlog','Todo','InProgress','InReview','Done'];
  readonly priorities: IssuePriority[] = ['Lowest','Low','Medium','High','Highest'];
  readonly types: IssueType[] = ['Story','Task','Bug','Epic'];
  draft: IssueFilters = {};
  ngOnChanges(): void { this.draft = { ...this.value }; }
  changed(): void { this.valueChange.emit(this.clean({ ...this.draft })); }
  clear(): void { this.draft = {}; this.valueChange.emit({}); }
  private clean(x: IssueFilters): IssueFilters { return Object.fromEntries(Object.entries(x).filter(([,v]) => v !== undefined && v !== null && v !== '')) as IssueFilters; }
}
