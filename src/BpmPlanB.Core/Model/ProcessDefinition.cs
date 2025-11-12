using System.Text.Json.Serialization;

namespace BpmPlanB.Model;

public sealed record ProcessDefinition
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    public int Version { get; init; }

    [JsonPropertyName("startStep")]
    public required string StartStepId { get; init; }

    [JsonPropertyName("steps")]
    public IReadOnlyDictionary<string, StepDefinition> Steps { get; init; } =
        new Dictionary<string, StepDefinition>(StringComparer.OrdinalIgnoreCase);

    public bool TryGetStep(string stepId, out StepDefinition step) => Steps.TryGetValue(stepId, out step!);
}
