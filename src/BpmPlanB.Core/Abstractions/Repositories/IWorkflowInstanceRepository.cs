using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using BpmPlanB.Core.Runtime;

namespace BpmPlanB.Core.Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for workflow instances.
/// </summary>
public interface IWorkflowInstanceRepository
{
    /// <summary>
    /// Creates a new workflow instance in the persistence layer.
    /// </summary>
    /// <param name="instance">Workflow instance to persist.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task CreateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workflow instance.
    /// </summary>
    /// <param name="instance">Workflow instance to update.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a workflow instance.
    /// </summary>
    /// <param name="instanceId">Unique identifier of the workflow instance.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<WorkflowInstance?> GetAsync(Guid instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists workflow instances waiting to resume execution at a given date.
    /// </summary>
    /// <param name="scheduledBefore">Date used to filter instances scheduled before or equal to that time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<IReadOnlyCollection<WorkflowInstance>> GetScheduledAsync(DateTimeOffset scheduledBefore, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists workflow instances waiting for a specific signal.
    /// </summary>
    /// <param name="signalKey">Key of the signal awaited by the instances.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<IReadOnlyCollection<WorkflowInstance>> GetWaitingForSignalAsync(string signalKey, CancellationToken cancellationToken = default);
}
