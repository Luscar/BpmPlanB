using System;
using System.Collections.Generic;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Represents a process definition described in a JSON document.
/// </summary>
public sealed class ProcessDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the process.</param>
    /// <param name="name">Human readable name of the process.</param>
    /// <param name="version">Version of the process definition.</param>
    /// <param name="startStepId">Identifier of the first step to execute.</param>
    /// <param name="steps">Steps that compose the process.</param>
    public ProcessDefinition(
        string id,
        string name,
        int version,
        string startStepId,
        IDictionary<string, WorkflowStepDefinition> steps)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("The process identifier cannot be null or whitespace.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The process name cannot be null or whitespace.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(startStepId))
        {
            throw new ArgumentException("The start step identifier cannot be null or whitespace.", nameof(startStepId));
        }

        if (steps is null || steps.Count == 0)
        {
            throw new ArgumentException("A process definition must contain at least one step.", nameof(steps));
        }

        if (!steps.ContainsKey(startStepId))
        {
            throw new ArgumentException("The start step identifier must match one of the declared steps.", nameof(startStepId));
        }

        Id = id;
        Name = name;
        Version = version;
        StartStepId = startStepId;
        Steps = new Dictionary<string, WorkflowStepDefinition>(steps, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the unique identifier of the process definition.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the name of the process definition.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the version of the process definition.
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Gets the identifier of the step executed at the beginning of the process.
    /// </summary>
    public string StartStepId { get; }

    /// <summary>
    /// Gets the collection of steps keyed by their identifier.
    /// </summary>
    public IReadOnlyDictionary<string, WorkflowStepDefinition> Steps { get; }

    /// <summary>
    /// Retrieves a step definition by its identifier.
    /// </summary>
    /// <param name="stepId">Identifier of the step to retrieve.</param>
    /// <returns>The matching <see cref="WorkflowStepDefinition"/>.</returns>
    public WorkflowStepDefinition GetStep(string stepId)
    {
        if (!Steps.TryGetValue(stepId, out var step))
        {
            throw new KeyNotFoundException($"The step '{stepId}' does not exist in process '{Id}'.");
        }

        return step;
    }

    /// <summary>
    /// Validates that every declared step has the required routing information according to its type.
    /// </summary>
    public void Validate()
    {
        foreach (var step in Steps.Values)
        {
            switch (step)
            {
                case LinearWorkflowStepDefinition linear when linear.NextStepId is not null && !Steps.ContainsKey(linear.NextStepId):
                    throw new InvalidOperationException($"The step '{linear.Id}' references an unknown next step '{linear.NextStepId}'.");
                case DecisionStepDefinition decision:
                    foreach (var route in decision.Routes.Values)
                    {
                        if (!Steps.ContainsKey(route))
                        {
                            throw new InvalidOperationException($"The decision step '{decision.Id}' references an unknown route target '{route}'.");
                        }
                    }

                    if (decision.DefaultNextStepId is not null && !Steps.ContainsKey(decision.DefaultNextStepId))
                    {
                        throw new InvalidOperationException($"The decision step '{decision.Id}' references an unknown default target '{decision.DefaultNextStepId}'.");
                    }

                    break;
            }
        }
    }
}
