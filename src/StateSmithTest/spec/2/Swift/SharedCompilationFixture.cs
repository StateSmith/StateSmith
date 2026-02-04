#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmithTest.Processes;

namespace Spec.Spec2.Swift;

/// <summary>
/// Required so that we only do the dotnet compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/swift-spec2/Sources";

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            runner.Settings.transpilerId = TranspilerId.Swift;
            runner.AlgoOrTranspilerUpdated();
            runner.Settings.outputGilCodeAlways = true;

            runner.SmTransformer.transformationPipeline.Insert(0, new TransformationStep(action: (sm) => {
                sm.VisitRecursively((node) =>
                {
                    foreach (var behavior in node.Behaviors)
                    {
                        behavior.actionCode = SwiftifyDiagramCode(behavior.actionCode);
                        behavior.guardCode = SwiftifyDiagramCode(behavior.guardCode);
                    }
                });
            }));
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action, semiColon: "");

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = $"{OutputDirectory}/..",
            ProgramPath = "swift",
            Args = "build"
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    private static string SwiftifyDiagramCode(string str)
    {
        str = str.Replace(";", "");
        str = str.Replace("++", " += 1");
        str = str.Replace("<=1", "<= 1");
        return str;
    }

    public class MyGlueFile : IRenderConfigSwift
    {
        string IRenderConfig.FileTop => @"
            // any text you put in IRenderConfig.FileTop (like this comment) will be written to the generated file
            ";

        string IRenderConfigSwift.Imports => @"
            @MainActor
            ";

        string IRenderConfig.VariableDeclarations => @"
            public var count = 0
            ";

        string IRenderConfig.AutoExpandedVars => @"
            public var auto_var_1 = 0
            ";

        string IRenderConfigSwift.Extends => "Spec2SmBase";

        public class Expansions : Spec2GenericVarExpansions
        {
            public override string trace(string message) => $"MainClass.trace({message})"; // this isn't actually needed, but helps ensure expansions are working

            public override string trace_guard(string message, string guardCode) => $"Spec2SmBase.trace_guard({message}, {guardCode})";
        }
    }
}



