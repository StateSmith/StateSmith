using StateSmith.Input.Expansions;
using StateSmith.Output;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

namespace ExampleLaserTagMenu1
{
    public class ButtonSm1
    {
        public static void GenFile()
        {
            SmRunner runner = new(diagramPath: "../LaserTagMenu1/ButtonSm1.graphml", new MyGlueFile());
            runner.Run();
        }

        public class MyGlueFile : IRenderConfigC
        {
            string IRenderConfigC.CEnumDeclarer => "typedef enum __attribute__((packed)) {enumName}";

            string IRenderConfigC.HFileIncludes => @"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ";

            string IRenderConfigC.CFileIncludes => @"
                #include ""PortApi.h""
            ";

            string IRenderConfig.VariableDeclarations => @"
                    // Note! This example below uses bitfields just to show that you can. They aren't required and might not
                    // save you any actual RAM depending on the compiler struct padding/alignment/enum size... One day, we will be able choose where the vars
                    // structure is positioned relative to the other state machine fields.
                    // You can convert any of the fields below from bitfields and the code will still work fine.

                    /** used by state machine. If you change bitfield size, also update `time_ms` expansion masking. */
                    uint16_t debounce_started_at_ms : 11;

                    uint16_t input_is_pressed : 1; // input
                    uint16_t output_event_press : 1; // output
                    uint16_t output_event_release : 1; // output
                    uint16_t output_event_held : 1; // output
                    uint16_t output_event_tap : 1; // output
                ";

            public class Expansions : UserExpansionScriptBase
            {
                #pragma warning disable IDE1006 // Naming Styles

                public string time_mask => "2047";
                public string time_ms => $"({time_mask} & PortApi_get_time_ms())"; // only use bottom 11 bits of of time. 2047 ms is long enough for this state machine.

                public string is_pressed => VarsPath + "input_" + AutoNameCopy(); // ends up as "input_is_pressed"
                public string is_released => $"(!{is_pressed})";

                public string output_event(string eventName) => $"{VarsPath}output_event_{eventName.ToLower()} = true";

                public string debounce_started_at_ms => AutoVarName();
                public string reset_debounce_timer() => $"{debounce_started_at_ms} = {time_ms}";
                public string debounce_ms() => $"({time_mask} & ({time_ms} - {debounce_started_at_ms}))";
                public string after_debounce_ms(string ms) => $"( {debounce_ms()} >= {ms} )";    // handles ms wrap around

                public string is_debounced => $"({after_debounce_ms("20")})";

                #pragma warning restore IDE1006 // Naming Styles
            }
        }
    }
}
