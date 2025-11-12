using System;
using System.Collections.Generic;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Step that evaluates a business decision by calling an external query and routing the workflow based on the response.
/// </summary>
public sealed record DecisionStepDefinition : WorkflowStepDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DecisionStepDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the step inside its process.</param>
    /// <param name="name">Human readable name.</param>
    /// <param name="serviceIdentifier">Identifier of the query or service used to evaluate the decision.</param>
    /// <param name="routes">Mapping between an outcome and the identifier of the next step.</param>
    /// <param name="defaultNextStepId">Optional identifier of the step executed when no route matches the outcome.</param>
    public DecisionStepDefinition(
        string id,
        string name,
        string serviceIdentifier,
        IDictionary<string, string> routes,
        string? defaultNextStepId = null)
        : base(id, name)
    {
        ServiceIdentifier = string.IsNullOrWhiteSpace(serviceIdentifier)
            ? throw new ArgumentException("The service identifier cannot be null or whitespace.", nameof(serviceIdentifier))
            : serviceIdentifier;

        if (routes is null || routes.Count == 0)
        {
            throw new ArgumentException("At least one route must be provided for a decision step.", nameof(routes));
        }

        Routes = new Dictionary<string, string>(routes, StringComparer.OrdinalIgnoreCase);
        DefaultNextStepId = defaultNextStepId;
    }

    /// <summary>
    /// Gets the identifier of the external query that evaluates the decision.
    /// </summary>
    public string ServiceIdentifier { get; }

    /// <summary>
    /// Gets the mapping between outcomes and the identifier of the next step to execute.
    /// </summary>
    public IReadOnlyDictionary<string, string> Routes { get; }

    /// <summary>
    /// Gets the identifier of the step executed when the decision outcome does not match any route.
    /// When null, the workflow instance is marked as completed.
    /// </summary>
    public string? DefaultNextStepId { get; }

    /// <inheritdoc />
    public override WorkflowStepType StepType => WorkflowStepType.Decision;
}
