using StateSmith.Common;
using StateSmith.compiler;
using StateSmith.compiler.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable enable

namespace StateSmith.Compiling
{
    public abstract class Vertex
    {
        public string DiagramId { get; set; } = "";

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
        internal List<Vertex> _children = new List<Vertex>();
        public IReadOnlyList<Vertex> Children => _children;

        /// <summary>
        /// data structure may change
        /// </summary>
        internal List<Behavior> _behaviors = new List<Behavior>();
        public IReadOnlyList<Behavior> Behaviors => _behaviors;

        /// <summary>
        /// data structure may change
        /// </summary>
        internal HashList<string, NamedVertex> _namedDescendants = new();

        /// <summary>
        /// data structure may change
        /// </summary>
        internal List<Behavior> _incomingTransitions = new List<Behavior>();

        protected Vertex()
        {
            ResetNamedDescendantsMap();
        }

        public IReadOnlyList<Behavior> IncomingTransitions => _incomingTransitions;

        public void AddBehavior(Behavior behavior, int index = -1)
        {
            behavior._owningVertex = this;

            if (index >= 0)
            {
                _behaviors.Insert(index, behavior);
            }
            else
            {
                _behaviors.Add(behavior);
            }
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

        public abstract void Accept(VertexVisitor visitor);

        internal void ResetNamedDescendantsMap()
        {
            _namedDescendants = new HashList<string, NamedVertex>();
        }

        public List<NamedVertex> DescendantsWithName(string name)
        {
            List<NamedVertex> list = new List<NamedVertex>();
            var matches = _namedDescendants.GetValuesOrEmpty(name);
            list.AddRange(matches);

            return list;
        }


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

        /// <summary>
        /// NOTE! Must manually update descendants after calling.
        /// </summary>
        /// <param name="child"></param>
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

        /// <summary>
        /// NOTE! Must manually update descendants after calling.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(Vertex child)
        {
            _children.RemoveOrThrow(child);
            child._parent = null;

            foreach (var childBehavior in child.Behaviors)
            {
                var target = childBehavior.TransitionTarget;
                if (target != null)
                {
                    target._incomingTransitions.RemoveOrThrow(childBehavior);
                }
            }

            if (child._incomingTransitions.Count > 0)
            {
                throw new VertexValidationException(child, "cannot safely remove child as it still has incoming transitions");
            }
        }

        public void RemoveSelf()
        {
            NonNullParent.RemoveChild(this);
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
        public bool ContainsVertex(Vertex v)
        {
            Vertex? current = v;
            int vParentHops = v.Depth - Depth;

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

            switch (v)
            {
                case Statemachine sm:
                    return "ROOT";

                case NamedVertex namedVertex:
                    return namedVertex.Name;

                case InitialState:
                    return Describe(v.Parent) + "." + nameof(InitialState);

                case EntryPoint e:
                    return $"{Describe(v.Parent)}.{nameof(EntryPoint)}({e.label})";

                case ExitPoint e:
                    return $"{Describe(v.Parent)}.{nameof(ExitPoint)}({e.label})";

                case ChoicePoint point:
                    return $"{Describe(v.Parent)}.{nameof(ChoicePoint)}({point.label})";

                case HistoryVertex historyVertex:
                    return Describe(v.Parent) + ".History";

                case NotesVertex:
                    return Describe(v.Parent) + ".Notes";

                default: throw new ArgumentException("Unsupported type " + v.GetType().FullName);
            }
        }
    }
}
