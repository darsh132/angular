import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, switchMap, tap } from 'rxjs';
import { JiraApiService, Issue, IssueFilters } from './jira-api.service';

@Injectable({ providedIn: 'root' })
export class IssueQueryService {
  private readonly api = inject(JiraApiService);
  private readonly filtersSubject = new BehaviorSubject<IssueFilters>({});
  readonly filters$ = this.filtersSubject.asObservable();

  setFilters(filters: IssueFilters): void { this.filtersSubject.next({ ...filters }); }
  filters(): IssueFilters { return this.filtersSubject.value; }
  issues(): Observable<Issue[]> { return this.filters$.pipe(switchMap(filters => this.api.issues(filters))); }
  update(partial: Partial<IssueFilters>): void { this.setFilters({ ...this.filters(), ...partial }); }
  clear(): void { this.setFilters({}); }
}
