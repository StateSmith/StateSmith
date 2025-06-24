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
using System.Text.RegularExpressions;
using System.Linq;

namespace StateSmith.Output.Sim;

/// <summary>
/// 模拟器Web生成器
/// 负责生成状态机的Web模拟器页面，包括HTML、JavaScript和Mermaid图表
/// </summary>
public class SimWebGenerator
{
    /// <summary>
    /// 获取运行器设置
    /// </summary>
    public RunnerSettings RunnerSettings => runner.Settings;

    /// <summary>
    /// 代码文件写入器
    /// </summary>
    private readonly ICodeFileWriter codeFileWriter;
    
    /// <summary>
    /// Mermaid边缘跟踪器，用于跟踪图表中的边缘
    /// </summary>
    MermaidEdgeTracker mermaidEdgeTracker = new();
    
    /// <summary>
    /// 跟踪扩展器，用于跟踪代码扩展
    /// </summary>
    TrackingExpander trackingExpander = new();
    
    /// <summary>
    /// Mermaid代码写入器
    /// </summary>
    TextWriter mermaidCodeWriter = new StringWriter();
    
    /// <summary>
    /// 模拟代码写入器
    /// </summary>
    TextWriter mocksWriter = new StringWriter();
    
    /// <summary>
    /// 单文件捕获器，用于捕获生成的代码
    /// </summary>
    SingleFileCapturer fileCapturer = new();
    
    /// <summary>
    /// 状态机提供器
    /// </summary>
    StateMachineProvider stateMachineProvider;
    
    /// <summary>
    /// 名称修饰器，用于处理命名冲突
    /// </summary>
    NameMangler nameMangler;
    
    /// <summary>
    /// 历史GIL正则表达式，用于匹配历史相关的GIL代码
    /// </summary>
    Regex historyGilRegex;

    /// <summary>
    /// 图表事件名称集合
    /// 我们希望在模拟器中向用户显示他们的原始事件名称，而不是经过清理的名称
    /// </summary>
    HashSet<string> diagramEventNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 状态到可用事件的映射
    /// 键是状态名称，值是该状态可以处理的事件名称集合
    /// </summary>
    Dictionary<string, HashSet<string>> stateToAvailableEvents = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 行为跟踪器，用于跟踪行为的原始表示
    /// </summary>
    BehaviorTracker behaviorTracker = new();

    /// <summary>
    /// 状态机运行器
    /// </summary>
    SmRunner runner;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="codeFileWriter">代码文件写入器</param>
    /// <param name="mainRunnerSettings">主运行器设置</param>
    public SimWebGenerator(ICodeFileWriter codeFileWriter, RunnerSettings mainRunnerSettings)
    {
        // 注意！我们需要mainRunnerSettings，这样我们就可以使用与主运行器相同的算法。
        // 这需要在构造期间发生，因为依赖注入的缘故。

        // 在内部，`SimWebGenerator`使用`SmRunner`将输入图表转换为模拟网页。
        // 为了自定义转换/代码生成过程，我们向`SmRunner`注册自定义DI服务。

        this.codeFileWriter = codeFileWriter;
        DiServiceProvider simDiServiceProvider;

        var enablePreDiagramBasedSettings = false;  // 需要阻止它尝试过早读取图表，因为使用了假的图表路径
        runner = new(diagramPath: "placeholder-updated-in-generate-method.txt", renderConfig: new SimRenderConfig(), transpilerId: TranspilerId.JavaScript, algorithmId: mainRunnerSettings.algorithmId, enablePDBS: enablePreDiagramBasedSettings);
        runner.Settings.propagateExceptions = true;

        // 注册DI服务必须在访问`runner.SmTransformer`之前完成。
        simDiServiceProvider = runner.GetExperimentalAccess().DiServiceProvider;
        simDiServiceProvider.AddSingletonT<IExpander>(trackingExpander);
        simDiServiceProvider.AddSingletonT<ICodeFileWriter>(fileCapturer);
        simDiServiceProvider.AddSingletonT<IConsolePrinter>(new DiscardingConsolePrinter());   // 我们希望常规SmRunner控制台输出被丢弃
        AdjustTransformationPipeline(); // 调整转换管道
        PreventCertainDiagramSpecifiedSettings(simDiServiceProvider.GetInstanceOf<RenderConfigBaseVars>());

        stateMachineProvider = simDiServiceProvider.GetInstanceOf<StateMachineProvider>();

        nameMangler = simDiServiceProvider.GetInstanceOf<NameMangler>();

        SetupGilHistoryRegex(); // 设置GIL历史正则表达式
    }

    /// <summary>
    /// 防止用户图表设置干扰生成的模拟
    /// 阻止可能破坏生成模拟的用户图表设置
    /// https://github.com/StateSmith/StateSmith/issues/337
    /// </summary>
    /// <param name="renderConfigBaseVars">渲染配置基础变量</param>
    private void PreventCertainDiagramSpecifiedSettings(RenderConfigBaseVars renderConfigBaseVars)
    {
        DiagramBasedSettingsPreventer.Process(runner.SmTransformer, action: (readRenderConfigAllVars, _) =>
        {
            // 只复制对模拟安全的设置
            renderConfigBaseVars.TriggerMap = readRenderConfigAllVars.Base.TriggerMap;
        });
    }

    /// <summary>
    /// 设置GIL历史正则表达式
    /// GIL是通用中间语言（Generic Intermediary Language）。它被历史顶点和其他特殊情况使用。
    /// </summary>
    /// <exception cref="InvalidOperationException">当预期的配置不匹配时抛出异常</exception>
    [MemberNotNull(nameof(historyGilRegex))]
    private void SetupGilHistoryRegex()
    {
        if (nameMangler.HistoryVarEnumTypePostfix != "_HistoryId")
            throw new InvalidOperationException("Expected HistoryVarEnumTypePostfix to be '_HistoryId' for regex below");

        if (nameMangler.HistoryVarNamePostfix != "_history")
            throw new InvalidOperationException("Expected HistoryVarNamePostfix to be '_history' for regex below");

        if (GilCreationHelper.GilExpansionMarkerFuncName != "$gil")
            throw new InvalidOperationException("Expected GilExpansionMarkerFuncName to be '$gil' for regex below");

        // 希望匹配：`$gil(this.vars.Running_history = Running_HistoryId.SETUPCHECK__START;)`
        historyGilRegex = new(@"(?x)
        \$gil\(
            \s*
            this\.vars\.
            (?<varName>\w+)_history         # 例如：Running_history
            \s* = \s*
            \w+ [.] (?<storedStateName>\w+);   # 例如：Running_HistoryId.SETUPCHECK__START
        \)
    ");
    }

    /// <summary>
    /// 调整转换管道
    /// 配置状态机转换过程中的各个步骤顺序和处理逻辑
    /// </summary>
    private void AdjustTransformationPipeline()
    {
        // 注意！为了让`MermaidEdgeTracker`正确工作，下面的两个转换必须在同一个`SmRunner`中发生。
        // 这允许我们轻松地将SS行为映射到其对应的mermaid边缘ID。

        const string GenMermaidCodeStepId = "gen-mermaid-code";
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_SupportHistory, new TransformationStep(id: GenMermaidCodeStepId, GenerateMermaidCode));
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, V1LoggingTransformationStep);
        
        // 在触发器映射完成后收集图表名称
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_TriggerMapping, CollectDiagramNames);

        // 我们需要在历史支持之前生成mermaid图表（以避免显示大量转换），但要在名称冲突解决之后。
        // 参见 https://github.com/StateSmith/StateSmith/issues/302
        // 验证这是正确的。
        int historyIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_SupportHistory);
        int nameConflictIndex = runner.SmTransformer.GetMatchIndex(StandardSmTransformer.TransformationId.Standard_NameConflictResolution);
        int mermaidIndex = runner.SmTransformer.GetMatchIndex(GenMermaidCodeStepId);
        if (mermaidIndex <= nameConflictIndex || mermaidIndex >= historyIndex)
            throw new Exception("Mermaid generation must occur after name conflict resolution and before history support.");

        // 在mermaid图表中显示默认的'do'事件
         runner.SmTransformer.InsertBeforeFirstMatch(GenMermaidCodeStepId, (StateMachine sm) => { DefaultToDoEventVisitor.Process(sm); });
    }

    /// <summary>
    /// 收集图表名称
    /// 遍历状态机收集所有事件触发器的名称
    /// </summary>
    /// <param name="sm">状态机对象</param>
    private void CollectDiagramNames(StateMachine sm)
    {
        sm.VisitRecursively((Vertex vertex) =>
        {
            // 收集所有事件名称
            foreach (var behavior in vertex.Behaviors)
            {
                foreach (var trigger in behavior.Triggers)
                {
                    if (TriggerHelper.IsEvent(trigger))
                        diagramEventNames.Add(trigger);
                }
            }

            // 收集每个状态的可用事件
            if (vertex is State state)
            {
                var availableEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                // 收集直接在该状态定义的事件
                foreach (var behavior in state.Behaviors)
                {
                    foreach (var trigger in behavior.Triggers)
                    {
                        if (TriggerHelper.IsEvent(trigger))
                        {
                            availableEvents.Add(trigger);
                        }
                    }
                }

                // 收集从父状态继承的事件
                var currentVertex = state.Parent;
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

    /// <summary>
    /// 生成模拟器HTML文件
    /// </summary>
    /// <param name="diagramPath">图表文件路径</param>
    /// <param name="outputDir">输出目录</param>
    public void Generate(string diagramPath, string outputDir)
    {
        runner.Settings.DiagramPath = diagramPath;
        runner.Run(); // 运行状态机转换
        var smName = stateMachineProvider.GetStateMachine().Name;

        // 确保输出目录存在
        if (Directory.Exists(outputDir) == false)
            Directory.CreateDirectory(outputDir);

        string path = Path.Combine(outputDir, $"{smName}.sim.html");

        // 将事件名称组织为JavaScript数组格式
        string diagramEventNamesArray = OrganizeEventNamesIntoJsArray(diagramEventNames);

        // 将状态事件映射组织为JavaScript对象格式
        string stateEventsMapping = OrganizeStateEventsIntoJsObject();

        // 构建HTML内容
        var sb = new StringBuilder();
        HtmlRenderer.Render(sb,
            smName: smName,
            mocksCode: mocksWriter.ToString(),
            mermaidCode: mermaidCodeWriter.ToString(),
            jsCode: fileCapturer.CapturedCode,
            diagramEventNamesArray: diagramEventNamesArray,
            stateEventsMapping: stateEventsMapping);
            
        // 写入HTML文件
        codeFileWriter.WriteFile(path, code: sb.ToString());
    }

    /// <summary>
    /// 将事件名称组织为JavaScript数组格式
    /// </summary>
    /// <param name="unOrderedEventNames">无序的事件名称集合</param>
    /// <returns>JavaScript数组格式的字符串</returns>
    private static string OrganizeEventNamesIntoJsArray(HashSet<string> unOrderedEventNames)
    {
        string? doEvent = null;
        List<string> eventNames = new();

        // 分离do事件和其他事件
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

        // 对非do事件进行排序
        eventNames.Sort(StringComparer.OrdinalIgnoreCase);  // 不区分大小写的排序

        // 将do事件放在第一位
        if (doEvent != null)
        {
            eventNames.Insert(0, doEvent);
        }

        // 构建JavaScript数组字符串
        var diagramEventNamesArray = "[";
        foreach (var name in eventNames)
        {
            diagramEventNamesArray += $"'{name}', ";
        }
        diagramEventNamesArray += "]";
        return diagramEventNamesArray;
    }

    /// <summary>
    /// 生成Mermaid代码
    /// </summary>
    /// <param name="sm">状态机对象</param>
    void GenerateMermaidCode(StateMachine sm)
    {
        var visitor = new MermaidGenerator(mermaidEdgeTracker);
        visitor.RenderAll(sm);
        mermaidCodeWriter.WriteLine(visitor.GetMermaidCode());
    }


    /// <summary>
    /// V1日志转换步骤
    /// 为模拟添加日志记录和跟踪功能
    /// </summary>
    /// <param name="sm">状态机对象</param>
    void V1LoggingTransformationStep(StateMachine sm)
    {
        sm.VisitRecursively((Vertex vertex) =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                behaviorTracker.RecordOriginalBehavior(behavior); // 记录原始行为
                V1ModBehaviorsForSimulation(vertex, behavior); // 修改行为以适应模拟
            }

            V1AddEntryExitTracing(sm, vertex); // 添加进入/退出跟踪
            V1AddEdgeTracing(vertex); // 添加边缘跟踪
        });
    }

    /// <summary>
    /// 添加边缘跟踪
    /// 为转换行为添加边缘转换跟踪代码
    /// </summary>
    /// <param name="vertex">顶点对象</param>
    void V1AddEdgeTracing(Vertex vertex)
    {
        foreach (var b in vertex.TransitionBehaviors())
        {
            if (mermaidEdgeTracker.ContainsEdge(b))
            {
                // 注意：大多数历史行为不会在mermaid图表中显示
                var domId = "edge" + mermaidEdgeTracker.GetEdgeId(b);
                // 注意！在修复bug之前避免在ss守卫/动作代码中使用单引号：https://github.com/StateSmith/StateSmith/issues/282
                b.actionCode += $"this.tracer?.edgeTransition(\"{domId}\");";
            }
        }
    }

    /// <summary>
    /// 添加进入/退出跟踪
    /// 为状态添加进入和退出时的跟踪代码
    /// </summary>
    /// <param name="sm">状态机对象</param>
    /// <param name="vertex">顶点对象</param>
    void V1AddEntryExitTracing(StateMachine sm, Vertex vertex)
    {
        // 我们故意不想跟踪状态机本身的进入/退出。
        // 这就是为什么我们使用`State`而不是`NamedVertex`。
        if (vertex is State state)
        {
            var mermaidName = state.Name;
            state.AddEnterAction($"this.tracer?.enterState('{mermaidName}');", index: 0);
            state.AddExitAction($"this.tracer?.exitState('{mermaidName}');");
        }
    }

    /// <summary>
    /// 修改行为以适应模拟
    /// 将原始行为代码转换为适合模拟器的格式
    /// </summary>
    /// <param name="vertex">顶点对象</param>
    /// <param name="behavior">行为对象</param>
    void V1ModBehaviorsForSimulation(Vertex vertex, Behavior behavior)
    {
        if (behavior.HasActionCode())
        {
            var historyGilMatch = historyGilRegex.Match(behavior.actionCode);
            
            if (historyGilMatch.Success)
            {
                // TODO https://github.com/StateSmith/StateSmith/issues/323
                // 显示历史变量更新
                // var historyVarName = historyGilMatch.Groups["varName"].Value;
                // var storedStateName = historyGilMatch.Groups["storedStateName"].Value;
                // behavior.actionCode += $"this.tracer?.log('📝 History({historyVarName}) = {storedStateName}');";
            }
            else
            {
                // 我们不想执行动作，只是记录它。
                behavior.actionCode = $"this.tracer?.log(\"⚡ FSM would execute action: \" + {FsmCodeToJsString(behavior.actionCode)});";
            }
        }

        if (vertex is HistoryVertex)
        {
            if (behavior.HasGuardCode())
            {
                // 我们希望历史顶点按原样工作，而不提示用户评估这些守卫。
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
                // 注意！logCode不返回值，所以确认代码总是会被评估。
            }
        }
    }

    /// <summary>
    /// 将FSM代码转换为JavaScript字符串
    /// 处理换行符和引号转义
    /// </summary>
    /// <param name="code">要转换的代码</param>
    /// <returns>JavaScript字符串格式的代码</returns>
    static string FsmCodeToJsString(string code)
    {
        code = code.ReplaceLineEndings("\\n");  // 需要为跨多行的fsm代码转义换行符
        return "\"" + code.Replace("\"", "\\\"") + "\"";
    }

    /// <summary>
    /// 模拟器渲染配置
    /// 为JavaScript代码生成提供特定于模拟器的配置
    /// </summary>
    public class SimRenderConfig : IRenderConfigJavaScript
    {
        /// <summary>
        /// 生成的JavaScript类代码
        /// 包含用于守卫评估的回调函数
        /// </summary>
        string IRenderConfigJavaScript.ClassCode => @"        
        // 默认为null。
        // 可以被重写以覆盖守卫评估（例如在模拟器中）
        evaluateGuard = null;
    ";
    }

    /// <summary>
    /// 将状态到可用事件的映射转换为JavaScript对象格式
    /// </summary>
    /// <returns>JavaScript对象格式的字符串</returns>
    private string OrganizeStateEventsIntoJsObject()
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        
        foreach (var kvp in stateToAvailableEvents)
        {
            var stateName = kvp.Key;
            var events = kvp.Value;
            
            sb.Append($"    '{stateName}': [");
            foreach (var eventName in events.OrderBy(e => e, StringComparer.OrdinalIgnoreCase))
            {
                sb.Append($"'{eventName}', ");
            }
            sb.AppendLine("],");
        }
        
        sb.AppendLine("}");
        return sb.ToString();
    }
}
