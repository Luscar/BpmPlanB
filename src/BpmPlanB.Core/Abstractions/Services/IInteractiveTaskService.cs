using BpmPlanB.Model;
using BpmPlanB.Execution;

namespace BpmPlanB.Abstractions.Services;

public interface IInteractiveTaskService
{
    Task<string> CreateTaskAsync(InteractiveStepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken);
}
