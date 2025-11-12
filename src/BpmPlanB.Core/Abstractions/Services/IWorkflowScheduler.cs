using BpmPlanB.Model;
using BpmPlanB.Execution;

namespace BpmPlanB.Abstractions.Services;

public interface IWorkflowScheduler
{
    Task ScheduleAsync(ScheduledStepDefinition step, WorkflowExecutionContext context, DateTimeOffset resumeAt, CancellationToken cancellationToken);
}
