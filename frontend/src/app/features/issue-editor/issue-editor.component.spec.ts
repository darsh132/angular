import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { JiraApiService } from '../../core/jira-api.service';
import { IssueEditorComponent } from './issue-editor.component';

describe('IssueEditorComponent', () => {
  let fixture: ComponentFixture<IssueEditorComponent>;
  const api = { update: () => of(void 0) } as unknown as JiraApiService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [IssueEditorComponent], providers: [{ provide: JiraApiService, useValue: api }] }).compileComponents();
    fixture = TestBed.createComponent(IssueEditorComponent);
    fixture.componentInstance.issue = { id: 1, key: 'DEMO-1', title: 'Test', description: 'Desc', status: 'Todo', priority: 'Medium', type: 'Task', storyPoints: 3, updatedAt: '', comments: [], activities: [] };
    fixture.detectChanges();
  });

  it('initializes form fields from the issue', () => {
    expect(fixture.componentInstance.title).toBe('Test');
    expect(fixture.componentInstance.storyPoints).toBe(3);
  });
});
