//using Xunit;
//using FluentAssertions;
//using StateSmith.Output;
//using System.IO;
//using StateSmith.Input.Expansions;
//using StateSmith.Output.UserConfig;

//namespace StateSmithTest.roslyn
//{
//    public class ConfigReaderTest
//    {
//        [Fact]
//        public void Test()
//        {
//            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "/roslyn/ExampleCAndJs.cs";

//            var diagramToSmConverter = new ExternalCodeCompiler();

//            var codeCompilationResult = diagramToSmConverter.CompileCode(File.ReadAllText(filepath), typeName: typeof(StateSmithTest.roslyn.OvenC).FullName);

//            codeCompilationResult.createdObject.Should().NotBeNull();
//            codeCompilationResult.failures.Should().BeNull();

//            Expander expander = new Expander();
//            ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);
//            var configObject = (IRenderConfigC)codeCompilationResult.createdObject;

//            configObject.HFileTop.Should().Be(
//@"/**
// * This is a header for the .h file
// */
//#include <stdbool.h>
//#include ""some_stuff.h"""
//            );

//            configObject.CFileTop.Should().Be(
//@"/**
// * This is a header for the .c file
// */
//#include <stddef.h>
//#include ""my_stuff.h"""
//            );

//            configObject.VariableDeclarations.Should().Be(
//@"uint16_t count = 0;
//bool flag = false;"
//            );

//            ExpansionConfigReader reader = new(expander, expansionVarsPath: "sm->vars.");
//            reader.ReadObject(new ExpansionConfigReaderObjectProvider(configObject));

//            expander.GetVariableNames().Should().BeEquivalentTo(new string[] {
//                "a_count",
//            });
//            expander.TryExpandVariableExpansion("a_count").Should().Be("sm->vars.a_count");

//            expander.GetFunctionNames().Should().BeEquivalentTo(new string[] {
//                "some_guard",
//                "b_exit",
//                "set_mode",
//            });
//            expander.TryExpandFunctionExpansion("some_guard", new string[] { "7" }).Should().Be("some_guard(7)");
//            expander.TryExpandFunctionExpansion("some_guard", new string[] { "123" }).Should().Be("some_guard(1123)");
//            expander.TryExpandFunctionExpansion("b_exit", new string[] { }).Should().Be("b_exit_count++");
//            expander.TryExpandFunctionExpansion("set_mode", new string[] { "SUSHI" }).Should().Be("set_mode(MODE_SUSHI)");
//        }
//    }
//}
