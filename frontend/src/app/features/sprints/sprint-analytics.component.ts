import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, inject, signal } from '@angular/core';
import { JiraApiService, SprintAnalytics } from '../../core/jira-api.service';

@Component({ selector: 'app-sprint-analytics', standalone: true, imports: [CommonModule], template: `
<section class="card bg-base-100 shadow-sm border border-base-300">
  <div class="card-body">
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3"><div><h2 class="card-title">Sprint analytics</h2><p class="text-sm text-base-content/60">Server-calculated scope and burndown.</p></div><span *ngIf="loading()" class="loading loading-spinner loading-sm"></span></div>
    <div *ngIf="error()" class="alert alert-error mt-4"><span>{{ error() }}</span></div>
    <ng-container *ngIf="data() as x">
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 mt-5"><div class="stat bg-base-200 rounded-box p-4"><div class="stat-title">Committed</div><div class="stat-value text-2xl">{{ x.committedPoints }}</div><div class="stat-desc">story points</div></div><div class="stat bg-base-200 rounded-box p-4"><div class="stat-title">Completed</div><div class="stat-value text-2xl">{{ x.completedPoints }}</div><div class="stat-desc">story points</div></div><div class="stat bg-base-200 rounded-box p-4"><div class="stat-title">Remaining</div><div class="stat-value text-2xl">{{ x.remainingPoints }}</div><div class="stat-desc">story points</div></div><div class="stat bg-base-200 rounded-box p-4"><div class="stat-title">Issues done</div><div class="stat-value text-2xl">{{ x.completedIssueCount }}/{{ x.issueCount }}</div><div class="stat-desc">completed</div></div></div>
      <div class="mt-6"><div class="flex justify-between text-sm mb-2"><span>Completion</span><strong>{{ completion(x) }}%</strong></div><progress class="progress progress-primary w-full" [value]="completion(x)" max="100"></progress></div>
      <div class="mt-7"><div class="flex justify-between items-center mb-3"><h3 class="font-semibold">Burndown</h3><span class="text-xs text-base-content/50">{{ x.startDate | date:'dd MMM' }} — {{ x.endDate | date:'dd MMM' }}</span></div><div class="overflow-x-auto"><svg class="w-full min-w-[640px] h-64" viewBox="0 0 800 260" role="img" aria-label="Sprint burndown chart"><line x1="50" y1="20" x2="50" y2="220" stroke="currentColor" stroke-opacity=".2"/><line x1="50" y1="220" x2="780" y2="220" stroke="currentColor" stroke-opacity=".2"/><polyline fill="none" stroke="currentColor" stroke-opacity=".3" stroke-width="3" [attr.points]="polyline(x.ideal)"/><polyline fill="none" stroke="currentColor" stroke-width="4" [attr.points]="polyline(x.actual)"/><text x="55" y="245" class="fill-current text-xs opacity-60">Start</text><text x="735" y="245" class="fill-current text-xs opacity-60">Today</text></svg></div><div class="flex gap-5 text-xs text-base-content/60 mt-2"><span>— Actual</span><span class="opacity-60">— Ideal</span></div></div>
    </ng-container>
  </div>
</section>` })
export class SprintAnalyticsComponent implements OnChanges {
  @Input({ required: true }) projectId = 0; @Input({ required: true }) sprintId = 0;
  private readonly api = inject(JiraApiService); readonly data = signal<SprintAnalytics | null>(null); readonly loading = signal(false); readonly error = signal<string | null>(null);
  ngOnChanges(): void { if (!this.projectId || !this.sprintId) return; this.loading.set(true); this.error.set(null); this.api.sprintAnalytics(this.projectId, this.sprintId).subscribe({ next: x => this.data.set(x), error: () => this.error.set('Unable to load sprint analytics.'), complete: () => this.loading.set(false) }); }
  completion(x: SprintAnalytics): number { return x.committedPoints ? Math.round(x.completedPoints * 100 / x.committedPoints) : 0; }
  polyline(points: { date: string; remainingPoints: number }[]): string { if (!points.length) return ''; const max = Math.max(1, ...(this.data()?.ideal ?? []).map(p => p.remainingPoints)); const width = 730, height = 190; return points.map((p, i) => `${50 + (points.length === 1 ? 0 : i * width / (points.length - 1))},${30 + (height - (p.remainingPoints / max) * height)}`).join(' '); }
}
