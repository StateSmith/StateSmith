//using Xunit;
//using FluentAssertions;
//using System.IO;
//using StateSmith.Input.Expansions;
//using StateSmith.Output;
//using StateSmith.Cli.CompRun;

//namespace StateSmithTest;

//public class RoslynTest
//{
//    [Fact]
//    public void Test()
//    {
//        string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "/roslyn/ExampleExpansions123.cs";

//        var diagramToSmConverter = new ExternalCodeCompiler();

//        var codeCompilationResult = diagramToSmConverter.CompileCode(File.ReadAllText(filepath), "ExampleExpansions123");

//        codeCompilationResult.createdObject.Should().NotBeNull();
//        codeCompilationResult.failures.Should().BeNull();

//        Expander expander = new Expander();
//        ExpanderFileReflection expanderFileReflection = new(expander, new());
//        var expansionObject = (UserExpansionScriptBase)codeCompilationResult.createdObject;
//        expansionObject.VarsPath = "sm->vars.";
//        expanderFileReflection.AddAllExpansions(expansionObject);

//        expander.GetVariableNames().Should().BeEquivalentTo(new string[] {
//            "time",
//            "get_time",
//            "hit_count",
//            "jump_count",
//        });
//        expander.TryExpandVariableExpansion("time").Should().Be("system_get_time()");
//        expander.TryExpandVariableExpansion("get_time").Should().Be("system_get_time()");
//        expander.TryExpandVariableExpansion("hit_count").Should().Be("sm->vars.hit_count");
//        expander.TryExpandVariableExpansion("jump_count").Should().Be("sm->vars.jump_count");

//        expander.GetFunctionNames().Should().BeEquivalentTo(new string[] {
//            "set_mode",
//            "func",
//        });
//        expander.TryExpandFunctionExpansion("set_mode", new string[] { "GRUNKLE" }).Should().Be("set_mode(ENUM_PREFIX_GRUNKLE)");
//        expander.TryExpandFunctionExpansion("set_mode", new string[] { "STAN" }).Should().Be("set_mode(ENUM_PREFIX_STAN)");
//        expander.TryExpandFunctionExpansion("func", new string[] { }).Should().Be("123");
//    }
//}
