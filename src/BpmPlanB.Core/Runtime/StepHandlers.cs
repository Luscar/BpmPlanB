using BpmPlanB.Core.Definitions;

namespace BpmPlanB.Core.Runtime;

/// <summary>
/// Handles the execution of business steps.
/// </summary>
public interface IBusinessStepHandler : IWorkflowStepHandler<BusinessStepDefinition> { }

/// <summary>
/// Handles the execution of interactive steps.
/// </summary>
public interface IInteractiveStepHandler : IWorkflowStepHandler<InteractiveStepDefinition> { }

/// <summary>
/// Handles the execution of decision steps.
/// </summary>
public interface IDecisionStepHandler : IWorkflowStepHandler<DecisionStepDefinition> { }

/// <summary>
/// Handles the execution of scheduled steps.
/// </summary>
public interface IScheduledStepHandler : IWorkflowStepHandler<ScheduledStepDefinition> { }

/// <summary>
/// Handles the execution of signal steps.
/// </summary>
public interface ISignalStepHandler : IWorkflowStepHandler<SignalStepDefinition> { }

/// <summary>
/// Handles the execution of subprocess steps.
/// </summary>
public interface ISubProcessStepHandler : IWorkflowStepHandler<SubProcessStepDefinition> { }
