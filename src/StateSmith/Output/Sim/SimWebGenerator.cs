#nullable enable

using StateSmith.Common;
using StateSmith.Input.Expansions;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.Gil;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

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
    NameMangler nameMangler;
    Regex historyGilRegex;

    /// <summary>
    /// We want to show the user their original event names in the simulator.
    /// Not the sanitized names.
    /// </summary>
    HashSet<string> diagramEventNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Mapping from state to available events
    /// Key is state name, value is the set of event names that the state can handle
    /// </summary>
    Dictionary<string, HashSet<string>> stateToAvailableEvents = new(StringComparer.OrdinalIgnoreCase);

    BehaviorTracker behaviorTracker = new();

    SmRunner runner;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="codeFileWriter">Code file writer</param>
    /// <param name="mainRunnerSettings">Main runner settings</param>
    public SimWebGenerator(ICodeFileWriter codeFileWriter, RunnerSettings mainRunnerSettings)
    {
        // NOTE! we need mainRunnerSettings so that we can use the same algorithm as the main runner.
        // This needs to happen during construction because of dependency injection.

        // Internally, the `SimWebGenerator` uses a `SmRunner` to transform the input diagram into a simulation web page.
        // To customize the transformation/code generation process, we register custom DI services with the `SmRunner`.

        this.codeFileWriter = codeFileWriter;

        var enablePreDiagramBasedSettings = false;  // need to stop it from trying to read diagram early as fake diagram path is used
        runner = new(diagramPath: "placeholder-updated-in-generate-method.txt", renderConfig: new SimRenderConfig(), transpilerId: TranspilerId.JavaScript, algorithmId: mainRunnerSettings.algorithmId, serviceOverrides: (services)=>
        {
            services.AddSingleton<IExpander>(trackingExpander);
            services.AddSingleton<ICodeFileWriter>(fileCapturer);
            services.AddSingleton<IConsolePrinter>(new DiscardingConsolePrinter());   // we want regular SmRunner console output to be discarded            
        }, enablePDBS: enablePreDiagramBasedSettings);
        runner.Settings.propagateExceptions = true;

        // Registering DI services must be done before accessing `runner.SmTransformer`.
        DiServiceProvider simDiServiceProvider = runner.GetExperimentalAccess().DiServiceProvider;
        AdjustTransformationPipeline();
        PreventCertainDiagramSpecifiedSettings(simDiServiceProvider.GetInstanceOf<RenderConfigBaseVars>());

        stateMachineProvider = simDiServiceProvider.GetInstanceOf<StateMachineProvider>();

        nameMangler = simDiServiceProvider.GetInstanceOf<NameMangler>();

        SetupGilHistoryRegex();
    }

    /// <summary>
    /// Prevent user diagram settings that could mess up the generated simulation.
    /// https://github.com/StateSmith/StateSmith/issues/337
    /// </summary>
    /// <param name="renderConfigBaseVars">Render configuration base variables</param>
    private void PreventCertainDiagramSpecifiedSettings(RenderConfigBaseVars renderConfigBaseVars)
    {
        DiagramBasedSettingsPreventer.Process(runner.SmTransformer, action: (readRenderConfigAllVars, _) =>
        {
            // copy only the settings that are safe to copy for the simulation
            renderConfigBaseVars.TriggerMap = readRenderConfigAllVars.Base.TriggerMap;
        });
    }

    /// <summary>
    /// GIL is Generic Intermediary Language. It is used by history vertices and other special cases.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    [MemberNotNull(nameof(historyGilRegex))]
    private void SetupGilHistoryRegex()
    {
        if (nameMangler.HistoryVarEnumTypePostfix != "_HistoryId")
            throw new InvalidOperationException("Expected HistoryVarEnumTypePostfix to be '_HistoryId' for regex below");

        if (nameMangler.HistoryVarNamePostfix != "_history")
            throw new InvalidOperationException("Expected HistoryVarNamePostfix to be '_history' for regex below");

        if (GilCreationHelper.GilExpansionMarkerFuncName != "$gil")
            throw new InvalidOperationException("Expected GilExpansionMarkerFuncName to be '$gil' for regex below");

        // want to match: `$gil(this.vars.Running_history = Running_HistoryId.SETUPCHECK__START;)`
        historyGilRegex = new(@"(?x)
        \$gil\(
            \s*
            this\.vars\.
            (?<varName>\w+)_history         # e.g. Running_history
            \s* = \s*
            \w+ [.] (?<storedStateName>\w+);   # e.g. Running_HistoryId.SETUPCHECK__START
        \)
    ");
    }

    private void AdjustTransformationPipeline()
    {
        // Note! For `MermaidEdgeTracker` to function correctly, both below transformations must occur in the same `SmRunner`.
        // This allows us to easily map an SS behavior to its corresponding mermaid edge ID.

        const string GenMermaidCodeStepId = "gen-mermaid-code";
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_SupportHistory, new TransformationStep(id: GenMermaidCodeStepId, GenerateMermaidCode));
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, V1LoggingTransformationStep);
        
        // collect diagram names after trigger mapping completes
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_TriggerMapping, CollectDiagramNames);

        // We to generate mermaid diagram before history support (to avoid a ton of transitions being shown), but AFTER name conflict resolution.
        // See https://github.com/StateSmith/StateSmith/issues/302
        // Validate that this is true.
        int historyIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_SupportHistory);
        int nameConflictIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_NameConflictResolution);
        int mermaidIndex = runner.SmTransformer.GetMatchIndex(GenMermaidCodeStepId);
        if (mermaidIndex <= nameConflictIndex || mermaidIndex >= historyIndex)
            throw new Exception("Mermaid generation must occur after name conflict resolution and before history support.");

        // show default 'do' events in mermaid diagram
         runner.SmTransformer.InsertBeforeFirstMatch(GenMermaidCodeStepId, (StateMachine sm) => { DefaultToDoEventVisitor.Process(sm); });
    }

    private void CollectDiagramNames(StateMachine sm)
    {
        sm.VisitRecursively((Vertex vertex) =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                foreach (var trigger in behavior.Triggers)
                {
                    if (TriggerHelper.IsEvent(trigger))
                        diagramEventNames.Add(trigger);
                }
            }

            // Collect available events for each state
            if (vertex is State state)
            {
                var availableEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                // Collect events from this state and all its ancestors
                Vertex? currentVertex = state;
                while (currentVertex != null)
                {
                    foreach (var behavior in currentVertex.Behaviors)
                    {
                        foreach (var trigger in behavior.Triggers)
                        {
                            if (TriggerHelper.IsEvent(trigger))
                            {
                                availableEvents.Add(trigger);
                            }
                        }
                    }
                    currentVertex = currentVertex.Parent;
                }

                if (availableEvents.Count > 0)
                {
                    stateToAvailableEvents[state.Name] = availableEvents;
                }
            }
        });
    }


    public void Generate(string diagramPath, string outputDir)
    {
        runner.Settings.DiagramPath = diagramPath;
        runner.Run();
        var smName = stateMachineProvider.GetStateMachine().Name;

        if (Directory.Exists(outputDir) == false)
            Directory.CreateDirectory(outputDir);

        string path = Path.Combine(outputDir, $"{smName}.sim.html");

        string diagramEventNamesArray = OrganizeEventNamesIntoJsArray(diagramEventNames);

        // Organize state event mapping into JavaScript object format
        string stateEventsMapping = OrganizeStateEventsIntoJsObject();

        // Build HTML content
        var sb = new StringBuilder();
        HtmlRenderer.Render(sb,
            smName: smName,
            mocksCode: mocksWriter.ToString(),
            mermaidCode: mermaidCodeWriter.ToString(),
            jsCode: fileCapturer.CapturedCode,
            diagramEventNamesArray: diagramEventNamesArray,
            stateEventsMapping: stateEventsMapping);
            
        codeFileWriter.WriteFile(path, code: sb.ToString());
    }

    private static string OrganizeEventNamesIntoJsArray(HashSet<string> unOrderedEventNames)
    {
        string? doEvent = null;
        List<string> eventNames = new();

        foreach (var name in unOrderedEventNames)
        {
            if (TriggerHelper.IsDoEvent(name))
            {
                doEvent = name;
            }
            else
            {
                eventNames.Add(name);
            }
        }

        // sort non-do events
        eventNames.Sort(StringComparer.OrdinalIgnoreCase);  // case-insensitive sort

        // put do event first
        if (doEvent != null)
        {
            eventNames.Insert(0, doEvent);
        }

        var diagramEventNamesArray = "[";
        foreach (var name in eventNames)
        {
            diagramEventNamesArray += $"'{name}', ";
        }
        diagramEventNamesArray += "]";
        return diagramEventNamesArray;
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
                behaviorTracker.RecordOriginalBehavior(behavior);
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
                b.actionCode += $"this.tracer?.edgeTransition(\"{domId}\");";
            }
        }
    }

    void V1AddEntryExitTracing(StateMachine sm, Vertex vertex)
    {
        // we purposely don't want to trace the entry/exit of the state machine itself.
        // That's why we use `State` instead of `NamedVertex`.
        if (vertex is State state)
        {
            var mermaidName = state.Name;
            state.AddEnterAction($"this.tracer?.enterState('{mermaidName}');", index: 0);
            state.AddExitAction($"this.tracer?.exitState('{mermaidName}');");
        }
    }

    void V1ModBehaviorsForSimulation(Vertex vertex, Behavior behavior)
    {
        if (behavior.HasActionCode())
        {
            var historyGilMatch = historyGilRegex.Match(behavior.actionCode);
            
            if (historyGilMatch.Success)
            {
                // TODO https://github.com/StateSmith/StateSmith/issues/323
                // show history var updating
                // var historyVarName = historyGilMatch.Groups["varName"].Value;
                // var storedStateName = historyGilMatch.Groups["storedStateName"].Value;
                // behavior.actionCode += $"this.tracer?.log('📝 History({historyVarName}) = {storedStateName}');";
            }
            else
            {
                // we don't want to execute the action, just log it.
                behavior.actionCode = $"this.tracer?.log(\"⚡ FSM would execute action: \" + {FsmCodeToJsString(behavior.actionCode)});";
            }
        }

        if (vertex is HistoryVertex)
        {
            if (behavior.HasGuardCode())
            {
                // we want the history vertex to work as is without prompting the user to evaluate those guards.
                behavior.actionCode += $"this.tracer?.log(\"🕑 History: transitioning to {Vertex.Describe(behavior.TransitionTarget)}.\");";
            }
            else
            {
                behavior.actionCode += $"this.tracer?.log(\"🕑 History: default transition.\");";
            }
        }
        else
        {
            if (behavior.HasGuardCode())
            {
                var logCode = $"this.tracer?.log(\"🛡️ User evaluating guard: \" + {FsmCodeToJsString(behavior.guardCode)})";
                var originalBehaviorUml = behaviorTracker.GetOriginalUmlOrCurrent(behavior);
                var confirmCode = $"this.evaluateGuard(\"{Vertex.Describe(behavior.OwningVertex)}\",{FsmCodeToJsString(originalBehaviorUml)})";
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

    /// <summary>
    /// Convert state-to-available-events mapping to JavaScript object format
    /// </summary>
    /// <returns>String in JavaScript object format</returns>
    private string OrganizeStateEventsIntoJsObject()
    {
        // Convert to a dictionary with sorted event arrays for consistent output
        var sortedStateEvents = stateToAvailableEvents.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToArray()
        );
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null // Keep original property names
        };
        
        return JsonSerializer.Serialize(sortedStateEvents, options);
    }
}
