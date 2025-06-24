using StateSmith.SmGraph;
using System.Collections.Generic;

namespace StateSmith.Output.Sim;

/// <summary>
/// Mermaid边缘跟踪器
/// 此类将行为映射到其对应的mermaid边缘ID
/// </summary>
public class MermaidEdgeTracker
{
    /// <summary>
    /// 存储行为到边缘ID的映射关系
    /// </summary>
    Dictionary<Behavior, int> edgeIdMap = new();
    
    /// <summary>
    /// 下一个可用的边缘ID
    /// </summary>
    int nextId = 0;

    /// <summary>
    /// 添加一个行为边缘并分配一个唯一的ID
    /// </summary>
    /// <param name="b">要添加的行为对象</param>
    /// <returns>分配给该行为的边缘ID</returns>
    public int AddEdge(Behavior b)
    {
        int id = nextId;
        AdvanceId(); // 递增ID计数器
        edgeIdMap.Add(b, id); // 将行为和ID添加到映射中
        return id;
    }

    /// <summary>
    /// 递增ID计数器（用于添加非行为边缘时）
    /// </summary>
    /// <returns>当前ID值（递增前的值）</returns>
    public int AdvanceId()
    {
        return nextId++;
    }

    /// <summary>
    /// 检查指定行为是否已存在边缘映射
    /// </summary>
    /// <param name="b">要检查的行为对象</param>
    /// <returns>如果存在映射则返回true，否则返回false</returns>
    public bool ContainsEdge(Behavior b)
    {
        return edgeIdMap.ContainsKey(b);
    }

    /// <summary>
    /// 获取指定行为的边缘ID
    /// </summary>
    /// <param name="b">要获取ID的行为对象</param>
    /// <returns>该行为对应的边缘ID</returns>
    public int GetEdgeId(Behavior b)
    {
        return edgeIdMap[b];
    }
}
