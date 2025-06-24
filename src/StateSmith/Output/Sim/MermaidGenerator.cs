using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace StateSmith.Output.Sim;

/// <summary>
/// Mermaid图表生成器
/// 实现IVertexVisitor接口，用于生成Mermaid格式的状态图代码
/// </summary>
class MermaidGenerator : IVertexVisitor
{
    /// <summary>
    /// 当前缩进级别
    /// </summary>
    int indentLevel = 0;
    
    /// <summary>
    /// 用于构建Mermaid代码的字符串构建器
    /// </summary>
    StringBuilder sb = new();
    
    /// <summary>
    /// Mermaid边缘跟踪器，用于跟踪边缘ID
    /// </summary>
    MermaidEdgeTracker mermaidEdgeTracker;
    
    /// <summary>
    /// 行为描述器，用于生成行为的文本描述
    /// </summary>
    BehaviorDescriber behaviorDescriber;

    /// <summary>
    /// 换行符令牌
    /// 我们将此作为行为描述中换行符的替换，然后转义mermaid特殊字符，
    /// 最后在最终输出中将此令牌替换为换行符。
    /// 如果没有这个处理，用户动作代码中的换行符会在mermaid输出中变成`#92;n`而不是`\n`。
    /// </summary>
    const string LINE_BREAK_TOKEN = "__LINE_BREAK__";

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="edgeOrderTracker">边缘顺序跟踪器</param>
    public MermaidGenerator(MermaidEdgeTracker edgeOrderTracker)
    {
        this.mermaidEdgeTracker = edgeOrderTracker;
        // 初始化行为描述器，使用单行格式，换行符使用自定义令牌
        behaviorDescriber = new(singleLineFormat: true, newLine: LINE_BREAK_TOKEN);
        behaviorDescriber.describeTransition = false; // 我们不希望在mermaid标签中打印`TransitionTo(state)`
    }

    /// <summary>
    /// 渲染整个状态机
    /// </summary>
    /// <param name="sm">要渲染的状态机</param>
    public void RenderAll(StateMachine sm)
    {
        sm.Accept(this); // 访问状态机节点
        RenderEdges(sm); // 渲染边缘
    }

    /// <summary>
    /// 获取生成的Mermaid代码
    /// </summary>
    /// <returns>Mermaid代码字符串</returns>
    public string GetMermaidCode()
    {
        return sb.ToString();
    }

    /// <summary>
    /// 访问状态机节点
    /// </summary>
    /// <param name="v">状态机对象</param>
    public void Visit(StateMachine v)
    {
        AppendIndentedLine("stateDiagram");
        indentLevel--; // 不缩进状态机内容
        VisitChildren(v);
    }

    /// <summary>
    /// 访问状态节点
    /// </summary>
    /// <param name="v">状态对象</param>
    public void Visit(State v)
    {
        if (v.Children.Count <= 0)
        {
            VisitLeafState(v); // 访问叶子状态
        }
        else
        {
            VisitCompoundState(v); // 访问复合状态
        }
    }

    /// <summary>
    /// 访问复合状态（包含子状态的状态）
    /// </summary>
    /// <param name="v">复合状态对象</param>
    private void VisitCompoundState(State v)
    {
        AppendIndentedLine($"state {v.Name} {{");
        // FIXME - 当mermaid支持时在此处添加行为代码
        // https://github.com/StateSmith/StateSmith/issues/268#issuecomment-2111432194
        VisitChildren(v);
        AppendIndentedLine("}");
    }

    /// <summary>
    /// 访问叶子状态（没有子状态的状态）
    /// </summary>
    /// <param name="v">叶子状态对象</param>
    private void VisitLeafState(State v)
    {
        string name = v.Name;
        AppendIndentedLine(name);
        AppendIndentedLine($"{name} : {name}");
        
        // 为每个非转换行为添加描述
        foreach (var b in v.NonTransitionBehaviors())
        {
            // 为了 https://github.com/StateSmith/StateSmith/issues/355 始终显示动作代码
            string behaviorText = BehaviorToMermaidLabel(b, alwaysShowActionCode: true);
            AppendIndentedLine($"{name} : {behaviorText}");
        }
    }

    /// <summary>
    /// 访问初始状态节点
    /// </summary>
    /// <param name="initialState">初始状态对象</param>
    public void Visit(InitialState initialState)
    {
        string initialStateId = MakeVertexDiagramId(initialState);

        bool showInitialStateDot = false;
        // 暂时禁用，因为我们需要更改边缘高亮显示
        // @emmby: 我们是否可以对`$initial_state`进行样式设置，使其看起来像初始状态的黑色填充圆圈？你的想法？

        if (showInitialStateDot)
        {
            // Mermaid和PlantUML没有允许转换到初始状态的语法。
            // 如果你写`someState --> [*]`，这意味着转换到最终状态。
            // 然而，StateSmith确实允许转换到初始状态，所以我们添加一个虚拟状态来表示初始状态。
            AppendIndentedLine($"[*] --> {initialStateId}");
            mermaidEdgeTracker.AdvanceId();  // 我们暂时跳过这个"变通方法"边缘。我们稍后可以改进这一点。
        }

        AppendIndentedLine($"%% Initial state name as \".\" so that it fits in black circle shape.", extraLine: false);
        AppendIndentedLine($"%% See https://github.com/StateSmith/StateSmith/issues/404", extraLine: false);
        AppendIndentedLine($"state \".\" as {initialStateId}");
    }

    /// <summary>
    /// 访问选择点节点
    /// </summary>
    /// <param name="v">选择点对象</param>
    public void Visit(ChoicePoint v)
    {
        AppendIndentedLine($"state {MakeVertexDiagramId(v)} <<choice>>");
    }

    /// <summary>
    /// 访问入口点节点
    /// </summary>
    /// <param name="v">入口点对象</param>
    public void Visit(EntryPoint v)
    {
        AppendIndentedLine($"state \"$entry_pt.{v.label}\" as {MakeVertexDiagramId(v)}");
    }

    /// <summary>
    /// 访问出口点节点
    /// </summary>
    /// <param name="v">出口点对象</param>
    public void Visit(ExitPoint v)
    {
        AppendIndentedLine($"state \"$exit_pt.{v.label}\" as {MakeVertexDiagramId(v)}");
    }

    /// <summary>
    /// 访问历史顶点节点
    /// </summary>
    /// <param name="v">历史顶点对象</param>
    public void Visit(HistoryVertex v)
    {
        AppendIndentedLine($"state \"$H\" as {MakeVertexDiagramId(v)}");
    }

    /// <summary>
    /// 访问历史继续顶点节点
    /// </summary>
    /// <param name="v">历史继续顶点对象</param>
    public void Visit(HistoryContinueVertex v)
    {
        AppendIndentedLine($"state \"$HC\" as {MakeVertexDiagramId(v)}");
    }

    /// <summary>
    /// 渲染所有边缘（状态转换）
    /// </summary>
    /// <param name="sm">状态机对象</param>
    public void RenderEdges(StateMachine sm)
    {
        sm.VisitRecursively((Vertex v) =>
        {
            string vertexDiagramId = MakeVertexDiagramId(v);

            foreach (var behavior in v.Behaviors)
            {
                // 处理转换行为
                if (behavior.TransitionTarget != null)
                {
                    Append($"{vertexDiagramId} --> {MakeVertexDiagramId(behavior.TransitionTarget)}");

                    // 只有当行为文本不为空时才添加边缘标签，以避免Mermaid解析错误
                    string behaviorText = BehaviorToMermaidLabel(behavior);
                    if (!string.IsNullOrWhiteSpace(behaviorText))
                    {
                        Append($" : {behaviorText}");
                    }
                    AppendLine();

                    mermaidEdgeTracker.AddEdge(behavior); // 添加边缘到跟踪器
                }
            }
        });
    }

    /// <summary>
    /// 将行为转换为Mermaid标签格式
    /// </summary>
    /// <param name="behavior">行为对象</param>
    /// <param name="alwaysShowActionCode">是否始终显示动作代码</param>
    /// <returns>格式化的Mermaid标签文本</returns>
    public string BehaviorToMermaidLabel(Behavior behavior, bool alwaysShowActionCode = false)
    {
        var behaviorText = behaviorDescriber.Describe(behavior, alwaysShowActionCode: alwaysShowActionCode);
        behaviorText = MermaidEscape(behaviorText); // 转义Mermaid特殊字符
        behaviorText = behaviorText.Replace(LINE_BREAK_TOKEN, "\\n"); // 替换换行符令牌
        return behaviorText;
    }

    /// <summary>
    /// 为顶点创建图表ID
    /// </summary>
    /// <param name="v">顶点对象</param>
    /// <returns>顶点的图表ID</returns>
    public static string MakeVertexDiagramId(Vertex v)
    {
        switch (v)
        {
            case NamedVertex namedVertex:
                return namedVertex.Name;
            default:
                // 参见 https://github.com/StateSmith/StateSmith/blob/04955e5df7d5eb6654a048dccb35d6402751e4c6/src/StateSmithTest/VertexDescribeTests.cs
                return Vertex.Describe(v).Replace("<", "(").Replace(">", ")");
        }
    }

    /// <summary>
    /// 转义Mermaid特殊字符
    /// TODO 处理 # 字符
    /// 不能简单地将#添加到字符列表中，因为#和;会相互干扰
    /// </summary>
    /// <param name="text">要转义的文本</param>
    /// <returns>转义后的文本</returns>
    public static string MermaidEscape(string text)
    {
        foreach (char c in ":;\\{}".ToCharArray())
        {
            text = text.Replace(c.ToString(), $"#{(int)c};");
        }
        return text;
    }

    /// <summary>
    /// 追加文本到字符串构建器
    /// </summary>
    /// <param name="message">要追加的消息</param>
    private void Append(string message)
    {
        sb.Append(message);
    }

    /// <summary>
    /// 追加一行文本到字符串构建器
    /// </summary>
    /// <param name="message">要追加的消息</param>
    /// <param name="extraLine">是否添加额外的空行</param>
    private void AppendLine(string message = "", bool extraLine = true)
    {
        sb.AppendLine(message);

        if (extraLine)
        {
            // 添加额外的空行，以便git diff在单独的行上工作，而不是巨大的文本块
            sb.AppendLine();
        }
    }

    /// <summary>
    /// 追加带缩进的行到字符串构建器
    /// </summary>
    /// <param name="message">要追加的消息</param>
    /// <param name="extraLine">是否添加额外的空行</param>
    private void AppendIndentedLine(string message, bool extraLine = true)
    {
        // 添加缩进
        for (int i = 0; i < indentLevel; i++)
            sb.Append("        ");

        AppendLine(message, extraLine);
    }

    /// <summary>
    /// 访问顶点的子节点
    /// </summary>
    /// <param name="v">父顶点</param>
    private void VisitChildren(Vertex v)
    {
        indentLevel++; // 增加缩进级别
        foreach (var child in v.Children)
        {
            child.Accept(this); // 访问每个子节点
        }
        indentLevel--; // 减少缩进级别
    }

    /// <summary>
    /// 访问渲染配置顶点（忽略处理）
    /// </summary>
    /// <param name="v">渲染配置顶点</param>
    public void Visit(RenderConfigVertex v)
    {
        // 忽略渲染配置及其子节点
    }

    /// <summary>
    /// 访问配置选项顶点（忽略处理）
    /// </summary>
    /// <param name="v">配置选项顶点</param>
    public void Visit(ConfigOptionVertex v)
    {
        // 忽略配置选项及其子节点
    }

    /// <summary>
    /// 访问正交状态（尚未实现）
    /// </summary>
    /// <param name="v">正交状态对象</param>
    public void Visit(OrthoState v)
    {
        // 正交状态尚未实现，但将来会实现
        throw new NotImplementedException();
    }

    /// <summary>
    /// 访问注释顶点（忽略处理）
    /// </summary>
    /// <param name="v">注释顶点</param>
    public void Visit(NotesVertex v)
    {
        // 忽略注释及其子节点
    }

    /// <summary>
    /// 访问命名顶点（不应被调用）
    /// </summary>
    /// <param name="v">命名顶点</param>
    public void Visit(NamedVertex v)
    {
        throw new NotImplementedException(); // 不应被调用
    }

    /// <summary>
    /// 访问通用顶点（不应被调用）
    /// </summary>
    /// <param name="v">顶点对象</param>
    public void Visit(Vertex v)
    {
        throw new NotImplementedException(); // 不应被调用
    }
}
