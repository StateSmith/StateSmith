using FluentAssertions;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using StateSmith.Runner;
using StateSmithTest;
using System;
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
        // Ensure that none of the enum member names have been changed. This is important as the enum values are persisted in the user's configuration file.
        Enum.GetNames(typeof(TargetLanguageId)).Should().BeEquivalentTo(["C", "Cpp", "CppC", "CSharp", "JavaScript", "TypeScript", "Java", "Python", "Berry"]);

        // Detect if a new language has been added.
        Enum.GetNames(typeof(TranspilerId)).Should().BeEquivalentTo(["NotYetSet", "Default", "C99", "Cpp", "CSharp", "JavaScript", "TypeScript", "Java", "Python", "Berry"]);

        Test("C", TargetLanguageId.C);
        Test("CppC", TargetLanguageId.CppC);
        Test("Cpp", TargetLanguageId.Cpp);
        Test("CSharp", TargetLanguageId.CSharp);
        Test("JavaScript", TargetLanguageId.JavaScript);
        Test("TypeScript", TargetLanguageId.TypeScript);
        Test("Java", TargetLanguageId.Java);
        Test("Python", TargetLanguageId.Python);
        Test("Berry", TargetLanguageId.Berry);

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
