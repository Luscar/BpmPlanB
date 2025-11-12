using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BpmPlanB.Core.Abstractions.Repositories;
using BpmPlanB.Core.Definitions;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Coordinates the execution of workflow instances.
/// </summary>
public sealed class WorkflowEngine
{
    private readonly IProcessDefinitionRepository _processRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly WorkflowStepHandlerRegistry _handlerRegistry;
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowEngine"/> class.
    /// </summary>
    public WorkflowEngine(
        IProcessDefinitionRepository processRepository,
        IWorkflowInstanceRepository instanceRepository,
        WorkflowStepHandlerRegistry handlerRegistry,
        IServiceProvider? serviceProvider = null)
    {
        _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
        _instanceRepository = instanceRepository ?? throw new ArgumentNullException(nameof(instanceRepository));
        _handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Starts a new workflow instance based on the provided process definition identifier.
    /// </summary>
    public async Task<WorkflowInstance> StartAsync(string processId, int? version = null, IDictionary<string, object?>? data = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(processId))
        {
            throw new ArgumentException("The process identifier cannot be null or whitespace.", nameof(processId));
        }

        var definition = await _processRepository.GetAsync(processId, version, cancellationToken).ConfigureAwait(false)
                         ?? throw new InvalidOperationException($"The process definition '{processId}' (version '{version?.ToString() ?? "latest"}') was not found.");
        var instance = new WorkflowInstance(Guid.NewGuid(), definition.Id, definition.Version, definition.StartStepId, WorkflowInstanceStatus.Running, data);
        await _instanceRepository.CreateAsync(instance, cancellationToken).ConfigureAwait(false);
        return await ExecuteAsync(definition, instance, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resumes a workflow instance that was previously waiting for a schedule or signal.
    /// </summary>
    public async Task<WorkflowInstance> ResumeAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepository.GetAsync(instanceId, cancellationToken).ConfigureAwait(false)
                       ?? throw new InvalidOperationException($"The workflow instance '{instanceId}' was not found.");

        if (instance.Status == WorkflowInstanceStatus.Completed || instance.Status == WorkflowInstanceStatus.Faulted)
        {
            return instance;
        }

        var definition = await _processRepository.GetAsync(instance.ProcessDefinitionId, instance.ProcessVersion, cancellationToken).ConfigureAwait(false)
                         ?? throw new InvalidOperationException($"The process definition '{instance.ProcessDefinitionId}' version '{instance.ProcessVersion}' was not found.");

        instance.Status = WorkflowInstanceStatus.Running;
        instance.ResumeAt = null;
        instance.WaitingSignal = null;
        instance.Touch();
        return await ExecuteAsync(definition, instance, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a signal and resumes the matching workflow instances.
    /// </summary>
    /// <returns>The number of instances resumed.</returns>
    public async Task<int> PublishSignalAsync(string signalKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(signalKey))
        {
            throw new ArgumentException("The signal key cannot be null or whitespace.", nameof(signalKey));
        }

        var waitingInstances = await _instanceRepository.GetWaitingForSignalAsync(signalKey, cancellationToken).ConfigureAwait(false);
        var resumedCount = 0;

        foreach (var instance in waitingInstances)
        {
            var definition = await _processRepository.GetAsync(instance.ProcessDefinitionId, instance.ProcessVersion, cancellationToken).ConfigureAwait(false);
            if (definition is null)
            {
                continue;
            }

            instance.Status = WorkflowInstanceStatus.Running;
            instance.WaitingSignal = null;
            instance.Touch();
            await ExecuteAsync(definition, instance, cancellationToken).ConfigureAwait(false);
            resumedCount++;
        }

        return resumedCount;
    }

    private async Task<WorkflowInstance> ExecuteAsync(ProcessDefinition definition, WorkflowInstance instance, CancellationToken cancellationToken)
    {
        while (instance.Status == WorkflowInstanceStatus.Running)
        {
            var step = definition.GetStep(instance.CurrentStepId);
            var context = new StepExecutionContext(definition, instance, _serviceProvider, cancellationToken);
            WorkflowStepResult result;

            try
            {
                result = await _handlerRegistry.ExecuteAsync(step, context, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                instance.Status = WorkflowInstanceStatus.Faulted;
                instance.Touch();
                await _instanceRepository.UpdateAsync(instance, cancellationToken).ConfigureAwait(false);
                throw new WorkflowExecutionException($"An exception occurred while executing step '{step.Id}'.", step.Id, ex);
            }

            if (result is null)
            {
                throw new InvalidOperationException($"The handler for step '{step.Id}' returned a null result.");
            }

            if (result.Error is not null)
            {
                instance.Status = WorkflowInstanceStatus.Faulted;
                instance.Touch();
                await _instanceRepository.UpdateAsync(instance, cancellationToken).ConfigureAwait(false);
                throw new WorkflowExecutionException($"The handler for step '{step.Id}' reported an error.", step.Id, result.Error);
            }

            if (result.StatusOverride == WorkflowInstanceStatus.Completed)
            {
                instance.Status = WorkflowInstanceStatus.Completed;
                instance.ResumeAt = null;
                instance.WaitingSignal = null;
                instance.Touch();
                break;
            }

            if (result.StatusOverride == WorkflowInstanceStatus.Faulted)
            {
                instance.Status = WorkflowInstanceStatus.Faulted;
                instance.Touch();
                await _instanceRepository.UpdateAsync(instance, cancellationToken).ConfigureAwait(false);
                throw new WorkflowExecutionException($"The handler for step '{step.Id}' reported a fault.", step.Id, result.Error ?? new InvalidOperationException("An unspecified error occurred."));
            }

            if (result.StatusOverride == WorkflowInstanceStatus.WaitingForSchedule || result.ResumeAt is not null || step is ScheduledStepDefinition)
            {
                var resumeAt = result.ResumeAt;
                if (resumeAt is null && step is ScheduledStepDefinition scheduled)
                {
                    resumeAt = scheduled.ResumeAt;
                    if (resumeAt is null && scheduled.Delay is not null)
                    {
                        resumeAt = DateTimeOffset.UtcNow.Add(scheduled.Delay.Value);
                    }
                }

                if (resumeAt is null)
                {
                    throw new InvalidOperationException($"The scheduled step '{step.Id}' did not provide a resume date.");
                }

                instance.Schedule(resumeAt.Value);
                instance.Touch();
                break;
            }

            if (result.StatusOverride == WorkflowInstanceStatus.WaitingForSignal || result.WaitingSignal is not null || step is SignalStepDefinition)
            {
                var signalKey = result.WaitingSignal;
                if (string.IsNullOrWhiteSpace(signalKey) && step is SignalStepDefinition signalDefinition)
                {
                    signalKey = signalDefinition.SignalKey;
                }

                if (string.IsNullOrWhiteSpace(signalKey))
                {
                    throw new InvalidOperationException($"The signal step '{step.Id}' did not provide a signal key.");
                }

                instance.WaitForSignal(signalKey);
                instance.Touch();
                break;
            }

            var nextStepId = result.NextStepId;
            if (nextStepId is null)
            {
                switch (step)
                {
                    case DecisionStepDefinition decision:
                        nextStepId = decision.DefaultNextStepId;
                        break;
                    case LinearWorkflowStepDefinition linear:
                        nextStepId = linear.NextStepId;
                        break;
                }
            }

            if (nextStepId is null)
            {
                instance.Status = WorkflowInstanceStatus.Completed;
                instance.ResumeAt = null;
                instance.WaitingSignal = null;
                instance.Touch();
                break;
            }

            if (!definition.Steps.ContainsKey(nextStepId))
            {
                throw new InvalidOperationException($"The next step '{nextStepId}' referenced from step '{step.Id}' does not exist in process '{definition.Id}'.");
            }

            instance.CurrentStepId = nextStepId;
            instance.Touch();
        }

        await _instanceRepository.UpdateAsync(instance, cancellationToken).ConfigureAwait(false);
        return instance;
    }
}
