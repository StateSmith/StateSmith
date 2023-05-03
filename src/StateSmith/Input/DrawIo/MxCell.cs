#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reflection.Emit;

namespace StateSmith.Input.DrawIo;

public class MxCell
{
    public enum Type { None, Edge, Vertex, VertexImage }

    public string id;
    public string? parent;
    public string? label;
    public Type type = Type.None;
    public string? source;
    public string? target;
    public string? styleString;
    public bool isCollapsed = false;

    /// <summary>Only available for vertices right now. If collapsed, this the collapsed bounds. If expanded, this is the expanded bounds.</summary>
    public MxBounds? primaryBounds;

    /// <summary>Only available for vertices right now. </summary>
    public MxBounds? alternateBounds;

    private Dictionary<string, string> styleMap = new();

    public IReadOnlyDictionary<string, string> StyleMap => styleMap;

    /// <summary>
    /// These special shapes don't need to have a shape style like `shape=ellipse`. They can also just have `ellipse`.
    /// </summary>
    public static readonly string[] specialShapes = {
            "rectangle",
            "ellipse",
            "doubleEllipse",
            "rhombus",
            "line",
            "image",
            "arrow",
            "arrowConnector",
            "label",
            "cylinder",
            "swimlane",
            "connector",
            "actor",
            "cloud",
            "triangle",
            "hexagon",
        };

    public MxCell(string id)
    {
        this.id = id;
    }

    public bool HasMatchingStyle(string key, string value)
    {
        if (StyleMap.TryGetValue(key, out var readValue))
        {
            return value == readValue;
        }

        return false;
    }

    public void SetStyle(string key, string value)
    { 
        styleMap[key] = value;
    }

    public string? GetStyleFor(string key)
    {
        StyleMap.TryGetValue(key, out var readValue);
        return readValue;
    }

    public bool HasShape()
    {
        foreach (var specialShape in specialShapes)
        {
            if (StyleMap.ContainsKey(specialShape))
                return true;
        }

        var shape = GetStyleFor("shape");
        if (shape != null)
        {
            if (shape != "" && shape != "none")
                return true;
        }

        return false;
    }
}
