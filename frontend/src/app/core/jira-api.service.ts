import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export type IssueStatus = 'Backlog' | 'Todo' | 'InProgress' | 'InReview' | 'Done';
export type IssuePriority = 'Lowest' | 'Low' | 'Medium' | 'High' | 'Highest';
export type IssueType = 'Story' | 'Task' | 'Bug' | 'Epic';
export interface UserSummary { id:number; name:string; avatar:string; }
export interface Comment { id:number; body:string; author:string; avatar:string; createdAt:string; }
export interface Activity { id:number; type:string; oldValue?:string; newValue?:string; actor:string; createdAt:string; }
export interface Issue { id:number; key:string; title:string; description:string; status:IssueStatus; priority:IssuePriority; type:IssueType; storyPoints:number; assignee?:UserSummary; updatedAt:string; }
export interface IssueDetails extends Issue { comments: Comment[]; activities: Activity[]; }
export interface Project { id:number; key:string; name:string; description:string; issueCount:number; }
export interface CreateIssueRequest { projectId:number; title:string; description:string; status:IssueStatus; priority:IssuePriority; type:IssueType; storyPoints:number; assigneeId?:number; sprintId?:number; }

@Injectable({ providedIn: 'root' })
export class JiraApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:7001/api';
  projects(): Observable<Project[]> { return this.http.get<Project[]>(`${this.baseUrl}/projects`); }
  issues(search = '', status?: IssueStatus): Observable<Issue[]> { let params = new HttpParams(); if (search) params = params.set('search', search); if (status) params = params.set('status', status); return this.http.get<Issue[]>(`${this.baseUrl}/issues`, { params }); }
  issue(id:number): Observable<IssueDetails> { return this.http.get<IssueDetails>(`${this.baseUrl}/issues/${id}`); }
  move(id:number, status:IssueStatus): Observable<void> { return this.http.patch<void>(`${this.baseUrl}/issues/${id}/status`, { status }); }
  create(request: CreateIssueRequest): Observable<number> { return this.http.post<number>(`${this.baseUrl}/issues`, request); }
  comment(id:number, body:string): Observable<number> { return this.http.post<number>(`${this.baseUrl}/issues/${id}/comments`, { body }); }
}
