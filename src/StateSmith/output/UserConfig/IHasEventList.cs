using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig
{
    public interface IHasEventList
    {
        /// <summary>
        /// A comma separated list of allowed event names.
        /// </summary>
        public string EventCommaList => "";
    }
}
