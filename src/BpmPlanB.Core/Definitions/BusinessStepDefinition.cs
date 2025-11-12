using System;
using System.Collections.Generic;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Step that delegates the execution of business logic to an external service hosted by the client system.
/// </summary>
public sealed record BusinessStepDefinition : LinearWorkflowStepDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessStepDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the step inside its process.</param>
    /// <param name="name">Human readable name.</param>
    /// <param name="serviceIdentifier">Identifier of the service that must be called by the handler.</param>
    /// <param name="nextStepId">Identifier of the next step.</param>
    /// <param name="parameters">Optional parameters passed to the handler.</param>
    public BusinessStepDefinition(
        string id,
        string name,
        string serviceIdentifier,
        string? nextStepId,
        IDictionary<string, string>? parameters = null)
        : base(id, name, nextStepId)
    {
        ServiceIdentifier = string.IsNullOrWhiteSpace(serviceIdentifier)
            ? throw new ArgumentException("The service identifier cannot be null or whitespace.", nameof(serviceIdentifier))
            : serviceIdentifier;

        Parameters = parameters ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the unique identifier or address of the service that the handler must call.
    /// </summary>
    public string ServiceIdentifier { get; }

    /// <summary>
    /// Gets arbitrary parameters that will be forwarded to the handler responsible of the step execution.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <inheritdoc />
    public override WorkflowStepType StepType => WorkflowStepType.Business;
}
