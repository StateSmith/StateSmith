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

namespace ExampleButtonSm1Cpp
{   
    public class ButtonSm1Cpp
    {
         public static void GenFile()
        {
            MyGlueFile myGlueFile = new MyGlueFile();

            var directory = DirectoryHelper.CodeDirectory;
            var diagramFile = directory + "ButtonSm1Cpp.graphml";

            RunnerSettings settings = new RunnerSettings(myGlueFile, diagramFile: diagramFile, outputDirectory: directory);
            settings.mangler = new MyMangler();
            SmRunner runner = new SmRunner(settings);

            runner.Run();
        }

        public class MyGlueFile : IRenderConfigC
        {
            string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""Arduino.h""
            ");

            // These are required for user specified variables
            string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                #include <stdint.h>
            ");

            string IRenderConfigC.VariableDeclarations =>
                StringUtils.DeIndentTrim(@"
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
                ");

            string IRenderConfigC.EventCommaList => @"
                do,
            ";

            public class Expansions : UserExpansionScriptBase
            {
                #pragma warning disable IDE1006 // Naming Styles

                public string time_mask => "2047";
                public string time_ms => $"({time_mask} & millis())"; // only use bottom 11 bits of of time. 2047 ms is long enough for this state machine.

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

        class MyMangler : CNameMangler
        {
            // NOTE! We choose to output as c++ code so that it can be used directly with Arduino.
            public override string CFileName => $"{SmName}.cpp";

            // packing attributes for gcc
            public override string SmStateEnumAttribute => "__attribute__((packed)) ";
            public override string SmEventEnumAttribute => "__attribute__((packed)) ";
        }
    }
}
