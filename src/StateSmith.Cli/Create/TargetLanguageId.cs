using System.Text.Json.Serialization;

namespace StateSmith.Cli.Create;

/// <summary>
/// DO NOT rename these values.
/// They are persisted in the user's configuration file.
/// You can change the order though.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetLanguageId
{
    C,
    Cpp,
    CppC,
    CSharp,
    JavaScript,
    TypeScript,
    Java,
    Python,
    Swift,
}
