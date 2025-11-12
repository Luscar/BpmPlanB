using BpmPlanB.Abstractions.Services;
using BpmPlanB.Execution;
using BpmPlanB.Model;

namespace BpmPlanB.Handlers;

public sealed class SignalStepHandler : IWorkflowStepHandler
{
    private readonly ISignalAwaiter _signalAwaiter;

    public SignalStepHandler(ISignalAwaiter signalAwaiter)
    {
        _signalAwaiter = signalAwaiter;
    }

    public StepType StepType => StepType.Signal;

    public async Task<WorkflowStepResult> ExecuteAsync(StepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        if (step is not SignalStepDefinition signalStep)
        {
            throw new ArgumentException($"Invalid step type {step.Type} for {nameof(SignalStepHandler)}.", nameof(step));
        }

        await _signalAwaiter.WaitForSignalAsync(signalStep, context, cancellationToken).ConfigureAwait(false);

        return WorkflowStepResult.Suspend();
    }
}
