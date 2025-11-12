namespace BpmPlanB.Execution;

public sealed record WorkflowStepResult
{
    public static WorkflowStepResult Continue(string? nextStepId = null) => new() { NextStepId = nextStepId, Status = WorkflowStepStatus.Continue };

    public static WorkflowStepResult Suspend() => new() { Status = WorkflowStepStatus.Suspend };

    public static WorkflowStepResult Complete() => new() { Status = WorkflowStepStatus.Complete };

    public static WorkflowStepResult Fault(Exception error) => new() { Status = WorkflowStepStatus.Faulted, Error = error };

    public WorkflowStepStatus Status { get; init; }

    public string? NextStepId { get; init; }

    public Exception? Error { get; init; }
}

public enum WorkflowStepStatus
{
    Continue,
    Suspend,
    Complete,
    Faulted
}
