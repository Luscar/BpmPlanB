using System.Text.Json.Serialization;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Base record for every workflow step definition declared in a process definition.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BusinessStepDefinition), "business")]
[JsonDerivedType(typeof(InteractiveStepDefinition), "interactive")]
[JsonDerivedType(typeof(DecisionStepDefinition), "decision")]
[JsonDerivedType(typeof(ScheduledStepDefinition), "scheduled")]
[JsonDerivedType(typeof(SignalStepDefinition), "signal")]
[JsonDerivedType(typeof(SubProcessStepDefinition), "subProcess")]
public abstract record WorkflowStepDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowStepDefinition"/> class.
    /// </summary>
    /// <param name="id">Unique identifier of the step inside its process definition.</param>
    /// <param name="name">Human readable label for the step.</param>
    protected WorkflowStepDefinition(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("The step identifier cannot be null or whitespace.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The step name cannot be null or whitespace.", nameof(name));
        }

        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the unique identifier of the step within a process definition.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the friendly name of the step.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the step type associated to this definition.
    /// </summary>
    [JsonIgnore]
    public abstract WorkflowStepType StepType { get; }
}

/// <summary>
/// Base record for steps that can only progress toward a single next node.
/// </summary>
public abstract record LinearWorkflowStepDefinition : WorkflowStepDefinition
{
    private protected LinearWorkflowStepDefinition(string id, string name, string? nextStepId)
        : base(id, name)
    {
        NextStepId = nextStepId;
    }

    /// <summary>
    /// Gets the identifier of the next step to execute when the current step completes successfully.
    /// </summary>
    public string? NextStepId { get; }
}
