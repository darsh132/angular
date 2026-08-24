import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { JiraApiService, IssueDetails, UserOption } from '../../core/jira-api.service';
import { IssueEditorComponent } from './issue-editor.component';

@Component({ selector: 'app-issue-editor-page', standalone: true, imports: [CommonModule, RouterLink, IssueEditorComponent], template: `
<div class="max-w-4xl mx-auto p-4 lg:p-8">
  <div class="breadcrumbs text-sm mb-4"><ul><li><a routerLink="/board">Board</a></li><li>Edit issue</li></ul></div>
  <div *ngIf="loading()" class="flex justify-center p-16"><span class="loading loading-spinner loading-lg"></span></div>
  <ng-container *ngIf="!loading() && issue() as item">
    <div class="mb-6"><div class="font-mono text-sm text-primary">{{ item.key }}</div><h1 class="text-3xl font-bold">Edit issue</h1><p class="text-base-content/60">Update the issue fields through the .NET API.</p></div>
    <app-issue-editor [issue]="item" [users]="users()" (saved)="saved()" (cancelled)="cancel()"></app-issue-editor>
  </ng-container>
</div>` })
export class IssueEditorPageComponent implements OnInit {
  private readonly api=inject(JiraApiService); private readonly route=inject(ActivatedRoute);
  readonly issue=signal<IssueDetails|null>(null); readonly users=signal<UserOption[]>([]); readonly loading=signal(true); id=0;
  ngOnInit():void { this.id=Number(this.route.snapshot.paramMap.get('id')); this.api.users().subscribe(x=>this.users.set(x)); this.api.issue(this.id).subscribe({next:x=>this.issue.set(x),complete:()=>this.loading.set(false),error:()=>this.loading.set(false)}); }
  saved():void { window.history.back(); }
  cancel():void { window.history.back(); }
}
