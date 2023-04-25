using StateSmith.Input.Expansions;
using StateSmith.Output;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.Gil;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;

namespace Blinky1
{
    public class Blinky1PrintfSm
    {
        public static void GenFile()
        {
            var diagramFileName = "blinky1_printf_sm.graphml";

            // You can use this example with the yEd file or an equivalent PlantUML file.
            // See https://github.com/StateSmith/StateSmith/issues/21
            bool usePlantUmlInput = false;
            if (usePlantUmlInput)
            {
                diagramFileName = "blinky1_printf_sm.plantuml";
            }

            RunnerSettings settings = new(diagramFile: "../src/" + diagramFileName);
            settings.dumpErrorsToFile = true;
            settings.dumpGilCodeOnError = true;
            settings.style = new MyStyler();

            SmRunner runner = new(settings, new MyGlueFile());
            runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<NameMangler>(new MyMangler());
            runner.Run();
        }


        /// <summary>
        /// This class 
        /// </summary>
        public class MyGlueFile : IRenderConfigC
        {
            string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ");

            string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                // this ends up in the generated c file
                #include ""app_timer.h""
                #include ""led.h""
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

                public string time_ms => "app_timer_get_ms()";

                // this expansion allows you to access the user defined variable `timer_started_at_ms`
                public string timer_started_at_ms => AutoVarName();

                // sets timer variable with current time
                public string reset_timer() => $"{timer_started_at_ms} = {time_ms}";

                // calculates the elapsed number of milliseconds
                // unsigned math handles ms wrap around
                public string elapsed_ms => $"({time_ms} - {timer_started_at_ms})";

                // creates a true condition 
                public string after_ms(string ms) => $"( {elapsed_ms} >= {ms} )";


                public string turn_led_off() => $"led_turn_off();";
                public string turn_led_on() => $"led_turn_on();";

                #pragma warning restore IDE1006 // Naming Styles
            }
        }


        /// <summary>
        /// This class mangles names. If you would like to customize the generated code names,
        /// here is where you do it. Override the relevant method.
        /// 
        /// Name mangling is more difficult now that we are first rendering to a Generic Intermediate Language (GIL).
        /// The GIL code is then translated to the target language. To get achieve full control over the mangled names,
        /// you may have to rely on the StateSmith post processor as shown below. Open to suggestions for improvements.
        /// </summary>
        class MyMangler : NameMangler
        {
            private string dummy = PostProcessor.dummy;
            private string rmStart = PostProcessor.rmIdentifierStart;
            private string CapsPrefix => $"{rmStart}{Sm.Name.ToUpper()}";

            public override string SmEventEnumType => $"event_id_t";
            public override string SmEventEnumValue(string evt) => $"{CapsPrefix}_EVENT_ID_{evt.ToUpper()}"; // this removes prefix
            public override string SmEventEnumCount => $"{CapsPrefix}_EVENT_ID_COUNT";

            public override string SmStateEnumType => $"{dummy}state_id_t"; // `dummy` required so that Generic Intermediate Language isn't illegal: `public state_id state_id;`.
            public override string SmStateEnumCount => $"{CapsPrefix}_STATE_ID_COUNT";
            public override string SmStateEnumValue(NamedVertex namedVertex)
            {
                string stateName = SmStateName(namedVertex);
                return $"{CapsPrefix}_STATE_ID_{stateName.ToUpper()}";
            }

            public override string SmHandlerFuncType => $"func_t";
        }

        class MyStyler : CodeStyleSettings
        {
            public override bool BracesOnNewLines => false;
            public override string Indent1 => "  "; 
        }
    }
}
