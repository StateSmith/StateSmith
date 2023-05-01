using StateSmith.Input.Expansions;
using StateSmith.Output;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;

namespace ExampleButtonSm1Cpp
{
    public class ButtonSm1Cpp
    {
        public static void GenFile()
        {
            var diagramFileName = "ButtonSm1Cpp.graphml";

            // You can use this example with the yEd file or an equivalent PlantUML file.
            // See https://github.com/StateSmith/StateSmith/issues/21
            bool usePlantUmlInput = false;
            if (usePlantUmlInput)
            {
                diagramFileName = "ButtonSm1Cpp.puml";
            }

            SmRunner runner = new(diagramPath: $"../ButtonSm1Cpp/{diagramFileName}", new MyGlueFile());
            runner.Run();
        }

        public class MyGlueFile : IRenderConfigC
        {
            string IRenderConfigC.CFileExtension => ".cpp"; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => ".h";
            string IRenderConfigC.CEnumDeclarer => "typedef enum __attribute__((packed)) {enumName}";

            // These are required for user specified variables
            string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ");

            string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""Arduino.h""
            ");

            string IRenderConfig.VariableDeclarations =>
                StringUtils.DeIndentTrim(@"
                    // Note! This example below uses bitfields just to show that you can. They aren't required.

                    // This can be made to be 11 bits if RAM is at a premium. See laser tag menu example.
                    uint32_t debounce_started_at_ms;

                    uint16_t input_is_pressed : 1; // input
                    uint16_t output_event_press : 1; // output
                    uint16_t output_event_release : 1; // output
                    uint16_t output_event_held : 1; // output
                    uint16_t output_event_tap : 1; // output
                ");

            public class Expansions : UserExpansionScriptBase
            {
                #pragma warning disable IDE1006 // Naming Styles

                public string time_ms => $"millis()";   // directly calls Arduino C++ code

                public string is_pressed => VarsPath + "input_" + AutoNameCopy(); // ends up as "input_is_pressed"
                public string is_released => $"(!{is_pressed})";

                public string output_event(string eventName) => $"{VarsPath}output_event_{eventName.ToLower()} = true";

                public string debounce_started_at_ms => AutoVarName();
                public string reset_debounce_timer() => $"{debounce_started_at_ms} = {time_ms}";
                public string debounce_ms() => $"({time_ms} - {debounce_started_at_ms})";       // unsigned math work even with ms roll over
                public string after_debounce_ms(string ms) => $"( {debounce_ms()} >= {ms} )";

                public string is_debounced => $"({after_debounce_ms("20")})";

                // expansion for plantuml. I can't get it to render multiline action code nicely (it center aligns it).
                public string release_events() => StringUtils.DeIndentTrim($@"
                    if ({debounce_ms()} <= 200) {{
                      {output_event("tap")};
                    }}
                    {output_event("release")}");

                #pragma warning restore IDE1006 // Naming Styles
            }
        }
    }
}
