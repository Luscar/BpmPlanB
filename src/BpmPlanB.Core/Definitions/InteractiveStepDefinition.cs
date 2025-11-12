using System;
using System.Collections.Generic;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Step that creates a user facing task in the client application.
/// </summary>
public sealed record InteractiveStepDefinition : LinearWorkflowStepDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveStepDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the step inside its process.</param>
    /// <param name="name">Human readable name.</param>
    /// <param name="interfaceIdentifier">Identifier of the UI component or API that will host the task.</param>
    /// <param name="role">Role that should receive the task.</param>
    /// <param name="nextStepId">Identifier of the next step.</param>
    /// <param name="metadata">Optional metadata forwarded to the task creation handler.</param>
    public InteractiveStepDefinition(
        string id,
        string name,
        string interfaceIdentifier,
        string role,
        string? nextStepId,
        IDictionary<string, string>? metadata = null)
        : base(id, name, nextStepId)
    {
        InterfaceIdentifier = string.IsNullOrWhiteSpace(interfaceIdentifier)
            ? throw new ArgumentException("The interface identifier cannot be null or whitespace.", nameof(interfaceIdentifier))
            : interfaceIdentifier;
        Role = string.IsNullOrWhiteSpace(role)
            ? throw new ArgumentException("The role cannot be null or whitespace.", nameof(role))
            : role;
        Metadata = metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the identifier of the client system interface that must create the task.
    /// </summary>
    public string InterfaceIdentifier { get; }

    /// <summary>
    /// Gets the role that should receive the task.
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// Gets metadata provided to the handler to assist with task creation.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <inheritdoc />
    public override WorkflowStepType StepType => WorkflowStepType.Interactive;
}
