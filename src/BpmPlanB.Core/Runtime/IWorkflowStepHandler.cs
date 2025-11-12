using System.Threading;
using System.Threading.Tasks;
using BpmPlanB.Core.Definitions;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Defines the contract for workflow step handlers.
/// </summary>
/// <typeparam name="TStep">Type of the step handled.</typeparam>
public interface IWorkflowStepHandler<in TStep> where TStep : WorkflowStepDefinition
{
    /// <summary>
    /// Executes a workflow step.
    /// </summary>
    /// <param name="step">Step definition.</param>
    /// <param name="context">Context information.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<WorkflowStepResult> ExecuteAsync(TStep step, StepExecutionContext context, CancellationToken cancellationToken);
}
