using BpmPlanB.Model;

namespace BpmPlanB.Abstractions.Repositories;

public interface IWorkflowTaskRepository
{
    Task<string> CreateTaskAsync(string instanceId, InteractiveStepDefinition step, CancellationToken cancellationToken);

    Task CompleteTaskAsync(string taskId, CancellationToken cancellationToken);
}
