#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmithTest.Processes;

namespace Spec.Spec2.Python;

/// <summary>
/// Required so that we only do the dotnet compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/python-spec2/";

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            runner.Settings.transpilerId = TranspilerId.Python;
            runner.AlgoOrTranspilerUpdated();
            runner.Settings.outputGilCodeAlways = true;

            runner.SmTransformer.transformationPipeline.Insert(0, new TransformationStep(action: (sm) => {
                // remove semi-colons from diagram actions
                sm.VisitRecursively((node) =>
                {
                    foreach (var behavior in node.Behaviors)
                    {
                        behavior.actionCode = behavior.actionCode.Replace(";", "");
                        behavior.actionCode = behavior.actionCode.Replace("++", " += 1");
                    }
                });
            }));
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action, semiColon: "");

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            ProgramPath = "python",
            Args = " -m compileall ."   // https://stackoverflow.com/questions/5607283/how-can-i-manually-generate-a-pyc-file-from-a-py-file
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    public class MyGlueFile : IRenderConfigPython
    {
        string IRenderConfig.FileTop => @"
            # any text you put in IRenderConfig.FileTop (like this comment) will be written to the generated file
            ";

        string IRenderConfig.VariableDeclarations => @"
            self.count = 0
            ";

        string IRenderConfig.AutoExpandedVars => @"
            self.auto_var_1 = 0
            ";

        string IRenderConfigPython.Extends => "Spec2SmBase";

        string IRenderConfigPython.Imports => @"
            from Spec2SmBase import Spec2SmBase
            ";

        public class Expansions : Spec2GenericVarExpansions
        {
            public override string SemiColon => "";
            public override string trace(string message) => $"MainClass.trace({message})"; // this isn't actually needed, but helps ensure expansions are working
        }
    }
}



