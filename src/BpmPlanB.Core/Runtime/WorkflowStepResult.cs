using System;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Represents the result of a workflow step execution.
/// </summary>
public sealed class WorkflowStepResult
{
    private WorkflowStepResult(
        string? nextStepId,
        WorkflowInstanceStatus? statusOverride,
        DateTimeOffset? resumeAt,
        string? waitingSignal,
        Exception? error)
    {
        NextStepId = nextStepId;
        StatusOverride = statusOverride;
        ResumeAt = resumeAt;
        WaitingSignal = waitingSignal;
        Error = error;
    }

    /// <summary>
    /// Gets the identifier of the next step to execute.
    /// When null the engine relies on the current step definition for the next step resolution.
    /// </summary>
    public string? NextStepId { get; }

    /// <summary>
    /// Gets the status override returned by the handler.
    /// </summary>
    public WorkflowInstanceStatus? StatusOverride { get; }

    /// <summary>
    /// Gets the date when the workflow instance should resume execution.
    /// </summary>
    public DateTimeOffset? ResumeAt { get; }

    /// <summary>
    /// Gets the signal key awaited by the workflow instance.
    /// </summary>
    public string? WaitingSignal { get; }

    /// <summary>
    /// Gets the exception returned by the step handler when execution fails.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Creates a result instructing the engine to continue to the specified step.
    /// </summary>
    public static WorkflowStepResult Continue(string? nextStepId = null) => new(nextStepId, null, null, null, null);

    /// <summary>
    /// Creates a result instructing the engine to mark the workflow as completed.
    /// </summary>
    public static WorkflowStepResult Complete() => new(null, WorkflowInstanceStatus.Completed, null, null, null);

    /// <summary>
    /// Creates a result instructing the engine to suspend the workflow until the provided date.
    /// </summary>
    public static WorkflowStepResult SuspendUntil(DateTimeOffset resumeAt) => new(null, WorkflowInstanceStatus.WaitingForSchedule, resumeAt, null, null);

    /// <summary>
    /// Creates a result instructing the engine to suspend the workflow until the specified signal is received.
    /// </summary>
    public static WorkflowStepResult WaitForSignal(string signalKey)
    {
        if (string.IsNullOrWhiteSpace(signalKey))
        {
            throw new ArgumentException("The signal key cannot be null or whitespace.", nameof(signalKey));
        }

        return new WorkflowStepResult(null, WorkflowInstanceStatus.WaitingForSignal, null, signalKey, null);
    }

    /// <summary>
    /// Creates a result instructing the engine to mark the workflow as faulted.
    /// </summary>
    public static WorkflowStepResult Fault(Exception error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        return new WorkflowStepResult(null, WorkflowInstanceStatus.Faulted, null, null, error);
    }
}
