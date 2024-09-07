using System.Text.Json.Serialization;

namespace StateSmith.Cli.Create;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetLanguageId
{
    C,
    CppC,
    CSharp,
    JavaScript,
    Java,
}
