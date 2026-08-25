namespace JiraClone.Api.Models;

public enum IssueStatus { Backlog, Todo, InProgress, InReview, Done }
public enum IssuePriority { Lowest, Low, Medium, High, Highest }
public enum IssueType { Story, Task, Bug, Epic }
public enum IssueActivityType { Created, StatusChanged, CommentAdded, AssigneeChanged, PriorityChanged, Updated }
public enum SprintStatus { Planned, Active, Completed }
public enum ProjectRole { Viewer, Member, Manager }

public sealed class User { public int Id { get; set; } public string Name { get; set; } = ""; public string Email { get; set; } = ""; public string Avatar { get; set; } = ""; public string PasswordHash { get; set; } = ""; public string Role { get; set; } = "User"; public ICollection<ProjectMember> ProjectMemberships { get; set; } = []; }
public sealed class Project { public int Id { get; set; } public string Key { get; set; } = ""; public string Name { get; set; } = ""; public string Description { get; set; } = ""; public ICollection<Issue> Issues { get; set; } = []; public ICollection<Sprint> Sprints { get; set; } = []; public ICollection<ProjectMember> Members { get; set; } = []; }
public sealed class ProjectMember { public int ProjectId { get; set; } public Project Project { get; set; } = null!; public int UserId { get; set; } public User User { get; set; } = null!; public ProjectRole Role { get; set; } }
public sealed class Sprint { public int Id { get; set; } public string Name { get; set; } = ""; public string? Goal { get; set; } public SprintStatus Status { get; set; } public int ProjectId { get; set; } public Project Project { get; set; } = null!; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public ICollection<Issue> Issues { get; set; } = []; }
public sealed class Issue { public int Id { get; set; } public int Number { get; set; } public string Title { get; set; } = ""; public string Description { get; set; } = ""; public IssueStatus Status { get; set; } public IssuePriority Priority { get; set; } public IssueType Type { get; set; } public int StoryPoints { get; set; } public int ProjectId { get; set; } public Project Project { get; set; } = null!; public int? AssigneeId { get; set; } public User? Assignee { get; set; } public int? SprintId { get; set; } public Sprint? Sprint { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public ICollection<IssueComment> Comments { get; set; } = []; public ICollection<IssueActivity> Activities { get; set; } = []; }
public sealed class IssueComment { public int Id { get; set; } public int IssueId { get; set; } public Issue Issue { get; set; } = null!; public int AuthorId { get; set; } public User Author { get; set; } = null!; public string Body { get; set; } = ""; public DateTime CreatedAt { get; set; } }
public sealed class IssueActivity { public int Id { get; set; } public int IssueId { get; set; } public Issue Issue { get; set; } = null!; public int ActorId { get; set; } public User Actor { get; set; } = null!; public IssueActivityType Type { get; set; } public string? OldValue { get; set; } public string? NewValue { get; set; } public DateTime CreatedAt { get; set; } }
