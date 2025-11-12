using System;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Step that executes another workflow as part of the current process.
/// </summary>
public sealed record SubProcessStepDefinition : LinearWorkflowStepDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubProcessStepDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the step inside its process.</param>
    /// <param name="name">Human readable name.</param>
    /// <param name="subProcessId">Identifier of the child process.</param>
    /// <param name="subProcessVersion">Optional version of the child process.</param>
    /// <param name="nextStepId">Identifier of the next step in the parent process.</param>
    public SubProcessStepDefinition(
        string id,
        string name,
        string subProcessId,
        int? subProcessVersion,
        string? nextStepId)
        : base(id, name, nextStepId)
    {
        SubProcessId = string.IsNullOrWhiteSpace(subProcessId)
            ? throw new ArgumentException("The subprocess identifier cannot be null or whitespace.", nameof(subProcessId))
            : subProcessId;
        SubProcessVersion = subProcessVersion;
    }

    /// <summary>
    /// Gets the identifier of the child process definition.
    /// </summary>
    public string SubProcessId { get; }

    /// <summary>
    /// Gets the optional version of the child process definition.
    /// </summary>
    public int? SubProcessVersion { get; }

    /// <inheritdoc />
    public override WorkflowStepType StepType => WorkflowStepType.SubProcess;
}
