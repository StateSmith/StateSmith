using StateSmith.Input.Expansions;
using StateSmith.output;
using StateSmith.output.C99BalancedCoder1;
using StateSmith.output.UserConfig;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankTemplate
{   
    public class BlankTemplateSm
    {
        public static void GenFile()
        {
            MyGlueFile myGlueFile = new MyGlueFile();

            var directory = AppDomain.CurrentDomain.BaseDirectory + "../../../../BlankTemplate/";
            var diagramFile = directory + "BlankTemplateSm.graphml";

            RunnerSettings settings = new RunnerSettings(myGlueFile, diagramFile: diagramFile, outputDirectory: directory);
            SmRunner runner = new SmRunner(settings);

            runner.Run();
        }

        public class MyGlueFile : IRenderConfigC
        {
            // These are required for user specified variables
            string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
            ");

            string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
            ");

            string IRenderConfigC.VariableDeclarations =>
                StringUtils.DeIndentTrim(@"
                ");

            public class Expansions : UserExpansionScriptBase
            {
                #pragma warning disable IDE1006 // Naming Styles

                #pragma warning restore IDE1006 // Naming Styles
            }
        }
    }
}
