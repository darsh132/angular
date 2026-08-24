# Sprint Metrics

## Metric definitions

Sprint progress is based on story points, not issue count.

- **Committed points** = sum of non-negative StoryPoints for issues assigned to the sprint.
- **Completed points** = sum of StoryPoints for issues whose status is `Done`.
- **Remaining points** = `max(0, Committed - Completed)`.
- **Completion %** = `Completed / Committed * 100`, rounded to the nearest whole number. A sprint with zero committed points reports 0%.

Example:

```text
Committed: 24 SP
Completed: 13 SP
Remaining: 11 SP
Progress:   54%
```

Issue count is still useful as a secondary metric, but it is not the primary delivery-progress measure because issues can have materially different estimates.

## Source of truth
The issue `StoryPoints` and `Status` persisted by the API are authoritative. The Angular application derives the displayed metrics from those API values and does not persist calculated progress.

## Future metrics
- Sprint velocity across completed sprints.
- Burndown time series.
- Scope-change tracking.
- Carry-over story points.
- Cycle time and lead time.
- Cumulative flow.
