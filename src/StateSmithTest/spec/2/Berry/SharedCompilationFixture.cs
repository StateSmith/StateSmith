#nullable enable

using System;
using System.IO;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmithTest.Processes;

namespace Spec.Spec2.Berry;

/// <summary>
/// Ensures we only compile the Spec2 diagram and build the Berry interpreter once per test run.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/berry-spec2/";

    private static readonly object buildLock = new();
    private static string? berryExecutablePath;

    public static string BerryExecutablePath
    {
        get
        {
            if (berryExecutablePath == null)
            {
                throw new InvalidOperationException("Berry executable has not been built yet.");
            }

            return berryExecutablePath;
        }
    }

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            runner.Settings.transpilerId = TranspilerId.Berry;
            runner.AlgoOrTranspilerUpdated();
            runner.Settings.outputGilCodeAlways = true;

            // normalize embedded diagram code so it runs under Berry
            runner.SmTransformer.transformationPipeline.Insert(0, new TransformationStep(action: sm =>
            {
                sm.VisitRecursively(node =>
                {
                    foreach (var behavior in node.Behaviors)
                    {
                        behavior.actionCode = BerryifyDiagramCode(behavior.actionCode);
                        behavior.guardCode = BerryifyDiagramCode(behavior.guardCode);
                    }
                });
            }));
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action, semiColon: "", trueString: "true");
        BuildBerryInterpreter();
    }

    private static void BuildBerryInterpreter()
    {
        lock (buildLock)
        {
            if (berryExecutablePath != null && File.Exists(berryExecutablePath))
            {
                return;
            }

            var repoRoot = Path.GetFullPath(Path.Combine(OutputDirectory, "../../.."));
            var berryDir = Path.Combine(repoRoot, "berry");
            var exeName = OperatingSystem.IsWindows() ? "berry.exe" : "berry";
            var exePath = Path.Combine(berryDir, exeName);

            if (!File.Exists(exePath))
            {
                SimpleProcess process = new()
                {
                    WorkingDirectory = berryDir,
                    ProgramPath = "make",
                    Args = ""
                };
                process.IfWindowsWrapWithCmd();
                process.throwOnStdErr = false; // Berry emits harmless warnings when built with GCC
                process.Run(SimpleProcess.DefaultLongTimeoutMs);
            }

            berryExecutablePath = exePath;
        }
    }

    private static string BerryifyDiagramCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        return code
            .Replace("++", " += 1")
            .Replace("--", " -= 1")
            .Replace("MainClass.", "Printer.");
    }

    public class MyGlueFile : IRenderConfigBerry
    {
        string IRenderConfig.FileTop => """
            # Berry Spec2 harness glue
            """;

        string IRenderConfig.VariableDeclarations => """
                self.count = 0
            """;

        string IRenderConfig.AutoExpandedVars => """
                self.auto_var_1 = 0
            """;

        string IRenderConfigBerry.Imports => """
            import Printer
            import Spec2SmBase
            """;

        string IRenderConfigBerry.Extends => "Spec2SmBase.Spec2SmBase";

        string IRenderConfigBerry.ClassCode => """
                def user_code()
                end
            """;

        public class Expansions : Spec2GenericVarExpansions
        {
            public override string trace(string message) => $"Printer.trace({message})";
            public override string trace_guard(string message, string guardCode) => $"self.trace_guard({message}, {guardCode})";
        }
    }
}
