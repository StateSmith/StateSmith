using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace StateSmith.Input.Expansions
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class ExpansionNameAttribute : Attribute
    {
        public string Name { get; }

        /// <summary>
        /// Use when 
        /// </summary>
        /// <param name="name"></param>
        public ExpansionNameAttribute(string name)
        {
            Name = name;
        }
    }
}
