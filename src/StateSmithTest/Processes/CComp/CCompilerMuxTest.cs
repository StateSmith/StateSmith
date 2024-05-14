#nullable enable

using FluentAssertions;
using System;
using Xunit;

namespace StateSmithTest.Processes.CComp;

public class CCompilerMuxTest
{
    readonly FakeEnvironmentVarProvider fakeEnvProvider = new();
    readonly CCompilerMux cCompilerMux;

    public CCompilerMuxTest()
    {
        cCompilerMux = new CCompilerMux(fakeEnvProvider);
    }

    [Fact]
    public void TestEnvVarGcc()
    {
        SetCompIdEnvVar("gcc");
        TestAndExpectId(CCompilerId.GCC);

        SetCompIdEnvVar("GCC");
        TestAndExpectId(CCompilerId.GCC);
    }

    [Fact]
    public void TestEnvVarWslGcc()
    {
        SetCompIdEnvVar("wsl_gcc");
        TestAndExpectId(CCompilerId.WSL_GCC);

        SetCompIdEnvVar("WSL_GCC");
        TestAndExpectId(CCompilerId.WSL_GCC);
    }

    [Fact]
    public void TestEnvVarClang()
    {
        SetCompIdEnvVar("clang");
        TestAndExpectId(CCompilerId.CLANG);

        SetCompIdEnvVar("CLANG");
        TestAndExpectId(CCompilerId.CLANG);
    }

    [Fact]
    public void TestEnvVarMsvc()
    {
        SetCompIdEnvVar("msvc");
        TestAndExpectId(CCompilerId.MSVC);

        SetCompIdEnvVar("MSVC");
        TestAndExpectId(CCompilerId.MSVC);
    }

    [Fact]
    public void TestEnvVarUnknownCompiler()
    {
        SetCompIdEnvVar("zig");
        Action a = () => cCompilerMux.PrepareForCompilation(NewRequest());
        a.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TestEnvVarBinPath()
    {
        SetCompIdEnvVar("GCC");
        SetCompPathEnvVar("/some/path/to/bin");
        CCompRequest request = NewRequest();
        CCompilerId compilerId = cCompilerMux.PrepareForCompilation(request);

        compilerId.Should().Be(CCompilerId.GCC);
        request.CompilerPath.Should().Be("/some/path/to/bin");
    }

    //[Fact]
    //public void TestRealEnvVars()
    //{
    //    var evp = new EnvironmentVarProvider();
    //    evp.CompilerId.Should().Be("clang");
    //}

    private void TestAndExpectId(CCompilerId expected)
    {
        CCompRequest request = NewRequest();
        CCompilerId compilerId = cCompilerMux.PrepareForCompilation(request);
        compilerId.Should().Be(expected);
    }

    private void SetCompPathEnvVar(string path)
    {
        fakeEnvProvider.CompilerPath = path;
    }

    private static CCompRequest NewRequest()
    {
        return new() { SourceFiles = [] };
    }

    private void SetCompIdEnvVar(string idName)
    {
        fakeEnvProvider.CompilerId = idName;
    }

    public class FakeEnvironmentVarProvider : IEnvironmentVarProvider
    {
        public string? CompilerId { get; set; }
        public string? CompilerPath { get; set; }
    }
}
