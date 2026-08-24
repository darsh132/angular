import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { IssueEditorPageComponent } from './issue-editor-page.component';
import { JiraApiService } from '../../core/jira-api.service';

describe('IssueEditorPageComponent', () => {
  let fixture: ComponentFixture<IssueEditorPageComponent>;
  const api = {
    issue: () => of({ id: 1, key: 'DEMO-1', title: 'Test', description: '', status: 'Todo', priority: 'Medium', type: 'Task', storyPoints: 3, updatedAt: '', comments: [], activities: [] }),
    users: () => of([])
  } as unknown as JiraApiService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IssueEditorPageComponent],
      providers: [
        { provide: JiraApiService, useValue: api },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => '1' } } } },
        provideHttpClient(), provideHttpClientTesting()
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(IssueEditorPageComponent);
    fixture.detectChanges();
  });

  it('loads the issue editor page', () => {
    expect(fixture.componentInstance.issue()?.key).toBe('DEMO-1');
    expect(fixture.nativeElement.textContent).toContain('Edit issue');
  });
});
