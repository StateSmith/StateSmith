using FluentAssertions;
using StateSmith.Cli.Create;
using System.Reflection;
using Xunit;

namespace StateSmithCliTest.Create;

public class EmbeddedFilesTest
{
    [Fact]
    public void LoadDrawIoSimple1()
    {
        const string templateName = TemplateIds.DrawIoSimple1;

        var diagramContents = TemplateLoader.LoadDiagram(templateName, isDrawIoSelected: true);
        diagramContents.Should().Contain("mxCell");
    }

    [Fact]
    public void LoadPlantumlSimple1()
    {
        const string templateName = TemplateIds.PlantUmlSimple1;

        var diagramContents = TemplateLoader.LoadDiagram(templateName, isDrawIoSelected: false);
        diagramContents.Should().Contain("@startuml ");
    }
}
