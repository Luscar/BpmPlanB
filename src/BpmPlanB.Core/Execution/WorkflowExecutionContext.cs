using BpmPlanB.Model;

namespace BpmPlanB.Execution;

public sealed class WorkflowExecutionContext
{
    private WorkflowInstance _instance;

    public WorkflowExecutionContext(ProcessDefinition definition, WorkflowInstance instance)
    {
        Definition = definition;
        _instance = instance;
    }

    public ProcessDefinition Definition { get; }

    public WorkflowInstance Instance => _instance;

    public IDictionary<string, object?> Data => _instance.Data;

    public CancellationToken CancellationToken { get; set; }

    internal void UpdateInstance(WorkflowInstance instance) => _instance = instance;
}
