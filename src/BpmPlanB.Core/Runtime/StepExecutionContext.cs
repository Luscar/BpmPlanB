using System;
using System.Collections.Generic;
using System.Threading;
using BpmPlanB.Core.Definitions;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Provides context information to step handlers.
/// </summary>
public sealed class StepExecutionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepExecutionContext"/> class.
    /// </summary>
    public StepExecutionContext(
        ProcessDefinition processDefinition,
        WorkflowInstance instance,
        IServiceProvider? services,
        CancellationToken cancellationToken)
    {
        ProcessDefinition = processDefinition ?? throw new ArgumentNullException(nameof(processDefinition));
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        Services = services;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the process definition associated to the current execution.
    /// </summary>
    public ProcessDefinition ProcessDefinition { get; }

    /// <summary>
    /// Gets the workflow instance being executed.
    /// </summary>
    public WorkflowInstance Instance { get; }

    /// <summary>
    /// Gets the service provider supplied by the host application when available.
    /// </summary>
    public IServiceProvider? Services { get; }

    /// <summary>
    /// Gets the cancellation token associated to the current execution.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the mutable payload stored in the workflow instance.
    /// </summary>
    public IDictionary<string, object?> Data => Instance.Data;
}
