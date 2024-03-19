using FluentAssertions;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest.Create;

public class JsonSettingsTest
{
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

        var settings =  JsonFilePersistence.RestoreFromString<Settings>(json);
        settings.TargetLanguageId.Should().Be(TargetLanguageId.JavaScript);
    }

    [Fact]
    public void EnumToString()
    {
        var settings = new Settings
        {
            StateSmithVersion = "0.9.9-alpha",
            FileExtension = ".drawio",
            TargetLanguageId = TargetLanguageId.CppC,
            DrawIoDiagramTemplateId = "drawio-simple-1",
            PlantUmlDiagramTemplateId = "plantuml-simple-1"
        };

        var json = JsonFilePersistence.PersistToString(settings).ConvertLineEndingsToN();
        json.ShouldBeShowDiff("""
            {
              "StateSmithVersion": "0.9.9-alpha",
              "FileExtension": ".drawio",
              "TargetLanguageId": "CppC",
              "DrawIoDiagramTemplateId": "drawio-simple-1",
              "PlantUmlDiagramTemplateId": "plantuml-simple-1"
            }
            """.ConvertLineEndingsToN());
    }
}
