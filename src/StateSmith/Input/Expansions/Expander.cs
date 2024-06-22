#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace StateSmith.Input.Expansions;

public class Expander : IExpander
{
    protected readonly Dictionary<string, string> variableExpansions = new();
    protected readonly Dictionary<string, ExpansionFunction> functionExpansions = new();

    protected DefaultExpander? defaultVarExpander = null;
    protected DefaultExpander? defaultFunctionExpander = null;

    /// <summary>This access may change in the future.</summary>
    public virtual IDictionary<string, string> VariableExpansions => variableExpansions;
    /// <summary>This access may change in the future.</summary>
    public virtual IDictionary<string, ExpansionFunction> FunctionExpansions => functionExpansions;

    protected void ThrowIfExpansionNameAlreadyUsed(string expansionName)
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

    public virtual void SetDefaultVarExpander(DefaultExpander defaultVarExpander)
    {
        this.defaultVarExpander = defaultVarExpander;
    }

    public virtual void SetDefaultFuncExpander(DefaultExpander defaultFunctionExpander)
    {
        this.defaultFunctionExpander = defaultFunctionExpander;
    }

    public virtual void AddVariableExpansion(string name, string code)
    {
        ThrowIfExpansionNameAlreadyUsed(name);
        variableExpansions.Add(name, code);
    }

    public virtual void AddExpansionFunction(string name, object userObject, MethodInfo method)
    {
        ThrowIfExpansionNameAlreadyUsed(name);
        functionExpansions.Add(name, new ExpansionFunction(name, userObject, method));
    }

    public virtual string TryExpandVariableExpansion(string name)
    {
        if (variableExpansions.ContainsKey(name) == false)
        {
            if (defaultVarExpander != null)
            {
                return defaultVarExpander.Process(name);
            }

            return name;
        }

        return variableExpansions[name];
    }

    public virtual string TryExpandFunctionExpansion(string name, string[] arguments, string rawBracedFuncArgs = "")
    {
        if (functionExpansions.ContainsKey(name) == false)
        {
            if (defaultFunctionExpander != null)
            {
                return defaultFunctionExpander.Process(name) + rawBracedFuncArgs;
            }

            return name;
        }

        var function = functionExpansions[name];
        var code = function.Evaluate(arguments);
        return code;
    }

    public virtual string[] GetVariableNames()
    {
        var keys = new string[variableExpansions.Count];
        variableExpansions.Keys.CopyTo(keys, 0);
        return keys;
    }

    public virtual bool HasFunctionName(string name)
    {
        return functionExpansions.ContainsKey(name) || (defaultFunctionExpander != null);
    }

    public virtual string[] GetFunctionNames()
    {
        var keys = new string[functionExpansions.Count];
        functionExpansions.Keys.CopyTo(keys, 0);
        return keys;
    }
}
