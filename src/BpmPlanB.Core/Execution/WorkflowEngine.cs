using BpmPlanB.Abstractions.Repositories;
using BpmPlanB.Handlers;
using BpmPlanB.Model;

namespace BpmPlanB.Execution;

public sealed class WorkflowEngine
{
    private readonly IProcessDefinitionRepository _processRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IReadOnlyDictionary<StepType, IWorkflowStepHandler> _handlers;

    public WorkflowEngine(
        IProcessDefinitionRepository processRepository,
        IWorkflowInstanceRepository instanceRepository,
        IEnumerable<IWorkflowStepHandler> handlers)
    {
        _processRepository = processRepository;
        _instanceRepository = instanceRepository;
        _handlers = handlers.ToDictionary(h => h.StepType);
    }

    public async Task<WorkflowInstance> StartAsync(
        string processId,
        int? version,
        IDictionary<string, object?>? data,
        CancellationToken cancellationToken)
    {
        var definition = await _processRepository.GetByIdAsync(processId, version, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Process definition '{processId}' not found.");

        if (!definition.TryGetStep(definition.StartStepId, out _))
        {
            throw new InvalidOperationException($"Start step '{definition.StartStepId}' not found in process '{definition.Id}'.");
        }

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid().ToString("N"),
            ProcessId = definition.Id,
            Version = definition.Version,
            CurrentStepId = definition.StartStepId,
            Status = WorkflowStatus.Running,
            Data = data is null ? new Dictionary<string, object?>() : new Dictionary<string, object?>(data)
        };

        await _instanceRepository.CreateAsync(instance, cancellationToken).ConfigureAwait(false);

        return await ExecuteAsync(definition, instance, cancellationToken).ConfigureAwait(false);
    }

    public async Task<WorkflowInstance> ResumeAsync(string instanceId, CancellationToken cancellationToken)
    {
        var instance = await _instanceRepository.GetAsync(instanceId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Workflow instance '{instanceId}' not found.");

        var definition = await _processRepository.GetByIdAsync(instance.ProcessId, instance.Version, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Process definition '{instance.ProcessId}' version '{instance.Version}' not found.");

        return await ExecuteAsync(definition, instance, cancellationToken).ConfigureAwait(false);
    }

    private async Task<WorkflowInstance> ExecuteAsync(ProcessDefinition definition, WorkflowInstance instance, CancellationToken cancellationToken)
    {
        var current = instance;
        var context = new WorkflowExecutionContext(definition, current)
        {
            CancellationToken = cancellationToken
        };

        while (current.Status == WorkflowStatus.Running)
        {
            if (!definition.TryGetStep(current.CurrentStepId, out var step))
            {
                throw new InvalidOperationException($"Step '{current.CurrentStepId}' not found in process '{definition.Id}'.");
            }

            if (!_handlers.TryGetValue(step.Type, out var handler))
            {
                throw new InvalidOperationException($"No handler registered for step type '{step.Type}'.");
            }

            var result = await handler.ExecuteAsync(step, context, cancellationToken).ConfigureAwait(false);

            switch (result.Status)
            {
                case WorkflowStepStatus.Continue:
                    var nextId = result.NextStepId ?? step.NextStepId;
                    if (string.IsNullOrWhiteSpace(nextId))
                    {
                        current = current with { Status = WorkflowStatus.Completed };
                        await PersistAsync(current, cancellationToken).ConfigureAwait(false);
                        context.UpdateInstance(current);
                        return current;
                    }

                    current = current with
                    {
                        CurrentStepId = nextId,
                        Status = WorkflowStatus.Running
                    };
                    await PersistAsync(current, cancellationToken).ConfigureAwait(false);
                    context.UpdateInstance(current);
                    continue;

                case WorkflowStepStatus.Suspend:
                    current = current with { Status = WorkflowStatus.Suspended };
                    await PersistAsync(current, cancellationToken).ConfigureAwait(false);
                    context.UpdateInstance(current);
                    return current;

                case WorkflowStepStatus.Complete:
                    current = current with { Status = WorkflowStatus.Completed };
                    await PersistAsync(current, cancellationToken).ConfigureAwait(false);
                    context.UpdateInstance(current);
                    return current;

                case WorkflowStepStatus.Faulted:
                    current = current with { Status = WorkflowStatus.Faulted };
                    await PersistAsync(current, cancellationToken).ConfigureAwait(false);
                    context.UpdateInstance(current);
                    if (result.Error is not null)
                    {
                        throw result.Error;
                    }

                    return current;

                default:
                    throw new InvalidOperationException($"Unsupported workflow step status '{result.Status}'.");
            }
        }

        return current;
    }

    private Task PersistAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        return _instanceRepository.UpdateAsync(instance, cancellationToken);
    }
}
