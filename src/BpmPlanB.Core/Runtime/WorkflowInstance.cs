using System;
using System.Collections.Generic;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Represents a workflow instance executing a process definition.
/// </summary>
public sealed class WorkflowInstance
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowInstance"/> class.
    /// </summary>
    public WorkflowInstance(
        Guid id,
        string processDefinitionId,
        int processVersion,
        string currentStepId,
        WorkflowInstanceStatus status,
        IDictionary<string, object?>? data = null)
    {
        if (string.IsNullOrWhiteSpace(processDefinitionId))
        {
            throw new ArgumentException("The process definition identifier cannot be null or whitespace.", nameof(processDefinitionId));
        }

        if (string.IsNullOrWhiteSpace(currentStepId))
        {
            throw new ArgumentException("The current step identifier cannot be null or whitespace.", nameof(currentStepId));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ProcessDefinitionId = processDefinitionId;
        ProcessVersion = processVersion;
        CurrentStepId = currentStepId;
        Status = status;
        Data = data != null
            ? new Dictionary<string, object?>(data, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    /// <summary>
    /// Gets the unique identifier of the workflow instance.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the identifier of the process definition executed by the instance.
    /// </summary>
    public string ProcessDefinitionId { get; }

    /// <summary>
    /// Gets the version of the process definition executed by the instance.
    /// </summary>
    public int ProcessVersion { get; }

    /// <summary>
    /// Gets or sets the identifier of the step currently being executed.
    /// </summary>
    public string CurrentStepId { get; set; }

    /// <summary>
    /// Gets or sets the status of the workflow instance.
    /// </summary>
    public WorkflowInstanceStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the UTC date where the workflow must resume when waiting for a schedule.
    /// </summary>
    public DateTimeOffset? ResumeAt { get; set; }

    /// <summary>
    /// Gets or sets the signal awaited by the workflow instance.
    /// </summary>
    public string? WaitingSignal { get; set; }

    /// <summary>
    /// Gets the payload carried by the workflow instance.
    /// </summary>
    public IDictionary<string, object?> Data { get; }

    /// <summary>
    /// Gets the creation timestamp of the workflow instance.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets or sets the timestamp of the last update.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Sets the workflow instance in a waiting state until the specified date.
    /// </summary>
    /// <param name="resumeAt">Date where the workflow must resume.</param>
    public void Schedule(DateTimeOffset resumeAt)
    {
        ResumeAt = resumeAt;
        WaitingSignal = null;
        Status = WorkflowInstanceStatus.WaitingForSchedule;
    }

    /// <summary>
    /// Sets the workflow instance in a waiting state until the specified signal is received.
    /// </summary>
    /// <param name="signalKey">Key of the signal awaited.</param>
    public void WaitForSignal(string signalKey)
    {
        if (string.IsNullOrWhiteSpace(signalKey))
        {
            throw new ArgumentException("The signal key cannot be null or whitespace.", nameof(signalKey));
        }

        ResumeAt = null;
        WaitingSignal = signalKey;
        Status = WorkflowInstanceStatus.WaitingForSignal;
    }

    /// <summary>
    /// Marks the workflow instance as updated.
    /// </summary>
    public void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
