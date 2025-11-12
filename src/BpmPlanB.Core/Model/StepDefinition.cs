using System.Text.Json.Serialization;

namespace BpmPlanB.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AffairStepDefinition), nameof(StepType.Affair))]
[JsonDerivedType(typeof(InteractiveStepDefinition), nameof(StepType.Interactive))]
[JsonDerivedType(typeof(DecisionStepDefinition), nameof(StepType.Decision))]
[JsonDerivedType(typeof(ScheduledStepDefinition), nameof(StepType.Scheduled))]
[JsonDerivedType(typeof(SignalStepDefinition), nameof(StepType.Signal))]
[JsonDerivedType(typeof(SubProcessStepDefinition), nameof(StepType.SubProcess))]
public abstract record StepDefinition
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonIgnore]
    public abstract StepType Type { get; }

    [JsonPropertyName("next")]
    public string? NextStepId { get; init; }
}

public sealed record AffairStepDefinition : StepDefinition
{
    [JsonPropertyName("serviceKey")]
    public string? ServiceKey { get; init; }

    [JsonPropertyName("payload")]
    public IDictionary<string, object>? Payload { get; init; }

    public override StepType Type => StepType.Affair;
}

public sealed record InteractiveStepDefinition : StepDefinition
{
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("taskTemplate")]
    public string? TaskTemplate { get; init; }

    public override StepType Type => StepType.Interactive;
}

public sealed record DecisionStepDefinition : StepDefinition
{
    [JsonPropertyName("queryKey")]
    public string? QueryKey { get; init; }

    [JsonPropertyName("routes")]
    public IDictionary<string, string> Routes { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public override StepType Type => StepType.Decision;
}

public sealed record ScheduledStepDefinition : StepDefinition
{
    [JsonPropertyName("scheduleKey")]
    public string? ScheduleKey { get; init; }

    [JsonPropertyName("delay")]
    public TimeSpan? Delay { get; init; }

    public override StepType Type => StepType.Scheduled;
}

public sealed record SignalStepDefinition : StepDefinition
{
    [JsonPropertyName("signalKey")]
    public string? SignalKey { get; init; }

    public override StepType Type => StepType.Signal;
}

public sealed record SubProcessStepDefinition : StepDefinition
{
    [JsonPropertyName("processKey")]
    public required string ProcessKey { get; init; }

    public override StepType Type => StepType.SubProcess;
}
