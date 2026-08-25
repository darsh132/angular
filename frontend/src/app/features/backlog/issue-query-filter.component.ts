import { Component, EventEmitter, Input, Output } from '@angular/core';
import { IssueFilters } from '../../core/jira-api.service';
import { IssueFilterBarComponent } from '../board/issue-filter-bar.component';

@Component({ selector: 'app-issue-query-filter', standalone: true, imports: [IssueFilterBarComponent], template: `<app-issue-filter-bar [value]="value" (valueChange)="valueChange.emit($event)"></app-issue-filter-bar>` })
export class IssueQueryFilterComponent {
  @Input() value: IssueFilters = {};
  @Output() valueChange = new EventEmitter<IssueFilters>();
}
