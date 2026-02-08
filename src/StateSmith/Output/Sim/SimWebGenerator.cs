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

namespace StateSmith.Output.Sim;

public class SimWebGenerator
{
    public RunnerSettings RunnerSettings => runner.Settings;

    private readonly ICodeFileWriter codeFileWriter;
    MermaidEdgeTracker mermaidEdgeTracker = new();
    TrackingExpander trackingExpander = new();
    TextWriter mermaidCodeWriter = new StringWriter();
    SingleFileCapturer fileCapturer = new();
    StateMachineProvider stateMachineProvider;
    NameMangler nameMangler;
    Regex gilHistoryVarSetRegex;

    // https://github.com/StateSmith/StateSmith/issues/512
    HashSet<Behavior> behaviorsAddedForIssue512 = new();

    // https://github.com/StateSmith/StateSmith/issues/512
    HashSet<Behavior> behaviorsToModifyForIssue512 = new();

    BehaviorTracker originalBehaviorTracker = new();

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

    /// <summary>
    /// Mapping from state to available transition edges
    /// https://github.com/StateSmith/StateSmith/issues/522
    /// </summary>
    Dictionary<string, HashSet<int>> stateToAvailableEdgeIds = new();

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/523
    /// </summary>
    Dictionary<string, string> stateDescriptionMapping = new();

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
        DiServiceProvider simDiServiceProvider;

        var enablePreDiagramBasedSettings = false;  // need to stop it from trying to read diagram early as fake diagram path is used
        runner = new(diagramPath: "placeholder-updated-in-generate-method.txt", renderConfig: new SimRenderConfig(), transpilerId: TranspilerId.JavaScript, algorithmId: mainRunnerSettings.algorithmId, enablePDBS: enablePreDiagramBasedSettings);
        runner.Settings.propagateExceptions = true;
        runner.Settings.stateMachineName = mainRunnerSettings.stateMachineName;  // copy over the state machine name

        // Registering DI services must be done before accessing `runner.SmTransformer`.
        simDiServiceProvider = runner.GetExperimentalAccess().DiServiceProvider;
        simDiServiceProvider.AddSingletonT<IExpander>(trackingExpander);
        simDiServiceProvider.AddSingletonT<ICodeFileWriter>(fileCapturer);
        simDiServiceProvider.AddSingletonT<IConsolePrinter>(new DiscardingConsolePrinter());   // we want regular SmRunner console output to be discarded
        AdjustTransformationPipeline();
        PreventCertainDiagramSpecifiedSettings(simDiServiceProvider.GetInstanceOf<RenderConfigBaseVars>());

        stateMachineProvider = simDiServiceProvider.GetInstanceOf<StateMachineProvider>();

        nameMangler = simDiServiceProvider.GetInstanceOf<NameMangler>();

        SetupGilHistoryVarSetRegex();
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
    [MemberNotNull(nameof(gilHistoryVarSetRegex))]
    private void SetupGilHistoryVarSetRegex()
    {
        if (nameMangler.HistoryVarEnumTypePostfix != "_HistoryId")
            throw new InvalidOperationException("Expected HistoryVarEnumTypePostfix to be '_HistoryId' for regex below");

        if (nameMangler.HistoryVarNamePostfix != "_history")
            throw new InvalidOperationException("Expected HistoryVarNamePostfix to be '_history' for regex below");

        if (GilCreationHelper.GilExpansionMarkerFuncName != "$gil")
            throw new InvalidOperationException("Expected GilExpansionMarkerFuncName to be '$gil' for regex below");

        // want to match: `$gil(this.vars.Running_history = Running_HistoryId.SETUPCHECK__START;)`
        gilHistoryVarSetRegex = new(@"(?x)
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
        // Note! For `MermaidEdgeTracker` to function correctly, below transformations must occur in the same `SmRunner`.
        // This allows us to easily map an SS behavior to its corresponding mermaid edge ID.

        // NOTE! BE VERY careful adding action code before Standard_SupportHistory. If we add a tracing action on a history default behavior,
        // then it causes HistoryProcessor to output a duplicate history enumeration value which then breaks compilation.

        const string GenMermaidCodeStepId = "gen-mermaid-code";
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_SupportHistory, new TransformationStep(id: GenMermaidCodeStepId, GenerateMermaidCode));
        
        // this step MUST run before mermaid generation for the workaround to be effective
        runner.SmTransformer.InsertBeforeFirstMatch(GenMermaidCodeStepId, MermaidStateSmithIssue512WorkAround);

        // show default 'do' events in mermaid diagram
        runner.SmTransformer.InsertBeforeFirstMatch(GenMermaidCodeStepId, (StateMachine sm) => { DefaultToDoEventVisitor.Process(sm); });

        // We want this step to run near the end so that behaviors are very close to their final form. This is good for users' understanding.
        // There's no functional difference in moving further back at this point and it is helpful to put in front of validation to ensure no issues are introduced.
        const string LoggingTransformationId = "gen-logging-transformation";
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, new TransformationStep(id: LoggingTransformationId, V1LoggingTransformationStep));

        // NOTE! Must happen before logging/tracing transformation step. We don't want to show users all the extra logging/tracing behaviors added.
        runner.SmTransformer.InsertBeforeFirstMatch(LoggingTransformationId, RecordVertexInfo);

        // collect diagram names after trigger mapping completes
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_TriggerMapping, CollectDiagramNames);
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_TriggerMapping, RecordInfoForEachState);

        // We to generate mermaid diagram before history support (to avoid a ton of transitions being shown), but AFTER name conflict resolution.
        // See https://github.com/StateSmith/StateSmith/issues/302
        // Validate that this is true.
        int historyIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_SupportHistory);
        int nameConflictIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_NameConflictResolution);
        int mermaidIndex = runner.SmTransformer.GetMatchIndex(GenMermaidCodeStepId);
        if (mermaidIndex <= nameConflictIndex || mermaidIndex >= historyIndex)
            throw new Exception("Mermaid generation must occur after name conflict resolution and before history support.");
    }

    private void CollectDiagramNames(StateMachine sm)
    {
        sm.VisitTypeRecursively((Vertex vertex) =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                foreach (var trigger in behavior.Triggers)
                {
                    if (TriggerHelper.IsEvent(trigger))
                        diagramEventNames.Add(trigger);
                }
            }
        });
    }

    /// <summary>
    /// This MUST be done before simulation tracing behaviors are added.
    /// https://github.com/StateSmith/StateSmith/issues/523
    /// </summary>
    private void RecordVertexInfo(StateMachine sm)
    {
        //todo-low: make this work for pseudo states as well?
        sm.VisitTypeRecursively((NamedVertex namedVertex) => {
            var sb = new StringBuilder();
            SmGraphDescriber smDescriber = new(new StringWriter(sb));
            smDescriber.SetOutputAncestorHandlers(true);
            smDescriber.ancestorPrefix = "=== From ancestor `";
            smDescriber.ancestorPostfix = "` ===";
            BehaviorDescriber behaviorDescriber = new(singleLineFormat: false, indent: "");
            behaviorDescriber.prependTransitionArrow = true;

            smDescriber.OutputForVertex(behaviorDescriber, namedVertex, prependSeparator: false);
            stateDescriptionMapping.Add(namedVertex.Name, sb.ToString());
        });
    }

    private void RecordInfoForEachState(StateMachine sm)
    {
        // recursively visit all named vertices (states, orthogonal states, state machines, ...)
        sm.VisitTypeRecursively((NamedVertex namedVertex) =>
        {
            if (namedVertex is StateMachine)
            {
                // Skip the state machine vertex itself. It can have events, but is more like a pseudo state.
                return;
            }

            var availableEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var availableEdgeIds =  new HashSet<int>();
            var transitionConsumedEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Collect events from this state and all its ancestors
            Vertex? currentVertex = namedVertex;
            while (currentVertex != null)
            {
                foreach (var behavior in currentVertex.Behaviors)
                {
                    foreach (var trigger in behavior.Triggers)
                    {
                        if (TriggerHelper.IsEvent(trigger))
                        {
                            availableEvents.Add(trigger);

                            if (behavior.HasTransition())
                            {
                                if (transitionConsumedEvents.Contains(trigger) == false)
                                {
                                    availableEdgeIds.Add(mermaidEdgeTracker.GetEdgeId(behavior));

                                    // any transition for an event that has no guard code is guaranteed to consume that event
                                    if (behavior.HasGuardCode() == false)
                                    {
                                        transitionConsumedEvents.Add(trigger);
                                    }
                                }
                            }
                        }
                    }
                }
                currentVertex = currentVertex.Parent;
            }

            stateToAvailableEvents[namedVertex.Name] = availableEvents;
            stateToAvailableEdgeIds[namedVertex.Name] = availableEdgeIds;
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

        // Build HTML content
        var sb = new StringBuilder();
        HtmlRenderer.Render(sb,
            smName: smName,
            mermaidCode: mermaidCodeWriter.ToString(),
            jsCode: fileCapturer.CapturedCode,
            diagramEventNamesArray: OrganizeEventNamesIntoJsArray(diagramEventNames),
            jsStateEventsMapping: MapToJson(stateToAvailableEvents),
            jsStateEdgeMapping: MapToJson(stateToAvailableEdgeIds),
            jsStateDescriptionMapping: SerializeToJson(stateDescriptionMapping)
        );
            
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

    // See https://github.com/StateSmith/StateSmith/issues/512
    void MermaidStateSmithIssue512WorkAround(StateMachine sm)
    {
        // to avoid modifying collection during enumeration, we first collect behaviors to modify
        sm.VisitRecursively((Vertex vertex) =>
        {
            if (vertex is State state)
            {
                foreach (var stateTransitionBehavior in state.TransitionBehaviors())
                {
                    // issue 512 only affects self-transitions of parent states that also have a parent shown in mermaid (not the state machine itself)
                    if (stateTransitionBehavior.TransitionTarget == state && state.Children.Any() && state.Parent != sm)
                    {
                        // Record original behavior UML before modifying.
                        // Without this, the guard message for the user to evaluate has text like:
                        //      TransitionTo(System_Run.<ChoicePoint>(Electricity_Control_Mode2__Mermaid_StateSmith_Issue_512__1)
                        // instead of:
                        //      TransitionTo(Electricity_Control_Mode2)
                        originalBehaviorTracker.RecordOriginalBehavior(stateTransitionBehavior);
                        behaviorsToModifyForIssue512.Add(stateTransitionBehavior);
                    }
                }
            }
        });

        // variables so that we can create unique names for choice states
        int choicePointId = 0;
        Behavior? previousBehavior = null;

        foreach (var behavior in behaviorsToModifyForIssue512)
        {
            var owningState = (State)behavior.OwningVertex;

            // reset choice point ID when we switch to a new owning state.
            // This keeps the names more readable and reduces git diff noise if the diagram changes.
            if (previousBehavior != null && previousBehavior.OwningVertex != owningState)
            {
                choicePointId = 0;
            }

            // add a new sibling choice point
            var choicePoint = new ChoicePoint($"{owningState.Name}__Mermaid_StateSmith_Issue_512__{choicePointId}");
            owningState.Parent.ThrowIfNull().AddChild(choicePoint);
            var newBehavior = choicePoint.AddTransitionTo(behavior.TransitionTarget.ThrowIfNull()); // can't be null as we know it's a self-transition
            behaviorsAddedForIssue512.Add(newBehavior);
            behavior._transitionTarget = choicePoint;

            previousBehavior = behavior;
            choicePointId++;
        }
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
                b.actionCode += $"this.tracer?.edgeTransition('{domId}');";
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
        // https://github.com/StateSmith/StateSmith/issues/512
        if (behaviorsAddedForIssue512.Contains(behavior))
        {
            behavior.actionCode = "this.tracer?.log(`<a href='https://github.com/StateSmith/StateSmith/issues/512' target='_blank' title='Workaround for mermaid issue tracked in StateSmith #512'>🧜‍♀️ mermaid issue #512</a>`, true);";
            return;
        }

        // Handle action code. This must happen before guard code handling as HistoryVertex also modifies action code.
        // record original behavior before we modify it.
        var originalBehaviorUml = originalBehaviorTracker.GetOriginalUmlOrCurrent(behavior);
        if (behavior.HasActionCode())
        {
            var gilHistoryVarSetMatch = gilHistoryVarSetRegex.Match(behavior.actionCode);
            
            // special case for history variable sets
            if (gilHistoryVarSetMatch.Success)
            {
                // https://github.com/StateSmith/StateSmith/issues/323
                var historyVarName = gilHistoryVarSetMatch.Groups["varName"].Value;
                var storedStateName = gilHistoryVarSetMatch.Groups["storedStateName"].Value;

                // in this case, we want original action to still execute, so we append logging code.
                behavior.actionCode += $"this.tracer?.logHistoryVarUpdate('{historyVarName}', '{storedStateName}');";
            }
            else
            {
                // we don't want to execute a user's custom action, just log it.
                behavior.actionCode = $"this.tracer?.logActionCode({FsmCodeToJsString(behavior.actionCode)});";
            }
        }

        // Handle guard code. We purposely need to treat history vertices differently.
        // We want the history vertex to work as is without prompting the user to evaluate those guards.
        if (vertex is HistoryVertex)
        {
            if (behavior.HasGuardCode())
            {
                behavior.actionCode += $"this.tracer?.logHistoryTransition('transitioning to {Vertex.Describe(behavior.TransitionTarget)}');";
            }
            else
            {
                behavior.actionCode += $"this.tracer?.logHistoryTransition('default transition');";
            }
        }
        else
        {
            // this is not a history vertex

            if (behavior.HasGuardCode())
            {
                var logCode = $"this.tracer?.logGuardCodeEvaluation(" + FsmCodeToJsString(behavior.guardCode) + ")"; // must not end with semicolon. See below.
                var confirmCode = $"this.evaluateGuard('{Vertex.Describe(behavior.OwningVertex)}', {FsmCodeToJsString(originalBehaviorUml)})";
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

        // this is needed so that simulator can call enter method when forcing a state.
        // https://github.com/StateSmith/StateSmith/issues/519
        string IRenderConfigJavaScript.PrivatePrefix => "_";
    }

    private string MapToJson(Dictionary<string, HashSet<string>> map)
    {
        // Convert to a dictionary with sorted for consistent output to minimize git diffs
        var sorted = map.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToArray()
        );

        return SerializeToJson(sorted);
    }

    private string MapToJson(Dictionary<string, HashSet<int>> map)
    {
        // Convert to a dictionary with sorted for consistent output to minimize git diffs
        var sorted = map.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderBy(e => e).ToArray()
        );

        return SerializeToJson(sorted);
    }

    private static string SerializeToJson<T>(Dictionary<string, T[]> sorted)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null // Keep original property names
        };

        return JsonSerializer.Serialize(sorted, options);
    }

    private static string SerializeToJson(Dictionary<string, string> sorted)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null // Keep original property names
        };

        return JsonSerializer.Serialize(sorted, options);
    }
}
