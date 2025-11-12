namespace BpmPlanB.Core;

/// <summary>
/// Identifies the different kinds of workflow steps supported by the engine.
/// </summary>
public enum WorkflowStepType
{
    Business,
    Interactive,
    Decision,
    Scheduled,
    Signal,
    SubProcess
}
