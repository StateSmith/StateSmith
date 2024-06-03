#nullable enable

using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.IO;
using System.Text;

namespace StateSmith.Output.Sim;

public class SimWebGenerator
{
    public RunnerSettings RunnerSettings => runner.Settings;

    private readonly ICodeFileWriter codeFileWriter;
    MermaidEdgeTracker mermaidEdgeTracker = new();
    TrackingExpander trackingExpander = new();
    TextWriter mermaidCodeWriter = new StringWriter();
    TextWriter mocksWriter = new StringWriter();
    SingleFileCapturer fileCapturer = new();
    StateMachineProvider stateMachineProvider;
    SmRunner runner;

    public SimWebGenerator(ICodeFileWriter codeFileWriter)
    {
        // Internally, the `SimWebGenerator` uses a `SmRunner` to transform the input diagram into a simulation web page.
        // To customize the transformation/code generation process, we register custom DI services with the `SmRunner`.

        this.codeFileWriter = codeFileWriter;
        DiServiceProvider simDiServiceProvider;

        runner = new(diagramPath: "placeholder-updated-in-generate-method.txt", renderConfig: new SimRenderConfig(), transpilerId: TranspilerId.JavaScript);
        runner.Settings.propagateExceptions = true;

        // Registering DI services must be done before accessing `runner.SmTransformer`.
        simDiServiceProvider = runner.GetExperimentalAccess().DiServiceProvider;
        simDiServiceProvider.AddSingletonT<IExpander>(trackingExpander);
        simDiServiceProvider.AddSingletonT<ICodeFileWriter>(fileCapturer);
        simDiServiceProvider.AddSingletonT<IConsolePrinter>(new DiscardingConsolePrinter());   // we want regular SmRunner console output to be discarded

        AdjustTransformationPipeline();
        stateMachineProvider = simDiServiceProvider.GetInstanceOf<StateMachineProvider>();
    }

    private void AdjustTransformationPipeline()
    {
        // Note! For `MermaidEdgeTracker` to function correctly, both below transformations must occur in the same `SmRunner`.
        // This allows us to easily map an SS behavior to its corresponding mermaid edge ID.

        const string GenMermaidCodeStepId = "gen-mermaid-code";
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_SupportHistory, new TransformationStep(id: GenMermaidCodeStepId, GenerateMermaidCode));
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, V1LoggingTransformationStep);

        // We to generate mermaid  history support (to avoid a ton of transitions being shown), but AFTER name conflict resolution.
        // See https://github.com/StateSmith/StateSmith/issues/302
        // Validate that this is true.
        int historyIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_SupportHistory);
        int nameConflictIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_NameConflictResolution);
        int mermaidIndex = runner.SmTransformer.GetMatchIndex(GenMermaidCodeStepId);
        if (mermaidIndex <= nameConflictIndex || mermaidIndex >= historyIndex)
            throw new Exception("Mermaid generation must occur after name conflict resolution and before history support.");
    }

    public void Generate(string diagramPath, string outputDir)
    {
        runner.Settings.DiagramPath = diagramPath;
        runner.Run();
        var smName = stateMachineProvider.GetStateMachine().Name;

        if (Directory.Exists(outputDir) == false)
            Directory.CreateDirectory(outputDir);

        string path = Path.Combine(outputDir, $"{smName}.sim.html");

        var sb = new StringBuilder();
        HtmlRenderer.Render(sb, smName: smName, mocksCode: mocksWriter.ToString(), mermaidCode: mermaidCodeWriter.ToString(), jsCode: fileCapturer.CapturedCode);
        codeFileWriter.WriteFile(path, code: sb.ToString());
    }

    void GenerateMermaidCode(StateMachine sm)
    {
        var visitor = new MermaidGenerator(mermaidEdgeTracker);
        visitor.RenderAll(sm);
        mermaidCodeWriter.WriteLine(visitor.GetMermaidCode());
    }


    void V1LoggingTransformationStep(StateMachine sm)
    {
        sm.VisitRecursively((Vertex vertex) =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                V1ModBehaviorsForSimulation(vertex, behavior);
            }

            V1AddEntryExitTracing(sm, vertex);
            V1AddEdgeTracing(vertex);
        });
    }

    void V1AddEdgeTracing(Vertex vertex)
    {
        foreach (var b in vertex.TransitionBehaviors())
        {
            if (mermaidEdgeTracker.ContainsEdge(b))
            {
                // Note: most history behaviors will not be shown in the mermaid diagram
                var domId = "edge" + mermaidEdgeTracker.GetEdgeId(b);
                // NOTE! Avoid single quotes in ss guard/action code until bug fixed: https://github.com/StateSmith/StateSmith/issues/282
                b.actionCode += $"this.tracer.edgeTransition(\"{domId}\");";
            }
        }
    }

    static void V1AddEntryExitTracing(StateMachine sm, Vertex vertex)
    {
        // we purposely don't want to trace the entry/exit of the state machine itself.
        // That's why we use `State` instead of `NamedVertex`.
        if (vertex is State state)
        {
            var id = $"{sm.Name}.StateId.{state.Name.ToUpper()}";
            state.AddEnterAction($"this.tracer?.enterState({id});", index: 0);
            state.AddExitAction($"this.tracer?.exitState({id});");  // TODO - can we remove null conditional operator? It's not needed anymore?
        }
    }

    void V1ModBehaviorsForSimulation(Vertex vertex, Behavior behavior)
    {
        if (behavior.HasActionCode())
        {
            // GIL is Generic Intermediary Language. It is used by history vertices and other special cases.
            if (behavior.actionCode.Contains("$gil("))
            {
                // keep actual code
                behavior.actionCode += $"this.tracer.log(\"Executed action: \" + {FsmCodeToJsString(behavior.actionCode)});";
            }
            else
            {
                // we don't want to execute the action, just log it.
                behavior.actionCode = $"this.tracer.log(\"FSM would execute action: \" + {FsmCodeToJsString(behavior.actionCode)});";
            }
        }

        if (vertex is HistoryVertex)
        {
            if (behavior.HasGuardCode())
            {
                // we want the history vertex to work as is without prompting the user to evaluate those guards.
                var logCode = $"this.tracer.log(\"History state evaluating guard: \" + {FsmCodeToJsString(behavior.guardCode)})";
                var actualCode = behavior.guardCode;
                behavior.guardCode = $"{logCode} || {actualCode}";
            }
            else
            {
                behavior.actionCode += $"this.tracer.log(\"History state taking default transition.\");";
            }
        }
        else
        {
            if (behavior.HasGuardCode())
            {
                var logCode = $"this.tracer.log(\"User evaluating guard: \" + {FsmCodeToJsString(behavior.guardCode)})";
                var confirmCode = $"this.evaluateGuard({FsmCodeToJsString(behavior.guardCode)})";
                behavior.guardCode = $"{logCode} || {confirmCode}";
                // NOTE! logCode doesn't return a value, so the confirm code will always be evaluated.
            }
        }
    }

    static string FsmCodeToJsString(string code)
    {
        code = code.ReplaceLineEndings("\\n");  // need to escape newlines for fsm code that spans multiple lines
        return "\"" + code.Replace("\"", "\\\"") + "\"";
    }

    public class SimRenderConfig : IRenderConfigJavaScript
    {
        string IRenderConfigJavaScript.ClassCode => @"        
        // Null by default.
        // May be overridden to override guard evaluation (eg. in a simulator)
        evaluateGuard = null;
    ";
    }
}
