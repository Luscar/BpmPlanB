namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Represents the status of a workflow instance.
/// </summary>
public enum WorkflowInstanceStatus
{
    Created,
    Running,
    WaitingForSchedule,
    WaitingForSignal,
    Completed,
    Faulted
}
