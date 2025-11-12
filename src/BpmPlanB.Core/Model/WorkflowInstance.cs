namespace BpmPlanB.Model;

public sealed record WorkflowInstance
{
    public required string Id { get; init; }

    public required string ProcessId { get; init; }

    public required int Version { get; init; }

    public string CurrentStepId { get; init; } = string.Empty;

    public WorkflowStatus Status { get; init; } = WorkflowStatus.Running;

    public IDictionary<string, object?> Data { get; init; } = new Dictionary<string, object?>();
}

public enum WorkflowStatus
{
    Running,
    Suspended,
    Completed,
    Faulted
}
