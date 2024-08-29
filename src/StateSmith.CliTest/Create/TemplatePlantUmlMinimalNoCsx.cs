using NSubstitute;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest.Create;

public class TemplatePlantUmlMinimalNoCsx
{
    Settings settings = new Settings
    {
        UseCsxWorkflow = false,
        diagramFileName = "RocketSm.plantuml",
        FileExtension = ".plantuml",
        PlantUmlDiagramTemplateId = TemplateIds.PlantUmlMinimal1,
        smName = "RocketSm"
    };

    IFileWriter mockFileWriter = Substitute.For<IFileWriter>();

    string Top = """
        @startuml RocketSm
        
        ' //////////////////////// STATE ORGANIZATION ///////////////////////
        ' Note: StateSmith treats state names and events as case insensitive.
        ' More info: https://github.com/StateSmith/StateSmith/wiki/PlantUML
        
        state State1
        state State2


        ' ///////////////////////// STATE HANDLERS /////////////////////////
        ' Syntax: https://github.com/StateSmith/StateSmith/wiki/Behaviors

        [*] -> State1

        ' State1
        State1: enter / count++;
        State1: exit  / 
        State1 --> State2 : event1

        ' State2
        State2: enter / count += 10;
        State2: exit  / 
        State2 --> State1 : event2
        
        
        ' //////////////////////// StateSmith config ////////////////////////
        ' The below special comment block sets the StateSmith configuration.
        ' More info: https://github.com/StateSmith/StateSmith/issues/335
        
        /'! $CONFIG : toml
        """;

    [Fact]
    public void LangC()
    {
        settings.TargetLanguageId = TargetLanguageId.C;
        Generator generator = new(settings);
        generator.tomlConfigType = TemplateLoader.TomlConfigType.Minimal;
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write("RocketSm.plantuml", Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff($$""""
            {{Top}}

            [RenderConfig]
            FileTop = """
                // Whatever you put in this `FileTop` section will end up 
                // being printed at the top of every generated code file.
                """
            AutoExpandedVars = """
                uint16_t count; // this var can be referenced in diagram
                """

            [RenderConfig.C]
            # CFileExtension = ".inc" # if you want to include sm in another file
            HFileIncludes = """
                #include <stdint.h> // required for count var
                """
            CFileIncludes = """
                // #include "your_header_here.h"
                """
            # IncludeGuardLabel = "{FILENAME}_H"

            [SmRunnerSettings]
            transpilerId = "C99"
            '/
            @enduml
            """", outputCleanActual: true);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.plantuml", Arg.Any<string>());
    }

    [Fact]
    public void LangCppC()
    {
        settings.TargetLanguageId = TargetLanguageId.CppC;
        Generator generator = new(settings);
        generator.tomlConfigType = TemplateLoader.TomlConfigType.Minimal;
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write("RocketSm.plantuml", Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff($$""""
            {{Top}}

            [RenderConfig]
            FileTop = """
                // Whatever you put in this `FileTop` section will end up 
                // being printed at the top of every generated code file.
                """
            AutoExpandedVars = """
                uint16_t count; // this var can be referenced in diagram
                """

            [RenderConfig.C]
            # CFileExtension = ".inc" # if you want to include sm in another file
            CFileExtension = ".cpp"
            HFileExtension = ".hpp"
            HFileIncludes = """
                #include <stdint.h> // required for count var
                """
            CFileIncludes = """
                // #include "your_header_here.h"
                """
            # IncludeGuardLabel = "{FILENAME}_H"

            [SmRunnerSettings]
            transpilerId = "C99"
            '/
            @enduml
            """", outputCleanActual: true);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.plantuml", Arg.Any<string>());
    }

    [Fact]
    public void LangCSharp()
    {
        settings.TargetLanguageId = TargetLanguageId.CSharp;
        Generator generator = new(settings);
        generator.tomlConfigType = TemplateLoader.TomlConfigType.Minimal;
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write("RocketSm.plantuml", Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff($""""
            {Top}

            [RenderConfig]
            FileTop = """
                // Whatever you put in this `FileTop` section will end up 
                // being printed at the top of every generated code file.
                """
            AutoExpandedVars = """
                int count; // this var can be referenced in diagram
                """

            [RenderConfig.CSharp]
            # NameSpace = ""
            # Usings = ""
            # BaseList = "MyUserBaseClass, IMyOtherUserInterface"
            # UseNullable = false
            # UsePartialClass = false

            [SmRunnerSettings]
            transpilerId = "CSharp"
            '/
            @enduml
            """", outputCleanActual: true);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.plantuml", Arg.Any<string>());
    }

    [Fact]
    public void LangJavaScript()
    {
        settings.TargetLanguageId = TargetLanguageId.JavaScript;
        Generator generator = new(settings);
        generator.tomlConfigType = TemplateLoader.TomlConfigType.Minimal;
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write("RocketSm.plantuml", Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff($""""
            {Top}

            [RenderConfig]
            FileTop = """
                // Whatever you put in this `FileTop` section will end up 
                // being printed at the top of every generated code file.
                """
            AutoExpandedVars = """
                count: 0, // this var can be referenced in diagram
                """

            [RenderConfig.JavaScript]
            # ExtendsSuperClass = "MyUserBaseClass"
            # UseExportOnClass = true

            [SmRunnerSettings]
            transpilerId = "JavaScript"
            '/
            @enduml
            """", outputCleanActual: true);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.plantuml", Arg.Any<string>());
    }
}
