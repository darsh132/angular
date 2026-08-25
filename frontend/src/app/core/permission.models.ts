import { ProjectRole } from './permission.service';

export interface ProjectMember {
  userId: number;
  name: string;
  email: string;
  avatar: string;
  role: ProjectRole;
}
