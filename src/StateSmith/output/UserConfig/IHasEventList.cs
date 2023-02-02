using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StateSmith.Output.UserConfig
{
    /// <summary>
    /// might be used when we support multiple output languages.don't use for now.
    /// </summary>
    public interface IHasEventList
    {
        /// <summary>
        /// A comma separated list of allowed event names.
        /// </summary>
        public string EventCommaList => "";
    }
}
