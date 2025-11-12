using BpmPlanB.Model;
using BpmPlanB.Execution;

namespace BpmPlanB.Abstractions.Services;

public interface ISignalAwaiter
{
    Task WaitForSignalAsync(SignalStepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken);
}
