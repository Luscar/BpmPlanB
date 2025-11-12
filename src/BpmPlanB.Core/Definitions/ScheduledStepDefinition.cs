using System;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Step that pauses the workflow until a target date or duration elapses.
/// </summary>
public sealed record ScheduledStepDefinition : LinearWorkflowStepDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledStepDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the step inside its process.</param>
    /// <param name="name">Human readable name.</param>
    /// <param name="nextStepId">Identifier of the next step.</param>
    /// <param name="resumeAt">Optional absolute date where the workflow must resume.</param>
    /// <param name="delay">Optional delay to wait before resuming.</param>
    /// <remarks>When both <paramref name="resumeAt"/> and <paramref name="delay"/> are provided, the handler decides which value to honour.</remarks>
    public ScheduledStepDefinition(
        string id,
        string name,
        string? nextStepId,
        DateTimeOffset? resumeAt = null,
        TimeSpan? delay = null)
        : base(id, name, nextStepId)
    {
        ResumeAt = resumeAt;
        Delay = delay;
    }

    /// <summary>
    /// Gets the absolute date where the workflow must resume when available.
    /// </summary>
    public DateTimeOffset? ResumeAt { get; }

    /// <summary>
    /// Gets the optional duration the workflow must wait before resuming.
    /// </summary>
    public TimeSpan? Delay { get; }

    /// <inheritdoc />
    public override WorkflowStepType StepType => WorkflowStepType.Scheduled;
}
