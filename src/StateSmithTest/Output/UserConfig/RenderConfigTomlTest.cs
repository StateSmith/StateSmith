#nullable enable

using FluentAssertions;
using Tomlyn;
using Tomlyn.Model;
using Xunit;

namespace StateSmithTest.Output.UserConfig;

//public class RenderConfigTomlReader
//{

//}

public class RenderConfigTomlTest
{
    [Fact]
    public void TestParsingToml()
    {
        var toml = """"
            [RenderConfig]
            CFileExtension = ".inc"
            HFileTop = """
            // comment at top 1
            // comment at top 2
            """

            [RenderConfig.CSharp]
            NameSpace = "MyNamespace"
            UseNullable = false
            """";

        var model = Toml.ToModel(toml);
        var RenderConfig = (TomlTable)model["RenderConfig"];

        RenderConfig.Values.Should().HaveCount(3);

        {
            string CFileExtension = (string)RenderConfig["CFileExtension"];
            CFileExtension.Should().Be(".inc");

            string HFileTop = (string)RenderConfig["HFileTop"];
            HFileTop.ReplaceLineEndings("\n").ShouldBeShowDiff("// comment at top 1\n// comment at top 2\n");
        }

        {
            var CSharp = (TomlTable)RenderConfig["CSharp"];
            var NameSpace = (string)CSharp["NameSpace"];
            NameSpace.Should().Be("MyNamespace");

            var UseNullable = (bool)CSharp["UseNullable"];
            UseNullable.Should().BeFalse();

            //((string)CSharp["UseNullable"]).Should().Be("false");
        }

    }
}
