using BpmPlanB.Abstractions.Services;
using BpmPlanB.Execution;
using BpmPlanB.Model;

namespace BpmPlanB.Handlers;

public sealed class AffairStepHandler : IWorkflowStepHandler
{
    private readonly IAffairServiceInvoker _serviceInvoker;

    public AffairStepHandler(IAffairServiceInvoker serviceInvoker)
    {
        _serviceInvoker = serviceInvoker;
    }

    public StepType StepType => StepType.Affair;

    public async Task<WorkflowStepResult> ExecuteAsync(StepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        if (step is not AffairStepDefinition affairStep)
        {
            throw new ArgumentException($"Invalid step type {step.Type} for {nameof(AffairStepHandler)}.", nameof(step));
        }

        await _serviceInvoker.InvokeAsync(affairStep, context, cancellationToken).ConfigureAwait(false);

        return WorkflowStepResult.Continue(affairStep.NextStepId);
    }
}
