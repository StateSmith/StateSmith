#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph.Validation;
using StateSmith.SmGraph.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph;

public abstract class Vertex
{
    public string DiagramId { get; set; } = "";

    /// <summary>
    /// Used to track the diagram IDs of other diagram elements that make up this vertex.
    /// One example is a state's nested handler node like: https://github.com/StateSmith/StateSmith/wiki/Getting-started-using-draw.io-with-StateSmith#restrictions-on-styling-
    /// </summary>
    public List<string> DiagramSubIds { get; set; } = new();

    /// <summary>
    /// Depth is used to optimize tree searching. See https://github.com/adamfk/StateSmith/issues/3
    /// </summary>
    internal int _depth;
    public int Depth => _depth;

    internal Vertex? _parent;
    public Vertex? Parent => _parent;

    public Vertex NonNullParent
    {
        get
        {
            if (Parent == null)
            {
                throw new VertexValidationException(this, "unexpected null parent");
            }
            return Parent;
        }
    }

    public NamedVertex? ParentState => (NamedVertex?)Parent;

    /// <summary>
    /// data structure may change
    /// </summary>
    internal List<Vertex> _children = new();
    public IReadOnlyList<Vertex> Children => _children;

    /// <summary>
    /// data structure may change
    /// </summary>
    internal List<Behavior> _behaviors = new();
    public IReadOnlyList<Behavior> Behaviors => _behaviors;

    /// <summary>
    /// data structure may change
    /// </summary>
    internal List<Behavior> _incomingTransitions = new();

    public IReadOnlyList<Behavior> IncomingTransitions => _incomingTransitions;

    /// <summary>
    /// Adds a behavior to end (unless index specified).
    /// </summary>
    /// <param name="behavior"></param>
    /// <param name="index">Set to -1 to ignore index, 0 to insert behavior at start. Too large values are clamped 
    /// to be valid so you don't have to worry.</param>
    /// <returns></returns>
    public Behavior AddBehavior(Behavior behavior, int index = -1)
    {
        behavior._owningVertex = this;

        if (index >= 0)
        {
            index = Math.Min(index, _behaviors.Count);
            _behaviors.Insert(index, behavior);
        }
        else
        {
            _behaviors.Add(behavior);
        }

        return behavior;
    }

    public void RemoveBehaviorAndUnlink(Behavior behavior)
    {
        if (behavior.HasTransition())
        {
            behavior.TransitionTarget.RemoveIncomingTransition(behavior);
        }

        if (!_behaviors.Remove(behavior))
        {
            throw new VertexValidationException(this, "tried to remove behavior that wasn't owned by this vertex");
        }
    }

    public abstract void Accept(IVertexVisitor visitor);

    internal void RemoveIncomingTransition(Behavior behavior)
    {
        _incomingTransitions.RemoveOrThrow(behavior);
    }

    internal void AddIncomingTransition(Behavior behavior)
    {
        if (behavior.TransitionTarget != this)
        {
            throw new BehaviorValidationException(behavior, "Inconsistent data structure. Behavior target must match incoming target");
        }
        _incomingTransitions.Add(behavior);
    }

    public Behavior AddTransitionTo(Vertex target)
    {
        var behavior = new Behavior(owningVertex: this, transitionTarget: target);
        AddBehavior(behavior);

        return behavior;
    }

    public T AddChild<T>(T child) where T : Vertex
    {
        if (child.Parent != null)
        {
            throw new VertexValidationException(child, "Cannot add a child that already has a parent");
        }

        child._parent = this;
        RenumberSubTreeDepth(child);
        _children.Add(child);
        return child;
    }

    /// <summary>
    /// Experimental. Mostly for test code. Needs more tests before non-experimental.
    /// </summary>
    public void ChangeParent(Vertex parent)
    {
        if (!this.NonNullParent._children.Remove(this))
            throw new VertexValidationException(this, "can't remove self from parent");
        this._parent = null;
        parent.AddChild(this);
    }

    protected void RenumberSubTreeDepth(Vertex subTreeRoot)
    {
        subTreeRoot._depth = Depth + 1;

        if (subTreeRoot.Children.Count == 0)
        {
            return;
        }

        LambdaVertexWalker walker = new()
        {
            enterAction = v => v._depth = v.NonNullParent.Depth + 1
        };

        walker.Walk(subTreeRoot);
    }

    public void RemoveChild(Vertex child)
    {
        _children.RemoveOrThrow(child);
        child._parent = null;

        foreach (var childBehavior in child.Behaviors)
        {
            var target = childBehavior.TransitionTarget;
            target?._incomingTransitions.RemoveOrThrow(childBehavior);
        }

        if (child._incomingTransitions.Count > 0)
        {
            throw new VertexValidationException(child, "cannot safely remove child as it still has incoming transitions");
        }
    }

    /// <summary>
    /// Auto removes any incoming transitions and then removes child.
    /// </summary>
    public void ForceRemoveChild(Vertex child)
    {
        foreach (var t in child.IncomingTransitions.ToList()) // copy so we can modify
        {
            t.OwningVertex.RemoveBehaviorAndUnlink(t);
        }

        RemoveChild(child);
    }

    public void RemoveSelf()
    {
        NonNullParent.RemoveChild(this);
    }

    /// <summary>
    /// Auto removes any incoming transitions and then removes self.
    /// </summary>
    public void ForceRemoveSelf()
    {
        NonNullParent.ForceRemoveChild(this);
    }

    public int FindIndexInParentKids()
    {
        if (Parent == null)
        {
            return -1;
        }

        return Parent.FindChildIndex(this);
    }

    public int FindChildIndex(Vertex child)
    {
        return _children.IndexOf(child);
    }

    /// <summary>
    /// Tests whether this vertex contains vertex <paramref name="v"/>. True if this vertex is the same as <paramref name="v"/>, 
    /// or is an ancestor of <paramref name="v"/>.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public bool ContainsVertex(Vertex? v)
    {
        Vertex? current = v;

        if (current == null)
        {
            return false;
        }

        int vParentHops = current.Depth - Depth;

        if (vParentHops < 0)
        {
            return false;
        }

        for (int i = 0; i < vParentHops; i++)
        {
            current = current.Parent;
            if (current == null)
            {
                return false;
            }
        }

        return current == this;
    }

    /// <summary>
    /// Finds least common ancestor (LCA) with node <paramref name="v"/>.
    /// <c>this.FindLcaWith(this) == this.Parent</c>
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public Vertex? FindLcaWith(Vertex v)
    {
        var path = FindTransitionPathTo(v);  //todolow - implement here without accumulating Vertices
        return path.leastCommonAncestor;
    }

    internal Vertex? AscendAndCollect(List<Vertex?> list, Vertex? v, int levelCount)
    {
        Vertex? current = v;

        for (int i = 0; i < levelCount; i++)
        {
            current.ThrowIfNull();
            list.Add(current);
            current = current.Parent;
        }

        return current;
    }

    public List<NamedVertex> CollectAncestorsThatHandleEvent(string eventName)
    {
        List<NamedVertex> parents = new();

        Vertex? current = this.NonNullParent;

        while (current != null)
        {
            if (TriggerHelper.HasBehaviorsWithTrigger(current, eventName))
            {
                parents.Add((NamedVertex)current);
            }
            
            current = current.Parent;
        }

        return parents;
    }

    public NamedVertex? FirstAncestorThatHandlesEvent(string eventName)
    {
        Vertex? current = this.Parent;

        while (current != null)
        {
            if (TriggerHelper.HasBehaviorsWithTrigger(current, eventName))
            {
                return (NamedVertex)current;
            }
            
            current = current.Parent;
        }

        return null;
    }

    public TransitionPath FindTransitionPathTo(Vertex target)
    {
        //note: need to consider that there might not be a path between vertices in the case where a diagram
        // had multiple top level state machines

        if (target == this)
        {
            // self transition
            return new TransitionPath()
            {
                toExit = new List<Vertex>() { this },
                leastCommonAncestor = Parent,
                toEnter = new List<Vertex>() { this },
            };
        }

        var path = new TransitionPath();
        var fromCurrent = this;
        var toCurrent = target;

        if (fromCurrent.Depth > toCurrent.Depth)
        {
            fromCurrent = AscendAndCollect(path.toExit!, fromCurrent, levelCount: fromCurrent.Depth - toCurrent.Depth);
        }
        else if (toCurrent.Depth > fromCurrent.Depth)
        {
            toCurrent = AscendAndCollect(path.toEnter!, toCurrent, levelCount: toCurrent.Depth - fromCurrent.Depth);
        }

        //at this point from and to are at the same depth
        while (true)
        {
            if (fromCurrent == toCurrent)
            {
                path.leastCommonAncestor = fromCurrent;
                break; //will also exit if no path between nodes as both verices will be null and match
            }

            fromCurrent = AscendAndCollect(path.toExit!, fromCurrent, levelCount: 1);
            toCurrent = AscendAndCollect(path.toEnter!, toCurrent, levelCount: 1);
        }

        // vertices are entered from top down so we reverse the list
        path.toEnter.Reverse();

        return path;
    }

    public static string Describe(Vertex? v)
    {
        if (v == null)
        {
            return "<null>";
        }

        return ShortDescribingVisitor.Describe(v);
    }
}
