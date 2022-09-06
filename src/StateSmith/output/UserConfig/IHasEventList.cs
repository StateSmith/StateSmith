using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StateSmith.output.UserConfig
{
    ///// <summary>
    ///// Used to specify which events are allowed in the state machine.
    ///// Can be used on a method/field that returns a comma seperated string of event names, or an array of event names.
    ///// If not found in an RenderConfig, no event validation is done.
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    //public class EventListAttribute : Attribute
    //{

    //}

    // TODO remove interface?
    public interface IHasEventList
    {
        /// <summary>
        /// A comma seperated list of allowed event names. TODO case sensitive?
        /// </summary>
        public string EventCommaList => "";
    }
}
