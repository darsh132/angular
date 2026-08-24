import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export type IssueStatus = 'Backlog' | 'Todo' | 'InProgress' | 'InReview' | 'Done';
export type IssuePriority = 'Lowest' | 'Low' | 'Medium' | 'High' | 'Highest';
export type IssueType = 'Story' | 'Task' | 'Bug' | 'Epic';
export interface Issue { id:number; key:string; title:string; description:string; status:IssueStatus; priority:IssuePriority; type:IssueType; assignee?:{ id:number; name:string; avatar:string }; updatedAt:string; }
export interface Project { id:number; key:string; name:string; description:string; issueCount:number; }
export interface CreateIssueRequest { title:string; description:string; status:IssueStatus; priority:IssuePriority; type:IssueType; assigneeId?:number; }

@Injectable({ providedIn: 'root' })
export class JiraApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:7001/api';

  projects(): Observable<Project[]> { return this.http.get<Project[]>(`${this.baseUrl}/projects`); }
  issues(search = '', status?: IssueStatus): Observable<Issue[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<Issue[]>(`${this.baseUrl}/issues`, { params });
  }
  move(id:number, status:IssueStatus): Observable<void> { return this.http.patch<void>(`${this.baseUrl}/issues/${id}/status`, { status }); }
  create(request: CreateIssueRequest): Observable<number> { return this.http.post<number>(`${this.baseUrl}/issues`, request); }
  login(email:string, password:string): Observable<{ token:string; user:{ id:number; name:string; email:string; avatar:string } }> { return this.http.post<any>(`${this.baseUrl}/auth/login`, { email, password }); }
}
