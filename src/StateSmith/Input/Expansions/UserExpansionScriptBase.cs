using System.Runtime.CompilerServices;

namespace StateSmith.Input.Expansions
{
    public class UserExpansionScriptBase
    {
        /// <summary>
        /// This value will be updated as necessary for target language and rendering engine
        /// </summary>
        internal string varsPath = null;

        /// <summary>
        /// This value will be updated as necessary for target language and rendering engine
        /// </summary>
        public string VarsPath => varsPath;


        /// <summary>
        /// todolow
        /// </summary>
        public static string AutoNameCopy([CallerMemberName] string methodName = null)
        {
            return methodName;
        }

        /// <summary>
        /// todolow
        /// </summary>
        public string AutoVarName([CallerMemberName] string methodName = null)
        {
            return VarsPath + methodName;
        }
    }
}
