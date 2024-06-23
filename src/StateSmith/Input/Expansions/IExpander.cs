using System.Collections.Generic;
using System.Reflection;

namespace StateSmith.Input.Expansions;

public interface IExpander
{
    /// <summary>This access may change in the future.</summary>
    IDictionary<string, string> VariableExpansions { get; }

    /// <summary>This access may change in the future.</summary>
    IDictionary<string, ExpansionFunction> FunctionExpansions { get; }

    void AddExpansionFunction(string name, object userObject, MethodInfo method);
    void AddVariableExpansion(string name, string code);
    string[] GetFunctionNames();
    string[] GetVariableNames();
    bool HasFunctionName(string name);
    void SetDefaultFuncExpander(DefaultExpander defaultFunctionExpander);
    void SetDefaultVarExpander(DefaultExpander defaultVarExpander);
    string TryExpandFunctionExpansion(string name, string[] arguments, string rawBracedFuncArgs);
    string TryExpandVariableExpansion(string name);
}
