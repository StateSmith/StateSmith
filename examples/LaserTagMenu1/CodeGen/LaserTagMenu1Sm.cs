using StateSmith.Input.Expansions;
using StateSmith.output;
using StateSmith.output.UserConfig;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleLaserTagMenu1
{
    public class LaserTagMenu1Sm
    {
        public static void GenFile()
        {
            MyGlueFile myGlueFile = new MyGlueFile();

            var codeDirectory = DirectoryHelper.CodeDirectory;
            var diagramFile = codeDirectory + "LaserTagMenu1Sm.graphml";

            RunnerSettings settings = new RunnerSettings(myGlueFile, diagramFile: diagramFile, outputDirectory: codeDirectory);
            SmRunner runner = new SmRunner(settings);

            runner.Run();
        }

        public class MyGlueFile : IRenderConfigC
        {
            // These are required for user specified variables
            string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                #include <stdint.h>
            ");

            string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""App.h""
                #include ""Display.h""
                #include ""PortApi.h""
            ");

            string IRenderConfigC.VariableDeclarations =>
                StringUtils.DeIndentTrim(@"
                    uint8_t option_value;
                    uint8_t back_press_eat_count;
                    uint32_t timer1_started_at_ms;
                ");

            string IRenderConfigC.EventCommaList => @"
                do,
                DOWN_PRESS, DOWN_HELD,
                UP_PRESS, UP_HELD,
                OK_PRESS,
            ";

            public class Expansions : UserExpansionScriptBase
            {
                #pragma warning disable IDE1006 // Naming Styles

                // variables
                public string option_value         => AutoVarName();
                public string timer1_started_at_ms => AutoVarName();
                public string back_press_eat_count => AutoVarName();

                public string time_ms => "PortApi_get_time_ms()";

                public string reset_timer1() => $"{timer1_started_at_ms} = {time_ms}";
                public string timer1_ms() => $"({time_ms} - {timer1_started_at_ms})";

                public string after_timer1_ms(string ms) => $"( {timer1_ms()} >= {ms} )";    // handles ms wrap around

                public string info_timed_out => $"({after_timer1_ms("3000")})";


                // Display short cuts
                public string menu_at_top() => "Display_" + AutoNameCopy() + "()";   // ends up as "Display_menu_at_top()"
                public string menu_at_mid() => $"Display_{AutoNameCopy()}()";
                public string menu_at_bottom() => "Display_" + AutoNameCopy() + "()";
                public string show_home_screen_1() => "Display_" + AutoNameCopy() + "()";
                public string show_home_screen_2() => "Display_" + AutoNameCopy() + "()";
                public string show_home_screen_3() => "Display_" + AutoNameCopy() + "()";
                public string show_back_press_taunt(string str) => $"Display_{AutoNameCopy()}({str})";
                public string show_random_back_press_taunt() => "Display_" + AutoNameCopy() + "()";


                public string show_back_press_count() => $"Display_show_back_press_count({back_press_eat_count})";

                public string dont_consume_event() => "consume_event = false";

                public string set_option_class(string class_name) => $"{option_value} = PlayerClass_{class_name}";

                public string set_menu_option_and_class(string class_name)
                {
                    return $"Display_menu_option(\"{class_name}\"); {set_option_class(class_name)}";
                }

                public string save_option_as_class() => $"App_save_player_class({option_value})";
                #pragma warning restore IDE1006 // Naming Styles
            }
        }
    }
}
