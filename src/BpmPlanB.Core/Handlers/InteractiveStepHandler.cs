using BpmPlanB.Abstractions.Services;
using BpmPlanB.Execution;
using BpmPlanB.Model;

namespace BpmPlanB.Handlers;

public sealed class InteractiveStepHandler : IWorkflowStepHandler
{
    private readonly IInteractiveTaskService _taskService;

    public InteractiveStepHandler(IInteractiveTaskService taskService)
    {
        _taskService = taskService;
    }

    public StepType StepType => StepType.Interactive;

    public async Task<WorkflowStepResult> ExecuteAsync(StepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        if (step is not InteractiveStepDefinition interactiveStep)
        {
            throw new ArgumentException($"Invalid step type {step.Type} for {nameof(InteractiveStepHandler)}.", nameof(step));
        }

        await _taskService.CreateTaskAsync(interactiveStep, context, cancellationToken).ConfigureAwait(false);

        return WorkflowStepResult.Suspend();
    }
}
