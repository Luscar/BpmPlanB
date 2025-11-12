using BpmPlanB.Model;

namespace BpmPlanB.Abstractions.Repositories;

public interface IWorkflowInstanceRepository
{
    Task<WorkflowInstance?> GetAsync(string instanceId, CancellationToken cancellationToken);

    Task<string> CreateAsync(WorkflowInstance instance, CancellationToken cancellationToken);

    Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken);
}
