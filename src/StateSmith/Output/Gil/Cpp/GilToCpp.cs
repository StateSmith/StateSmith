#nullable enable

namespace StateSmith.Output.Gil.Cpp;

public class GilToCpp: IGilTranspiler
{
    private readonly ICodeFileWriter codeFileWriter;
    private readonly CppGilHelpers cppObjs;
    private readonly IOutputInfo outputInfo;

    public GilToCpp(IOutputInfo outputInfo, ICodeFileWriter codeFileWriter, CppGilHelpers cppGilHelpers)
    {
        this.outputInfo = outputInfo;
        this.codeFileWriter = codeFileWriter;
        this.cppObjs = cppGilHelpers;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        CppGilVisitor visitor = new(gilCode, cppObjs);
        visitor.Process();

        PostProcessor.PostProcess(visitor.hFileSb);
        PostProcessor.PostProcess(visitor.cFileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{visitor.MakeCFileName()}", code: visitor.cFileSb.ToString());
        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{visitor.MakeHFileName()}", code: visitor.hFileSb.ToString());
    }
}
