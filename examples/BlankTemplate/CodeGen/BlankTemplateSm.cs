using StateSmith.Input.Expansions;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankTemplate;

public class BlankTemplateSm
{
    public static void GenFile()
    {
        SmRunner runner = new SmRunner(diagramPath: "../BlankTemplate/BlankTemplateSm.graphml", renderConfig: new MyGlueClass());
        runner.Run();
    }

    public class MyGlueClass : IRenderConfigC
    {
        // These may be required for user specified variables
        string IRenderConfigC.HFileIncludes => @"
            // my H File Includes at the top
        ";

        string IRenderConfigC.CFileIncludes => @"
        ";

        string IRenderConfig.VariableDeclarations => @"
        ";

        public class Expansions : UserExpansionScriptBase
        {
            #pragma warning disable IDE1006 // Naming Styles

            #pragma warning restore IDE1006 // Naming Styles
        }
    }
}
