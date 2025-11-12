using BpmPlanB.Model;

namespace BpmPlanB.Abstractions.Repositories;

public interface IProcessDefinitionRepository
{
    Task<ProcessDefinition?> GetByIdAsync(string processId, int? version, CancellationToken cancellationToken);

    Task SaveAsync(ProcessDefinition definition, CancellationToken cancellationToken);
}
