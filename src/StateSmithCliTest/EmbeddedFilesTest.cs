using FluentAssertions;
using StateSmithCli.Create;
using System.Reflection;
using Xunit;

namespace StateSmithCliTest;

public class EmbeddedFilesTest
{
    [Fact]
    public void Test1()
    {
        var csxContents = TemplateLoader.LoadCsx("drawio-simple-1");
        csxContents.Should().Contain("#r \"nuget: StateSmith,");
    }
}
