using BpmPlanB.Abstractions.Services;
using BpmPlanB.Execution;
using BpmPlanB.Model;

namespace BpmPlanB.Handlers;

public sealed class DecisionStepHandler : IWorkflowStepHandler
{
    private readonly IDecisionServiceClient _decisionClient;

    public DecisionStepHandler(IDecisionServiceClient decisionClient)
    {
        _decisionClient = decisionClient;
    }

    public StepType StepType => StepType.Decision;

    public async Task<WorkflowStepResult> ExecuteAsync(StepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken)
    {
        if (step is not DecisionStepDefinition decisionStep)
        {
            throw new ArgumentException($"Invalid step type {step.Type} for {nameof(DecisionStepHandler)}.", nameof(step));
        }

        var outcome = await _decisionClient.EvaluateAsync(decisionStep, context, cancellationToken).ConfigureAwait(false);
        if (outcome is null)
        {
            throw new InvalidOperationException($"Decision step '{decisionStep.Id}' returned no outcome.");
        }

        if (!decisionStep.Routes.TryGetValue(outcome, out var next))
        {
            throw new InvalidOperationException($"Decision step '{decisionStep.Id}' has no route for outcome '{outcome}'.");
        }

        return WorkflowStepResult.Continue(next);
    }
}
