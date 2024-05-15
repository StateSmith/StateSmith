using StateSmith.Input.Expansions;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace StateSmith.Output.UserConfig;

public class ExpansionConfigReader
{
    private readonly ExpanderFileReflection expanderFileReflection;
    private readonly string expansionVarsPath;

    // required for Dependency Injection
    public ExpansionConfigReader(IExpander expander, IExpansionVarsPathProvider expansionVarsPathProvider, UserExpansionScriptBases userExpansionScriptBases) : this(expander, expansionVarsPathProvider.ExpansionVarsPath, userExpansionScriptBases)
    {
    }

    internal ExpansionConfigReader(IExpander expander, string expansionVarsPath, UserExpansionScriptBases userExpansionScriptBases)
    {
        expanderFileReflection = new ExpanderFileReflection(expander, userExpansionScriptBases);
        this.expansionVarsPath = expansionVarsPath;
    }

    private void FindExpansionsFromFields(ExpansionConfigReaderObjectProvider objectProvider)
    {
        object configObject = objectProvider.obj;
        var type = configObject.GetType();

        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(f => f.FieldType.IsSubclassOf(typeof(UserExpansionScriptBase)));

        foreach (var f in fieldInfos)
        {
            UserExpansionScriptBase? expansionObject = (UserExpansionScriptBase?)f.GetValue(configObject);
            AddExpansionsFromObject(expansionObject);
        }
    }

    private void AddExpansionsFromObject(UserExpansionScriptBase? expansionObject)
    {
        if (expansionObject != null)
        {
            expansionObject.VarsPath = expansionVarsPath;
            expanderFileReflection.AddAllExpansions(expansionObject);
        }
    }

    private void FindExpansionsFromMethods(ExpansionConfigReaderObjectProvider objectProvider)
    {
        object configObject = objectProvider.obj;

        var type = configObject.GetType();

        var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => m.ReturnType.IsSubclassOf(typeof(UserExpansionScriptBase)) && m.GetParameters().Length == 0);

        foreach (var m in methodInfos)
        {
            UserExpansionScriptBase? expansionObject = (UserExpansionScriptBase?)m.Invoke(configObject, null);
            AddExpansionsFromObject(expansionObject);
        }
    }

    private void FindExpansionFromClasses(ExpansionConfigReaderObjectProvider objectProvider)
    {
        object configObject = objectProvider.obj;

        var type = configObject.GetType();

        var classes = type.GetNestedTypes()
            .Where(t => t.IsSubclassOf(typeof(UserExpansionScriptBase)));

        foreach (var classType in classes)
        {
            var ctor = classType.GetConstructor(Type.EmptyTypes);

            if (ctor != null)
            {
                UserExpansionScriptBase? expansionObject = (UserExpansionScriptBase)ctor.Invoke(null);
                AddExpansionsFromObject(expansionObject);
            }
        }
    }

    public void ReadObject(ExpansionConfigReaderObjectProvider objectProvider)
    {
        FindExpansionsFromFields(objectProvider);
        FindExpansionsFromMethods(objectProvider);
        FindExpansionFromClasses(objectProvider);
    }
}
