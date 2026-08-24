import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export type IssueStatus = 'Backlog' | 'Todo' | 'InProgress' | 'InReview' | 'Done';
export type IssuePriority = 'Lowest' | 'Low' | 'Medium' | 'High' | 'Highest';
export type IssueType = 'Story' | 'Task' | 'Bug' | 'Epic';
export type SprintStatus = 'Planned' | 'Active' | 'Completed';
export interface UserSummary { id:number; name:string; avatar:string; } export interface UserOption { id:number; name:string; avatar:string; }
export interface Comment { id:number; body:string; author:string; avatar:string; createdAt:string; } export interface Activity { id:number; type:string; oldValue?:string; newValue?:string; actor:string; avatar:string; createdAt:string; }
export interface Issue { id:number; key:string; title:string; description:string; status:IssueStatus; priority:IssuePriority; type:IssueType; storyPoints:number; assignee?:UserSummary; sprintId?:number; updatedAt:string; }
export interface IssueDetails extends Issue { comments:Comment[]; activities:Activity[]; } export interface Project { id:number; key:string; name:string; description:string; issueCount:number; }
export interface Sprint { id:number; name:string; goal?:string; status:SprintStatus; projectId:number; startDate:string; endDate:string; issues?:Issue[]; }
export interface IssueFilters { projectId?:number; sprintId?:number; assigneeId?:number; status?:IssueStatus; priority?:IssuePriority; type?:IssueType; search?:string; }
export interface CreateIssueRequest { projectId:number; title:string; description:string; status:IssueStatus; priority:IssuePriority; type:IssueType; storyPoints:number; assigneeId?:number; sprintId?:number; }
export interface UpdateIssueRequest { title:string; description:string; priority:IssuePriority; type:IssueType; storyPoints:number; assigneeId?:number; sprintId?:number; } export interface CreateSprintRequest { name:string; goal?:string; startDate:string; endDate:string; }

@Injectable({ providedIn:'root' })
export class JiraApiService {
  private readonly http=inject(HttpClient); private readonly baseUrl='https://localhost:7001/api';
  projects():Observable<Project[]> { return this.http.get<Project[]>(`${this.baseUrl}/projects`); } users():Observable<UserOption[]> { return this.http.get<UserOption[]>(`${this.baseUrl}/users`); }
  issues(filters:IssueFilters={}):Observable<Issue[]> { let p=new HttpParams(); Object.entries(filters).forEach(([k,v])=>{if(v!==undefined&&v!==null&&v!=='')p=p.set(k,String(v));}); return this.http.get<Issue[]>(`${this.baseUrl}/issues`,{params:p}); }
  issue(id:number):Observable<IssueDetails>{return this.http.get<IssueDetails>(`${this.baseUrl}/issues/${id}`);} move(id:number,status:IssueStatus):Observable<void>{return this.http.patch<void>(`${this.baseUrl}/issues/${id}/status`,{status});}
  create(request:CreateIssueRequest):Observable<number>{return this.http.post<number>(`${this.baseUrl}/issues`,request);} update(id:number,request:UpdateIssueRequest):Observable<void>{return this.http.put<void>(`${this.baseUrl}/issues/${id}`,request);} comment(id:number,body:string):Observable<number>{return this.http.post<number>(`${this.baseUrl}/issues/${id}/comments`,{body});}
  sprints(projectId:number):Observable<Sprint[]>{return this.http.get<Sprint[]>(`${this.baseUrl}/projects/${projectId}/sprints`);} createSprint(projectId:number,r:CreateSprintRequest):Observable<Sprint>{return this.http.post<Sprint>(`${this.baseUrl}/projects/${projectId}/sprints`,r);} startSprint(projectId:number,sprintId:number):Observable<Sprint>{return this.http.post<Sprint>(`${this.baseUrl}/projects/${projectId}/sprints/${sprintId}/start`,{});} completeSprint(projectId:number,sprintId:number):Observable<Sprint>{return this.http.post<Sprint>(`${this.baseUrl}/projects/${projectId}/sprints/${sprintId}/complete`,{});} assignIssueToSprint(projectId:number,sprintId:number,issueId:number):Observable<void>{return this.http.post<void>(`${this.baseUrl}/projects/${projectId}/sprints/${sprintId}/issues/${issueId}`,{});} removeIssueFromSprint(projectId:number,issueId:number):Observable<void>{return this.http.delete<void>(`${this.baseUrl}/projects/${projectId}/sprints/issues/${issueId}`);}
}
