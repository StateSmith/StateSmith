using System;
using System.Collections.Generic;
using System.Reflection;

namespace StateSmith.Input.Expansions;

public class Expander
{
    private readonly Dictionary<string, string> variableExpansions = new();
    private readonly Dictionary<string, ExpansionFunction> functionExpansions = new();

    /// <summary>This access may change in the future.</summary>
    public IDictionary<string, string> VariableExpansions => variableExpansions;
    /// <summary>This access may change in the future.</summary>
    public IDictionary<string, ExpansionFunction> FunctionExpansions => functionExpansions;

    private void ThrowIfExpansionNameAlreadyUsed(string expansionName)
    {
        //todo_low make custom exception
        if (variableExpansions.ContainsKey(expansionName))
        {
            throw new ArgumentException($"Expansion name `{expansionName}` already has a variable mapping");
        }

        if (functionExpansions.ContainsKey(expansionName))
        {
            throw new ArgumentException($"Expansion name `{expansionName}` already has a function mapping");
        }
    }

    public void AddVariableExpansion(string name, string code)
    {
        ThrowIfExpansionNameAlreadyUsed(name);
        variableExpansions.Add(name, code);
    }

    public void AddExpansionFunction(string name, object userObject, MethodInfo method)
    {
        ThrowIfExpansionNameAlreadyUsed(name);
        functionExpansions.Add(name, new ExpansionFunction(name, userObject, method));
    }

    public string TryExpandVariableExpansion(string name)
    {
        if (variableExpansions.ContainsKey(name) == false)
        {
            return name;
        }

        return variableExpansions[name];
    }

    public string TryExpandFunctionExpansion(string name, string[] arguments)
    {
        if (functionExpansions.ContainsKey(name) == false)
        {
            return name;
        }

        var function = functionExpansions[name];
        var code = function.Evaluate(arguments);
        return code;
    }

    public string[] GetVariableNames()
    {
        var keys = new string[variableExpansions.Count];
        variableExpansions.Keys.CopyTo(keys, 0);
        return keys;
    }

    public bool HasFunctionName(string name)
    {
        return functionExpansions.ContainsKey(name);
    }

    public string[] GetFunctionNames()
    {
        var keys = new string[functionExpansions.Count];
        functionExpansions.Keys.CopyTo(keys, 0);
        return keys;
    }
}
