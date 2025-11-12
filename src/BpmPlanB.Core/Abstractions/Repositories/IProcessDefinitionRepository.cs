using System.Threading;
using System.Threading.Tasks;

using BpmPlanB.Core.Definitions;

namespace BpmPlanB.Core.Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for process definitions.
/// </summary>
public interface IProcessDefinitionRepository
{
    /// <summary>
    /// Retrieves a process definition.
    /// </summary>
    /// <param name="processId">Identifier of the process.</param>
    /// <param name="version">Version of the process. When null, the latest version must be returned.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<ProcessDefinition?> GetAsync(string processId, int? version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a process definition.
    /// </summary>
    /// <param name="definition">Process definition instance.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task SaveAsync(ProcessDefinition definition, CancellationToken cancellationToken = default);
}
