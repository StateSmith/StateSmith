using StateSmith.Output.UserConfig;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StateSmith.Input.Expansions;

public class ExpanderFileReflection
{
    private readonly IExpander expander;
    private readonly HashSet<MethodInfo> propertyMethods = new();
    private readonly UserExpansionScriptBases userExpansionScriptBases;


    public ExpanderFileReflection(IExpander expander, UserExpansionScriptBases userExpansionScriptBases)
    {
        this.expander = expander;
        this.userExpansionScriptBases = userExpansionScriptBases;
    }

    public void AddAllExpansions(UserExpansionScriptBase expansionBase)
    {
        userExpansionScriptBases.Add(expansionBase);

        var fields = expansionBase.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        AddFieldExpansions(expansionBase, fields);

        // Must be done before methods are added. Code that adds method expansions explains more.
        var propertyInfos = expansionBase.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        AddPropertyExpansions(expansionBase, propertyInfos);

        var methods = expansionBase.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        AddMethodExpansions(expansionBase, methods);
    }

    private void AddPropertyExpansions(UserExpansionScriptBase userObject, PropertyInfo[] propertyInfos)
    {
        foreach (var p in propertyInfos)
        {
            ProcessProperty(userObject, p);
        }
    }

    private void AddMethodExpansions(UserExpansionScriptBase userObject, MethodInfo[] methods)
    {
        foreach (var method in methods)
        {
            ProcessMethod(userObject, method);
        }
    }

    private void ProcessProperty(UserExpansionScriptBase userObject, PropertyInfo property)
    {
        if (property.PropertyType != typeof(string))
        {
            return; //only care about methods that return strings
        }

        MethodInfo getMethodInfo = property.GetGetMethod(nonPublic: true);
        if (getMethodInfo == null)
        {
            return; //only care about getter properties
        }

        if (ShouldSkipInheritedMember(property.DeclaringType))
        {
            return;
        }

        propertyMethods.Add(getMethodInfo);

        string expansionName = property.Name;
        var expansionNameAttributes = property.GetCustomAttributes(typeof(ExpansionNameAttribute), inherit: false);

        if (expansionNameAttributes.Length > 0)
        {
            ExpansionNameAttribute attribute = (ExpansionNameAttribute)expansionNameAttributes[0];
            expansionName = attribute.Name;
        }

        string code = (string)getMethodInfo.Invoke(userObject, null);
        expander.AddVariableExpansion(expansionName, code);
    }

    private bool ShouldSkipInheritedMember(Type declaringType)
    {
        return declaringType == typeof(UserExpansionScriptBase) || declaringType == typeof(System.Object);
    }

    private void ProcessMethod(UserExpansionScriptBase userObject, MethodInfo method)
    {
        if (method.ReturnType != typeof(string))
        {
            return; //only care about methods that return strings
        }

        if (propertyMethods.Contains(method))
        {
            return; //we don't want compiler generated methods for properties like `get_get_time`
        }

        if (ShouldSkipInheritedMember(method.DeclaringType))
        {
            return;
        }

        string expansionName = method.Name;
        var expansionNameAttributes = method.GetCustomAttributes(typeof(ExpansionNameAttribute), inherit: false);

        if (expansionNameAttributes.Length > 0)
        {
            ExpansionNameAttribute attribute = (ExpansionNameAttribute)expansionNameAttributes[0];
            expansionName = attribute.Name;
        }

        expander.AddExpansionFunction(expansionName, userObject, method);
    }

    private void AddFieldExpansions(UserExpansionScriptBase userObject, FieldInfo[] fields)
    {
        foreach (var field in fields)
        {
            ProcessField(userObject, field);
        }
    }

    private void ProcessField(UserExpansionScriptBase userObject, FieldInfo field)
    {
        //we only care about fields of type string
        if (field.FieldType != typeof(string))
        {
            return;
        }

        if (ShouldSkipInheritedMember(field.DeclaringType))
        {
            return;
        }

        string expansionName = field.Name;
        string expansionOutput = (string)field.GetValue(userObject);
        var expansionNameAttributes = field.GetCustomAttributes(typeof(ExpansionNameAttribute), inherit: false);

        if (expansionNameAttributes.Length > 0)
        {
            ExpansionNameAttribute attribute = (ExpansionNameAttribute)expansionNameAttributes[0];
            expansionName = attribute.Name;
        }

        expander.AddVariableExpansion(name: expansionName, code: expansionOutput);
    }
}
