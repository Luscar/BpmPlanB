using System.Text.Json;
using System.Text.Json.Serialization;
using BpmPlanB.Model;

namespace BpmPlanB.Serialization;

public static class ProcessDefinitionLoader
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static ValueTask<ProcessDefinition?> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync<ProcessDefinition>(stream, Options, cancellationToken);
    }

    public static ProcessDefinition? FromJson(string json)
    {
        return JsonSerializer.Deserialize<ProcessDefinition>(json, Options);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
