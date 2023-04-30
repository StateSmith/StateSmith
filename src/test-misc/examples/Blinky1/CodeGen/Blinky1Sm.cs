using StateSmith.Input.Expansions;
using StateSmith.Output;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;

namespace Blinky1
{
    public class Blinky1Sm
    {
        public static void GenFile()
        {
            SmRunner runner = new(diagramPath: "../Blinky1/Blinky1Sm.graphml", new MyGlueFile());
            runner.Run();
        }

        /// <summary>
        /// This class 
        /// </summary>
        public class MyGlueFile : IRenderConfigC
        {
            string IRenderConfigC.CFileExtension => ".cpp"; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => ".h";

            string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ");

            string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                // this ends up in the generated .cpp file
                #include ""Arduino.h""
            ");

            string IRenderConfig.VariableDeclarations => StringUtils.DeIndentTrim(@"
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
    }
}
