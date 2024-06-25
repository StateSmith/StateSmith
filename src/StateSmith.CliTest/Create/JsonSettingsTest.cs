using FluentAssertions;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest.Create;

public class JsonSettingsTest
{
    private JsonFilePersistence _persistence;

    public JsonSettingsTest()
    {
        _persistence = new JsonFilePersistence() { IncludeFields = false };
    }

    [Fact]
    public void EnumFromString()
    {
        var json = """
            {
              "StateSmithVersion": "0.9.9-alpha",
              "FileExtension": ".drawio",
              "TargetLanguageId": "JavaScript",
              "DrawIoDiagramTemplateId": "drawio-simple-1",
              "PlantUmlDiagramTemplateId": "plantuml-simple-1"
            }
            """;

        var settings = _persistence.RestoreFromString<Settings>(json);
        settings.TargetLanguageId.Should().Be(TargetLanguageId.JavaScript);
    }

    [Fact]
    public void EnumToString()
    {
        var settings = new Settings
        {
            UseCsxWorkflow = true,
            StateSmithVersion = "0.9.9-alpha",
            FileExtension = ".drawio",
            TargetLanguageId = TargetLanguageId.CppC,
            DrawIoDiagramTemplateId = "drawio-simple-1",
            PlantUmlDiagramTemplateId = "plantuml-simple-1"
        };

        var json = _persistence.PersistToString(settings).ConvertLineEndingsToN();
        json.ShouldBeShowDiff("""
            {
              "UseCsxWorkflow": true,
              "StateSmithVersion": "0.9.9-alpha",
              "FileExtension": ".drawio",
              "TargetLanguageId": "CppC",
              "DrawIoDiagramTemplateId": "drawio-simple-1",
              "PlantUmlDiagramTemplateId": "plantuml-simple-1"
            }
            """.ConvertLineEndingsToN());
    }
}
