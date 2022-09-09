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

namespace Blinky1
{   
    public class Blinky1Sm
    {
        public static void GenFile()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory + "../../../../Blinky1/";
            var diagramFile = directory + nameof(Blinky1Sm)+ ".graphml";

            MyGlueFile myGlueFile = new();
            RunnerSettings settings = new(myGlueFile, diagramFile: diagramFile, outputDirectory: directory);
            settings.mangler = new MyMangler();

            SmRunner runner = new(settings);

            runner.Run();
        }


        /// <summary>
        /// This class 
        /// </summary>
        public class MyGlueFile : IRenderConfigC
        {
            string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                #include <stdint.h>  // this ends up in the generated .h file
            ");

            string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                // this ends up in the generated .cpp file
                #include ""Arduino.h""
            ");

            string IRenderConfigC.VariableDeclarations => StringUtils.DeIndentTrim(@"
                uint32_t timer_started_at_ms;  // milliseconds
            ");

            /// <summary>
            /// This class creates expansions. It is instantiated using reflection.
            /// </summary>
            public class Expansions : UserExpansionScriptBase
            {
                #pragma warning disable IDE1006 // Naming Styles

                public string time_ms => "millis()";

                // this expansion allows you to access the user defined variable `timer_started_at_ms`
                public string timer_started_at_ms => AutoVarName();

                // sets timer variable with current time
                public string reset_timer() => $"{timer_started_at_ms} = {time_ms}";

                // calculates the elapsed number of milliseconds
                // unsigned math handles ms wrap around
                public string elapsed_ms => $"({time_ms} - {timer_started_at_ms})";

                // creates a true condition 
                public string after_ms(string ms) => $"( {elapsed_ms} >= {ms} )";


                public string turn_led_off() => $"digitalWrite(LED_BUILTIN, LOW);";
                public string turn_led_on() => $"digitalWrite(LED_BUILTIN, HIGH);";

                #pragma warning restore IDE1006 // Naming Styles
            }
        }


        /// <summary>
        /// This class mangles names. If you would like to customize the generated code names,
        /// here is where you do it. Override the relevant method.
        /// </summary>
        class MyMangler : CNameMangler
        {
            // NOTE! We choose to output as c++ code (c is default) so that it can be used directly with Arduino.
            // `SmName` property is inherited from CNameMangler. 
            public override string CFileName => $"{SmName}.cpp";
        }
    }
}
