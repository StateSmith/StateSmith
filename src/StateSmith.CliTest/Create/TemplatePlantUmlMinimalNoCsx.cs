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
        State1: enter / count += 1;
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
                #include <stdint.h> // for count var
                """
            CFileIncludes = """
                // #include "your_header_here.h"
                """
            # IncludeGuardLabel = "{FILENAME}_H"

            # More C settings are available. See docs.

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
                #include <stdint.h> // for count var
                """
            CFileIncludes = """
                // #include "your_header_here.h"
                """
            # IncludeGuardLabel = "{FILENAME}_H"

            # More C settings are available. See docs.

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
    public void LangCpp()
    {
        settings.TargetLanguageId = TargetLanguageId.Cpp;
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

            [RenderConfig.Cpp]
            # HFileExtension = ".h"
            # IncludeGuardLabel = "{FILENAME}_H"
            # NameSpace = "MyNamespace"
            # BaseClassCode = "public: MyUserBaseClass"
            HFileIncludes = """
                #include <stdint.h> // for count var
                """
            CFileIncludes = """
                // #include "your_header_here.h"
                """

            # More Cpp settings are available. See docs.
            
            [SmRunnerSettings]
            transpilerId = "Cpp"
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

            # More CSharp settings are available. See docs.

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
            x.ArgAt<string>(1).ShouldBeShowDiff($$""""
            {{Top}}

            [RenderConfig]
            FileTop = """
                // Whatever you put in this `FileTop` section will end up 
                // being printed at the top of every generated code file.
                // You can use this section for imports:
                // import { SomeUserThing } from "./SomeUserThing.js";
                """
            AutoExpandedVars = """
                count: 0, // this var can be referenced in diagram
                """

            [RenderConfig.JavaScript]
            # ExtendsSuperClass = "MyUserBaseClass"
            # UseExportOnClass = true

            # More JavaScript settings are available. See docs.

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

    [Fact]
    public void LangTypeScript()
    {
        settings.TargetLanguageId = TargetLanguageId.TypeScript;
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
                // You can use this section for imports:
                // import { SomeUserThing } from "./SomeUserThing";
                """
            # TypeScript auto expanded var fields must end with a semicolon
            AutoExpandedVars = """
                public count: number = 0; // this var can be referenced in diagram
                """

            [RenderConfig.TypeScript]
            # Extends = "SomeUserBaseClass"
            # Implements = "SomeUserDefinedInterface"

            # More TypeScript settings are available. See docs.

            [SmRunnerSettings]
            transpilerId = "TypeScript"
            '/
            @enduml
            """", outputCleanActual: true);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.plantuml", Arg.Any<string>());
    }

    [Fact]
    public void LangJava()
    {
        settings.TargetLanguageId = TargetLanguageId.Java;
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
                int count = 0; // this var can be referenced in diagram
                """

            [RenderConfig.Java]
            # Package = "my.package.for.statemachine"
            Imports = """
                // whatever you need to import here
                """
            # Extends = "MyUserBaseClass"
            # Implements = "SomeUserInterface"

            # More Java settings are available. See docs.

            [SmRunnerSettings]
            transpilerId = "Java"
            '/
            @enduml
            """", outputCleanActual: true);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.plantuml", Arg.Any<string>());
    }


    [Fact]
    public void LangPython()
    {
        settings.TargetLanguageId = TargetLanguageId.Python;
        Generator generator = new(settings);
        generator.tomlConfigType = TemplateLoader.TomlConfigType.Minimal;
        generator.SetFileWriter(mockFileWriter);

        var pythonTop = Top;
        pythonTop = pythonTop.Replace(";", "");
        pythonTop = pythonTop.Replace("++", " += 1");

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write("RocketSm.plantuml", Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff($""""
            {pythonTop}

            [RenderConfig]
            FileTop = """
                # Whatever you put in this `FileTop` section will end up 
                # being printed at the top of every generated code file.
                """
            AutoExpandedVars = """
                self.count = 0  # this var can be referenced in diagram
                """
            
            [RenderConfig.Python]
            Imports = """
                # whatever you need to import
                # from some_module import some_function
                """
            Extends = "MyUserBaseClass"
            ClassCode = """
                # Add custom code here to inject into the generated class.
                # Inheritance or composition might be a better choice.
                """

            [SmRunnerSettings]
            transpilerId = "Python"
            '/
            @enduml
            """", outputCleanActual: true);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.plantuml", Arg.Any<string>());
    }
}
