using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;

namespace StateSmith.Output.Sim;

public class SimWebGenerator
{
    public RunnerSettings RunnerSettings => runner.Settings;

    string diagramPath;
    private readonly string outputDir;
    MermaidEdgeTracker mermaidEdgeTracker = new();
    TrackingExpander trackingExpander = new();
    TextWriter mermaidCodeWriter = new StringWriter();
    TextWriter mocksWriter = new StringWriter();
    SingleFileCapturer fileCapturer = new();
    StateMachineProvider stateMachineProvider;
    SmRunner runner;

    public SimWebGenerator(string diagramPath, string outputDir)
    {
        this.diagramPath = diagramPath;
        this.outputDir = outputDir;
        DiServiceProvider diServiceProvider;

        runner = new(diagramPath: diagramPath, renderConfig: new SimRenderConfig(), transpilerId: TranspilerId.JavaScript);
        runner.Settings.propagateExceptions = true;

        // Registering DI services must be done before accessing `runner.SmTransformer`.
        diServiceProvider = runner.GetExperimentalAccess().DiServiceProvider;
        diServiceProvider.AddSingletonT<IExpander>(trackingExpander);
        diServiceProvider.AddSingletonT<ICodeFileWriter>(fileCapturer);
        diServiceProvider.AddSingletonT<IConsolePrinter>(new DiscardingConsolePrinter());

        // Note! For `MermaidEdgeTracker` to function correctly, both below transformations must occur in the same `SmRunner`.
        // This allows us to easily map an SS behavior to its corresponding mermaid edge ID.
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_RemoveNotesVertices, new TransformationStep(id: "some string id", GenerateMermaidCode));
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, new TransformationStep(id: "my custom step blah", V1LoggingTransformationStep));
        stateMachineProvider = diServiceProvider.GetInstanceOf<StateMachineProvider>();
    }

    public void Generate()
    {
        runner.Run();
        var smName = stateMachineProvider.GetStateMachine().Name;

        if (Directory.Exists(outputDir) == false)
            Directory.CreateDirectory(outputDir);

        using StreamWriter htmlWriter = new(Path.Combine(outputDir, $"{smName}.sim.html"));
        HtmlRenderer.Render(htmlWriter, smName: smName, mocksCode: mocksWriter.ToString(), mermaidCode: mermaidCodeWriter.ToString(), jsCode: fileCapturer.CapturedCode);
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
                behavior.actionCode += $"this.tracer.log(\"Executed action: \" + {EscapeFsmCode(behavior.actionCode)});";
            }
            else
            {
                // we don't want to execute the action, just log it.
                behavior.actionCode = $"this.tracer.log(\"FSM would execute action: \" + {EscapeFsmCode(behavior.actionCode)});";
            }
        }

        if (vertex is HistoryVertex)
        {
            if (behavior.HasGuardCode())
            {
                // we want the history vertex to work as is without prompting the user to evaluate those guards.
                var logCode = $"this.tracer.log(\"History state evaluating guard: \" + {EscapeFsmCode(behavior.guardCode)})";
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
                var logCode = $"this.tracer.log(\"User evaluating guard: \" + {EscapeFsmCode(behavior.guardCode)})";
                var confirmCode = $"this.evaluateGuard({EscapeFsmCode(behavior.guardCode)})";
                behavior.guardCode = $"{logCode} || {confirmCode}";
                // NOTE! logCode doesn't return a value, so the confirm code will always be evaluated.
            }
        }
    }

    string EscapeFsmCode(string code)
    {
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
