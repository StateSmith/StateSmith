using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmithTest.CodeFileWriterTests;

// See https://github.com/StateSmith/StateSmith/issues/105
public class UserRemoveStateIdFuncAddCoverage
{
    public class MyCodeFileWriter : ICodeFileWriter
    {
        public static string RemoveStateIdToStringFunction(string code)
        {
            // See https://www.debuggex.com/r/FiMETjYifnxL-sxh
            Regex regex = new(@"(?xm)
                # matches any leading comments that are attached
                (?:
                    (?: 
                        //.* 
                        |
                        /[*] [\s\S]*? [*]/
                    )
                    (?: \r\n | \r | \n ) # a line ending
                )*

                ^ [\w*\s]+ state_id_to_string \s* [(]

                # matches function prototype or body
                (?:
                    [^)]+ [)]   # matches function parameters and closing brace ')'
                    \s*
                    ;
                    |
                    [\s\S]+? # matches anything lazily
                    ^}
                    \s*?
                )
                (?: \r\n | \r | \n )? # an optional line ending
            ");

            code = regex.Replace(input: code, replacement: "", count: 1);

            return code;
        }

        public static string SurroundRootExitWithAnnotations(string code)
        {
            // See https://www.debuggex.com/r/bCAg1nmGxB9Zeim0
            Regex regex = new(@"(?xm)
                (?<original_function> # named capture group
                    ^static \s+ void \s+ ROOT_exit \s* [(] [^)]* [)] \s* [{]
                    [^}]+   # match everything until closing brace
                    [}]
                    [ \t]*  # any horizontal whitespace
                    (?: \r\n | \r | \n ) # a line ending
                )
            ");

            var replacement = StringUtils.DeIndentTrim(@"
                // The root exit function can't actually be called so we add code coverage hints.
                // LCOV_EXCLUDE_START
                // GCOV_EXCLUDE_START
                // GCOVR_EXCLUDE_START
                ${original_function}// LCOV_EXCLUDE_STOP
                // GCOV_EXCLUDE_STOP
                // GCOVR_EXCLUDE_STOP
            ");

            code = regex.Replace(input: code, replacement: replacement, count: 1);

            return code;
        }

        void ICodeFileWriter.WriteFile(string filePath, string code)
        {
            code = RemoveStateIdToStringFunction(code);
            code = SurroundRootExitWithAnnotations(code);
            File.WriteAllText(path: filePath, code);
        }
    }

    // 
    [Fact]
    public void ExampleCustomCodeGen()
    {
        var spBuilder = DefaultServiceProviderBuilder.CreateDefault((services) =>
        {
            // register our custom code file writer
            services.AddSingleton<ICodeFileWriter, MyCodeFileWriter>();
        });
        using (spBuilder)
        {
            SmRunner runner = new(diagramPath: "Ex1.drawio.svg", algorithmId: AlgorithmId.Balanced1, serviceProvider: spBuilder.Build());

            // adjust settings because we are unit testing. Normally wouldn't do below.
            runner.Settings.propagateExceptions = true;
            runner.Settings.outputStateSmithVersionInfo = false;

            // run StateSmith with our custom code file writer
            runner.Run();

            // Test that we saw the expected output from your custom code generator.
            var cCode = File.ReadAllText(runner.Settings.outputDirectory + "Ex1.c");
            var hCode = File.ReadAllText(runner.Settings.outputDirectory + "Ex1.h");

            cCode.Should().NotContain("state_id_to_string");
            hCode.Should().NotContain("state_id_to_string");

            cCode.Should().Contain("// LCOV_EXCLUDE_START");
        }
    }
}
