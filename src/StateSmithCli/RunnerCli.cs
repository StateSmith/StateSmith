using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace StateSmith.Runner;

class RunnerCli
{
    /*
     StateSmith.exe --language c --diagram_file MySm.graphml --glue_script Glue.cs --glue_object MySmC --output_dir ../src/
         language defaults to c
         diagram_file required
         glue optional
         output_dir optional. Will default to where diagram_file is.

     StateSmith.exe --language c --diagram_file MySm.graphml  --target_sm MySm  --glue Glue.cs:MySmC --output_dir ../src/
         target_sm - optional. Useful when a diagram file has multiple statemachines in it.


    Execution Steps:
        - Parse yed files to node/edges graph
        - Allow modification

        - Build state machine vertex/transition graphs
        - Allow modification (add states, add events, modify behaviors...)

        - Parse expansions
        - Allow adding/modifying expansions
        - Expand state machine behaviors
        - Allow modification

        - Resolve statemachine references (one sm includes/references another)
        - Validate
            - requires `event_list` setting

        - render output
    */

    enum RendererId
    {
        Balanced1
    }

    enum RenderLanguageId
    {
        C
    }

    //https://github.com/commandlineparser/commandline
    class Options
    {
        [Option("diagram", Required = true, HelpText = "Path to diagram file.")]
        public string DiagramFile { get; set; }

        [Option("glue_script", Required = false, HelpText = "Path to glue script")]
        public string GlueScriptPath { get; set; }

        [Option("glue_object", Required = false, HelpText = "Which object to use from glue script.")]
        public string GlueObjectName { get; set; }

        [Option("output_dir", Required = false, HelpText = "")]
        public string OutputDirectory { get; set; }

        //[Option("select_sm", Required = false, HelpText = "Allows rendering a single state machine (if multiple present in diagram).")]
        //public string SelectedSmName { get; set; }

        [Option('r', "renderer", Default = RendererId.Balanced1, Required = false, HelpText = "Specifies which renderer to use")]
        public RendererId Renderer { get; set; }

        [Option('l', "language", Default = RenderLanguageId.C, Required = false, HelpText = "Specifies which renderer to use")]
        public RenderLanguageId Language { get; set; }
    }

    static void Main(string[] args)
    {
        new CreateUi().Run();

        //CommandLine.Parser.Default.ParseArguments<Options>(args)
        //  .WithParsed(RunOptions)
        //  .WithNotParsed(HandleParseError);
    }

    private static void RunOptions(Options opts)
    {
        
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        Console.WriteLine();
    }
}
