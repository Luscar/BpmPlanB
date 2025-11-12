using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BpmPlanB.Core.Definitions;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Stores step handlers and exposes a unified way to execute them.
/// </summary>
public sealed class WorkflowStepHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, Func<WorkflowStepDefinition, StepExecutionContext, CancellationToken, Task<WorkflowStepResult>>> _handlers = new();

    /// <summary>
    /// Registers a handler for the specified step type.
    /// </summary>
    public void Register<TStep>(IWorkflowStepHandler<TStep> handler) where TStep : WorkflowStepDefinition
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        _handlers[typeof(TStep)] = (definition, context, cancellationToken) => handler.ExecuteAsync((TStep)definition, context, cancellationToken);
    }

    /// <summary>
    /// Executes the handler associated to the supplied step definition.
    /// </summary>
    public Task<WorkflowStepResult> ExecuteAsync(WorkflowStepDefinition step, StepExecutionContext context, CancellationToken cancellationToken)
    {
        if (step is null)
        {
            throw new ArgumentNullException(nameof(step));
        }

        if (!_handlers.TryGetValue(step.GetType(), out var executor))
        {
            throw new InvalidOperationException($"No handler has been registered for step type '{step.GetType().Name}'.");
        }

        return executor(step, context, cancellationToken);
    }
}
