using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace StateSmith.Input.Expansions
{
    public class ExpansionFunction
    {
        public string Name { get; }
        private object methodObject;
        private MethodInfo methodInfo;

        public ExpansionFunction(string name, object methodObject, MethodInfo methodInfo)
        {
            this.methodObject = methodObject;
            this.methodInfo = methodInfo;
            this.Name = name;
        }

        public int ParameterCount => methodInfo.GetParameters().Length;

        public string Evaluate(params String[] strings)
        {
            if (strings.Length != ParameterCount)
            {
                throw new ArgumentException($"Expansion `{Name}` requires {ParameterCount} arguments. Was passed {strings.Length}: " + strings);
            }

            string result = (string)methodInfo.Invoke(methodObject, strings);

            return result;
        }

    }
}
