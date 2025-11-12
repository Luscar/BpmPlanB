using System;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Step that suspends the workflow until an external signal is received.
/// </summary>
public sealed record SignalStepDefinition : LinearWorkflowStepDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignalStepDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the step inside its process.</param>
    /// <param name="name">Human readable name.</param>
    /// <param name="nextStepId">Identifier of the next step.</param>
    /// <param name="signalKey">Key that identifies the awaited signal.</param>
    public SignalStepDefinition(string id, string name, string? nextStepId, string signalKey)
        : base(id, name, nextStepId)
    {
        SignalKey = string.IsNullOrWhiteSpace(signalKey)
            ? throw new ArgumentException("The signal key cannot be null or whitespace.", nameof(signalKey))
            : signalKey;
    }

    /// <summary>
    /// Gets the key that identifies the awaited signal.
    /// </summary>
    public string SignalKey { get; }

    /// <inheritdoc />
    public override WorkflowStepType StepType => WorkflowStepType.Signal;
}
