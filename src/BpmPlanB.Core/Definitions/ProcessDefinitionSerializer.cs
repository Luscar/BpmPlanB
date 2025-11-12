using System;
using System.Threading.Tasks;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BpmPlanB.Core.Definitions;

/// <summary>
/// Utility helpers used to load and persist process definitions.
/// </summary>
public static class ProcessDefinitionSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Loads a process definition from a JSON payload.
    /// </summary>
    /// <param name="json">String containing the process definition.</param>
    /// <returns>The parsed <see cref="ProcessDefinition"/>.</returns>
    public static ProcessDefinition Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("The JSON payload cannot be null or whitespace.", nameof(json));
        }

        var definition = JsonSerializer.Deserialize<ProcessDefinition>(json, Options)
                         ?? throw new InvalidDataException("Unable to parse the provided process definition.");
        definition.Validate();
        return definition;
    }

    /// <summary>
    /// Loads a process definition from a stream containing JSON.
    /// </summary>
    /// <param name="stream">Stream pointing to the JSON document.</param>
    /// <returns>The parsed <see cref="ProcessDefinition"/>.</returns>
    public static async Task<ProcessDefinition> DeserializeAsync(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var definition = await JsonSerializer.DeserializeAsync<ProcessDefinition>(stream, Options).ConfigureAwait(false)
                         ?? throw new InvalidDataException("Unable to parse the provided process definition.");
        definition.Validate();
        return definition;
    }

    /// <summary>
    /// Serializes a process definition to a JSON payload.
    /// </summary>
    /// <param name="definition">Process definition instance.</param>
    /// <returns>A JSON string representing the process definition.</returns>
    public static string Serialize(ProcessDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        definition.Validate();
        return JsonSerializer.Serialize(definition, Options);
    }

    /// <summary>
    /// Serializes a process definition to a stream.
    /// </summary>
    /// <param name="definition">Process definition instance.</param>
    /// <param name="stream">Destination stream.</param>
    public static Task SerializeAsync(ProcessDefinition definition, Stream stream)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        definition.Validate();
        return JsonSerializer.SerializeAsync(stream, definition, Options);
    }

    /// <summary>
    /// Loads a process definition from a JSON file.
    /// </summary>
    /// <param name="path">Path to the JSON file.</param>
    /// <returns>The parsed <see cref="ProcessDefinition"/>.</returns>
    public static async Task<ProcessDefinition> LoadFromFileAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The file path cannot be null or whitespace.", nameof(path));
        }

        await using var stream = File.OpenRead(path);
        return await DeserializeAsync(stream).ConfigureAwait(false);
    }

    /// <summary>
    /// Persists a process definition as JSON in a file.
    /// </summary>
    /// <param name="definition">Process definition instance.</param>
    /// <param name="path">Destination file path.</param>
    public static async Task SaveToFileAsync(ProcessDefinition definition, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The file path cannot be null or whitespace.", nameof(path));
        }

        await using var stream = File.Create(path);
        await SerializeAsync(definition, stream).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a configured <see cref="JsonSerializerOptions"/> instance that can be reused by consuming applications.
    /// </summary>
    public static JsonSerializerOptions GetSerializerOptions() => new(Options);
}
