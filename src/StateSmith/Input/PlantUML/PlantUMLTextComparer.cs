#nullable enable

using System;

namespace StateSmith.Input.PlantUML;

public class PlantUMLTextComparer
{
    private const string EntryPointStereotype = "entryPoint";
    private const string ExitPointStereotype = "exitPoint";
    private const string ChoicePointStereotype = "choice";

    /// <summary>
    /// Case insensitive comparison of stereotype text to see if it is an entry point
    /// </summary>
    /// <param name="stereotype"></param>
    /// <returns></returns>
    public static bool IsEntryPointStereotype(string stereotype)
    {
        return stereotype.Equals(EntryPointStereotype, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Case insensitive comparison of stereotype text to see if it is an exit point
    /// </summary>
    /// <param name="stereotype"></param>
    /// <returns></returns>
    public static bool IsExitPointStereotype(string stereotype)
    {
        return stereotype.Equals(ExitPointStereotype, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Case insensitive comparison of stereotype text to see if it is a choice point
    /// </summary>
    /// <param name="stereotype"></param>
    /// <returns></returns>
    public static bool IsChoicePointStereotype(string stereotype)
    {
        return stereotype.Equals(ChoicePointStereotype, StringComparison.OrdinalIgnoreCase);
    }
}
