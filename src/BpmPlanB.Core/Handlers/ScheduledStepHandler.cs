using BpmPlanB.Abstractions.Services;
using BpmPlanB.Execution;
using BpmPlanB.Model;

namespace BpmPlanB.Handlers;

public sealed class ScheduledStepHandler : IWorkflowStepHandler
{
    private readonly IWorkflowScheduler _scheduler;

    public ScheduledStepHandler(IWorkflowScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public StepType StepType => StepType.Scheduled;

    public async Task<WorkflowStepResult> ExecuteAsync(StepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        if (step is not ScheduledStepDefinition scheduledStep)
        {
            throw new ArgumentException($"Invalid step type {step.Type} for {nameof(ScheduledStepHandler)}.", nameof(step));
        }

        var resumeAt = DateTimeOffset.UtcNow;
        if (scheduledStep.Delay is { } delay)
        {
            resumeAt = resumeAt.Add(delay);
        }

        await _scheduler.ScheduleAsync(scheduledStep, context, resumeAt, cancellationToken).ConfigureAwait(false);

        return WorkflowStepResult.Suspend();
    }
}
