using BpmPlanB.Execution;
using BpmPlanB.Model;

namespace BpmPlanB.Handlers;

public interface IWorkflowStepHandler
{
    StepType StepType { get; }

    Task<WorkflowStepResult> ExecuteAsync(StepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken);
}
