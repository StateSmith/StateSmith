using StateSmith.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateSmith.SmGraph;

public static class ExtensionMethods
{
    public static void RemoveOrThrow<T>(this List<T> list, T item)
    {
        var success = list.Remove(item);
        if (!success)
        {
            throw new ArgumentException("List could not remove item: " + item);
        }
    }

    public static void RunOrWrapException(this Action action, Func<Exception, Exception> exceptionBuilder)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            var newException = exceptionBuilder(e);
            throw newException;
        }
    }

    public static void AddEnterAction(this NamedVertex namedVertex, string actionCode, int index = -1)
    {
        namedVertex.AddBehavior(Behavior.NewEnterBehavior(actionCode: actionCode), index: index);
    }

    public static void AddExitAction(this NamedVertex namedVertex, string actionCode, int index = -1)
    {
        namedVertex.AddBehavior(Behavior.NewExitBehavior(actionCode: actionCode), index: index);
    }
}
