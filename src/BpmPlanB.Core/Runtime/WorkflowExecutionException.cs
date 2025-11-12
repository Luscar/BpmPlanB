using System;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Exception thrown when workflow execution fails.
/// </summary>
public sealed class WorkflowExecutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowExecutionException"/> class.
    /// </summary>
    public WorkflowExecutionException(string message, string stepId, Exception innerException)
        : base(message, innerException)
    {
        StepId = stepId;
    }

    /// <summary>
    /// Gets the identifier of the step where the error occurred.
    /// </summary>
    public string StepId { get; }
}
