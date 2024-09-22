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

    /// <summary>
    /// This ensure that we can restore all the TargetLanguageId values
    /// and that none of enum member names have been changed.
    /// </summary>
    [Fact]
    public void TestAllTargetLanguageIds()
    {
        Test("C", TargetLanguageId.C);
        Test("CppC", TargetLanguageId.CppC);
        Test("CSharp", TargetLanguageId.CSharp);
        Test("JavaScript", TargetLanguageId.JavaScript);
        Test("TypeScript", TargetLanguageId.TypeScript);
        Test("Java", TargetLanguageId.Java);
        Test("Python", TargetLanguageId.Python);

        void Test(string languageIdName, TargetLanguageId targetLanguageId)
        {
            var json = $$"""
            {
              "TargetLanguageId": "{{languageIdName}}",
            }
            """;

            var settings = _persistence.RestoreFromString<Settings>(json);
            settings.TargetLanguageId.Should().Be(targetLanguageId);
        }
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
