using BpmPlanB.Abstractions.Services;
using BpmPlanB.Execution;
using BpmPlanB.Model;

namespace BpmPlanB.Handlers;

public sealed class SubProcessStepHandler : IWorkflowStepHandler
{
    private readonly ISubProcessLauncher _subProcessLauncher;

    public SubProcessStepHandler(ISubProcessLauncher subProcessLauncher)
    {
        _subProcessLauncher = subProcessLauncher;
    }

    public StepType StepType => StepType.SubProcess;

    public async Task<WorkflowStepResult> ExecuteAsync(StepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        if (step is not SubProcessStepDefinition subProcessStep)
        {
            throw new ArgumentException($"Invalid step type {step.Type} for {nameof(SubProcessStepHandler)}.", nameof(step));
        }

        await _subProcessLauncher.LaunchAsync(subProcessStep, context, cancellationToken).ConfigureAwait(false);

        return WorkflowStepResult.Continue(subProcessStep.NextStepId);
    }
}
