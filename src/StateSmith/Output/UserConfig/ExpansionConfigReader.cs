using StateSmith.Input.Expansions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

#nullable enable

namespace StateSmith.Output.UserConfig;

public class ExpansionConfigReader
{
    private readonly ExpanderFileReflection expanderFileReflection;
    private readonly string expansionVarsPath;

    // required for Dependency Injection
    public ExpansionConfigReader(Expander expander, IExpansionVarsPathProvider expansionVarsPathProvider) : this(expander, expansionVarsPathProvider.ExpansionVarsPath)
    {

    }

    internal ExpansionConfigReader(Expander expander, string expansionVarsPath)
    {
        expanderFileReflection = new ExpanderFileReflection(expander);
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
            expansionObject.varsPath = expansionVarsPath;
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
