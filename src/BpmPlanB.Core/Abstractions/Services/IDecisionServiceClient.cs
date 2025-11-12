using BpmPlanB.Model;
using BpmPlanB.Execution;

namespace BpmPlanB.Abstractions.Services;

public interface IDecisionServiceClient
{
    Task<string?> EvaluateAsync(DecisionStepDefinition step, WorkflowExecutionContext context, CancellationToken cancellationToken);
}
