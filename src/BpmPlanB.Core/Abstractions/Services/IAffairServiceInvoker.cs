using BpmPlanB.Model;
using BpmPlanB.Execution;

namespace BpmPlanB.Abstractions.Services;

public interface IAffairServiceInvoker
{
    Task InvokeAsync(AffairStepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken);
}
