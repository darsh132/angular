import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JiraApiService, IssueDetails, IssuePriority, IssueType, UserOption } from '../../core/jira-api.service';

@Component({ selector: 'app-issue-editor', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './issue-editor.component.html' })
export class IssueEditorComponent {
  private readonly api = inject(JiraApiService);
  @Input({ required: true }) issue!: IssueDetails;
  @Input() users: UserOption[] = [];
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
  readonly priorities: IssuePriority[] = ['Lowest','Low','Medium','High','Highest'];
  readonly types: IssueType[] = ['Story','Task','Bug','Epic'];
  title = ''; description = ''; priority: IssuePriority = 'Medium'; type: IssueType = 'Task'; storyPoints = 0; assigneeId?: number; saving = false;

  ngOnChanges(): void { if (this.issue) { this.title=this.issue.title; this.description=this.issue.description; this.priority=this.issue.priority; this.type=this.issue.type; this.storyPoints=this.issue.storyPoints; this.assigneeId=this.issue.assignee?.id; } }
  save(): void { if (!this.title.trim() || this.storyPoints < 0) return; this.saving=true; this.api.update(this.issue.id,{title:this.title.trim(),description:this.description,priority:this.priority,type:this.type,storyPoints:this.storyPoints,assigneeId:this.assigneeId,sprintId:this.issue.sprintId}).subscribe({next:()=>this.saved.emit(),complete:()=>this.saving=false,error:()=>this.saving=false}); }
}
