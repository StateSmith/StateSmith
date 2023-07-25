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

    /// <summary>
    /// Adds an enter behavior action to list end (unless index specified).
    /// </summary>
    /// <param name="namedVertex"></param>
    /// <param name="actionCode"></param>
    /// <param name="index">Set to -1 to ignore index, 0 to insert behavior at start. Too large values are clamped 
    /// to be valid so you don't have to worry.</param>
    /// <returns></returns>
    public static void AddEnterAction(this NamedVertex namedVertex, string actionCode, int index = -1)
    {
        namedVertex.AddBehavior(Behavior.NewEnterBehavior(actionCode: actionCode), index: index);
    }

    /// <summary>
    /// Adds an exit behavior action to list end (unless index specified).
    /// </summary>
    /// <param name="namedVertex"></param>
    /// <param name="actionCode"></param>
    /// <param name="index">Set to -1 to ignore index, 0 to insert behavior at start. Too large values are clamped 
    /// to be valid so you don't have to worry.</param>
    /// <returns></returns>
    public static void AddExitAction(this NamedVertex namedVertex, string actionCode, int index = -1)
    {
        namedVertex.AddBehavior(Behavior.NewExitBehavior(actionCode: actionCode), index: index);
    }
}
