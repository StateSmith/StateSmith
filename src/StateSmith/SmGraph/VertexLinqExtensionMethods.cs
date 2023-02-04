using StateSmith.SmGraph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using StateSmith.Runner;

#nullable enable

namespace StateSmith.SmGraph
{
    /// <summary>
    /// Helper extension methods
    /// </summary>
    public static class VertexLinqExtensionMethods
    {
        public static List<T> DescendantsOfType<T>(this Vertex vertex) where T : Vertex
        {
            List<T> list = new();
            vertex.VisitTypeRecursively<T>(v => list.Add(v));
            if (vertex is T t)
            {
                list.Remove(t);
            }
            return list;
        }

        private static void ThrowIfNotSingle(string name, List<NamedVertex> matches)
        {
            if (matches.Count > 1)
            {
                throw new ArgumentException($"More than 1 named vertex found with name: `{name}`");
            }

            if (matches.Count == 0)
            {
                throw new ArgumentException($"No named vertex found with name: `{name}`");
            }
        }

        private static void ThrowIfNotSingle<T>(List<T> matches) where T : Vertex
        {
            var typeName = typeof(T).FullName;
            if (matches.Count > 1)
            {
                throw new ArgumentException($"More than 1 vertex found with type `{typeName}`");
            }

            if (matches.Count == 0)
            {
                throw new ArgumentException($"No vertex found with type `{typeName}`");
            }
        }

        public static IEnumerable<T> Children<T>(this Vertex vertex) where T : Vertex
        {
            return vertex.Children.OfType<T>();
        }

        public static T? SingleChildOrNull<T>(this Vertex vertex) where T : Vertex
        {
            return vertex.Children<T>().SingleOrNull(vertex, $"Max of 1 {typeof(T).Name} allowed in {vertex.GetType().Name}");
        }

        // does not return self
        public static IEnumerable<Vertex> Siblings(this Vertex vertex)
        {
            return vertex.NonNullParent.Children.Where(v => v != vertex);
        }

        // does not return self
        public static IEnumerable<T> Siblings<T>(this Vertex vertex) where T : Vertex
        {
            return vertex.NonNullParent.Children.OfType<T>().Where(v => v != vertex);
        }

        public static T? SingleSiblingOrNull<T>(this Vertex vertex) where T : Vertex
        {
            var siblings = vertex.Siblings<T>();
            return siblings.SingleOrNull(vertex, $"Max of 1 {nameof(T)} allowed in {vertex.GetType().Name}");
        }

        // does not return self
        public static IEnumerable<T> SiblingsOfMyType<T>(this T vertex) where T : Vertex
        {
            return vertex.Siblings<T>();
        }

        public static T? SingleOrNull<T>(this IEnumerable<T> items, Vertex v, string errorMessage) where T : Vertex
        {
            int count = items.Count();

            if (count > 1)
            {
                throw new VertexValidationException(v, $"{errorMessage}. Found {count}.");
            }

            return items.FirstOrDefault();
        }

        public static void RunIfPresent<T>(this IEnumerable<T> items, int limit, Action<T> action)
        {
            int count = items.Count();

            if (count > limit)
            {
                throw new ArgumentException($"limit of {limit} exceeded on collection. Found {count}.");
            }

            foreach (var item in items)
            {
                action(item);
            }
        }

        public static IEnumerable<NamedVertex> NamedChildren(this Vertex vertex, string name)
        {
            return vertex.Children.OfType<NamedVertex>().Where(v => v.Name == name);
        }

        public static IEnumerable<NamedVertex> NamedChildren(this Vertex vertex)
        {
            return vertex.Children.OfType<NamedVertex>();
        }

        public static NamedVertex Child(this Vertex vertex, string name)
        {
             var list = vertex.Children.OfType<NamedVertex>().Where(v => v.Name == name).ToList();
            ThrowIfNotSingle(name, list);
            return list[0];
        }

        public static T Child<T>(this Vertex vertex, string name) where T : NamedVertex
        {
            return (T)vertex.Child(name);
        }

        public static T ChildType<T>(this Vertex vertex) where T : Vertex
        {
            var list = vertex.Children.OfType<T>().ToList();
            ThrowIfNotSingle<T>(list);
            return list[0];
        }

        public static IEnumerable<Behavior> TransitionBehaviors(this Vertex vertex)
        {
            return vertex.Behaviors.Where(b => b.HasTransition());
        }

        public static string UmlDescription(this IEnumerable<Behavior> behaviors)
        {
            string str = "";
            string joiner = "";

            foreach (var b in behaviors)
            {
                str += joiner + b.DescribeAsUml();
                joiner = "\n";
            }

            return str;
        }

        public static IEnumerable<Behavior> GetBehaviorsWithTrigger(this Vertex vertex, string triggerName)
        {
            return vertex.Behaviors.Where(b => b.triggers.Contains(triggerName));
        }

        public static bool HasInitialState(this Vertex vertex, out InitialState? initialState)
        {
            if (vertex is NamedVertex namedVertex)
            {
                initialState = namedVertex.Children.OfType<InitialState>().FirstOrDefault();
                return true;
            }
            else
            {
                initialState = null;
                return false;
            }
        }

        public static void VisitRecursively(this Vertex vertex, Action<Vertex> action)
        {
            vertex.VisitTypeRecursively<Vertex>(action);
        }

        public static void VisitTypeRecursively<T>(this Vertex vertex, Action<T> action) where T : Vertex
        {
            if (vertex is T t)
            {
                action(t);
            }

            foreach (var child in vertex.Children)
            {
                child.VisitTypeRecursively<T>(action);
            }
        }

        public static void RemoveAllChildren(this Vertex vertex)
        {
            foreach (var child in vertex.Children.ToArray()) // copy so we can modify collection
            {
                vertex.RemoveChild(child);
            }
        }

        public static void RemoveChildrenAndSelf(this Vertex vertex)
        {
            vertex.RemoveAllChildren();
            vertex.RemoveSelf();
        }

        public static void RemoveRecursively(this Vertex vertex)
        {
            Stack<Vertex> toRemove = new();
            vertex.VisitRecursively(v => toRemove.Push(v));

            foreach (var v in toRemove)
            {
                if (v.Parent != null)
                    v.RemoveSelf();
            }
        }
    }
}
