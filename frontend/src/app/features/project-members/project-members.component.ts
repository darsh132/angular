import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JiraApiService } from '../../core/jira-api.service';
import { PermissionService, ProjectRole } from '../../core/permission.service';
import { ProjectMember } from '../../core/permission.models';

@Component({selector:'app-project-members',standalone:true,imports:[CommonModule,FormsModule],templateUrl:'./project-members.component.html'})
export class ProjectMembersComponent implements OnInit {
  private readonly api=inject(JiraApiService); readonly permissions=inject(PermissionService);
  readonly members=signal<ProjectMember[]>([]); readonly loading=signal(true); readonly saving=signal(false); readonly roles:ProjectRole[]=['Viewer','Member','Manager'];
  projectId=1; error='';
  ngOnInit():void { this.load(); }
  load():void { this.loading.set(true); this.api.projectMembers(this.projectId).subscribe({next:x=>this.members.set(x),error:()=>this.error='Unable to load project members.',complete:()=>this.loading.set(false)}); }
  changeRole(member:ProjectMember, role:ProjectRole):void { if(member.role===role)return; this.saving.set(true); this.api.upsertProjectMember(this.projectId,member.userId,role).subscribe({next:()=>this.load(),error:()=>this.error='Unable to change role.',complete:()=>this.saving.set(false)}); }
  remove(member:ProjectMember):void { if(!confirm(`Remove ${member.name} from this project?`))return; this.saving.set(true); this.api.removeProjectMember(this.projectId,member.userId).subscribe({next:()=>this.load(),error:()=>this.error='Unable to remove member.',complete:()=>this.saving.set(false)}); }
}
