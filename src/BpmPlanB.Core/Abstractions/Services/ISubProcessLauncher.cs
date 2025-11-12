using BpmPlanB.Model;
using BpmPlanB.Execution;

namespace BpmPlanB.Abstractions.Services;

public interface ISubProcessLauncher
{
    Task LaunchAsync(SubProcessStepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken);
}
