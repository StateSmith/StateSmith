#nullable enable

using StateSmith.SmGraph;
using System;
using System.Collections.Generic;

namespace StateSmith.Output.Sim;

/// <summary>
/// 行为跟踪器类
/// 此类用于跟踪行为在被修改以用于模拟器之前的原始UML表示
/// https://github.com/StateSmith/StateSmith/issues/381
/// </summary>
public class BehaviorTracker
{
    /// <summary>
    /// 存储行为到其原始UML表示的映射关系
    /// </summary>
    Dictionary<Behavior, string> mappingToOriginalUml = new();

    /// <summary>
    /// 记录行为的原始UML表示
    /// </summary>
    /// <param name="behavior">要记录的行为对象</param>
    /// <exception cref="InvalidOperationException">当行为已经被添加时抛出异常</exception>
    public void RecordOriginalBehavior(Behavior behavior)
    {
        if (mappingToOriginalUml.ContainsKey(behavior))
        {
            throw new InvalidOperationException("Behavior already added");
        }
        // 将行为的原始UML描述存储到映射字典中
        mappingToOriginalUml[behavior] = behavior.DescribeAsUml(singleLineFormat:false);
    }

    /// <summary>
    /// 尝试获取行为的原始UML表示。如果找不到，则返回当前的UML表示
    /// </summary>
    /// <param name="behavior">要获取UML表示的行为对象</param>
    /// <returns>行为的原始UML表示或当前UML表示</returns>
    /// <exception cref="InvalidOperationException">操作无效时抛出异常</exception>
    public string GetOriginalUmlOrCurrent(Behavior behavior)
    {
        // 尝试从映射字典中获取原始UML表示
        if (mappingToOriginalUml.TryGetValue(behavior, out string? originalUml))
        {
            return originalUml;
        }
        // 如果没有找到原始表示，返回当前的UML表示
        return behavior.DescribeAsUml(singleLineFormat: false);
    }
}
