#nullable enable

using FluentAssertions;
using StateSmith.Output.UserConfig;
using Xunit;

namespace StateSmithTest.Output.UserConfig;

public class RenderConfigBaseVarsTest
{
    [Fact]
    public void SetFromRenderConfig()
    {
        RenderConfigBaseVars baseVars = new();
        var renderConfig = new RenderConfigFields()
        {
            FileTop = "FileTop",
            AutoExpandedVars = "AutoExpandedVars",
            DefaultVarExpTemplate = "DefaultVarExpTemplate",
            DefaultFuncExpTemplate = "DefaultFuncExpTemplate",
            DefaultAnyExpTemplate = "DefaultAnyExpTemplate",
            EventCommaList = "EventCommaList",
            VariableDeclarations = "VariableDeclarations",
            TriggerMap = "TriggerMap"
        };

        baseVars.SetFrom(renderConfig, false);

        int i = 0;
        i++; baseVars.FileTop.Should().Be("FileTop");
        i++; baseVars.AutoExpandedVars.Should().Be("AutoExpandedVars");
        i++; baseVars.DefaultVarExpTemplate.Should().Be("DefaultVarExpTemplate");
        i++; baseVars.DefaultFuncExpTemplate.Should().Be("DefaultFuncExpTemplate");
        i++; baseVars.DefaultAnyExpTemplate.Should().Be("DefaultAnyExpTemplate");
        i++; baseVars.EventCommaList.Should().Be("EventCommaList");
        i++; baseVars.VariableDeclarations.Should().Be("VariableDeclarations");
        i++; baseVars.TriggerMap.Should().Be("TriggerMap");
        TestHelper.ExpectFieldCount<RenderConfigBaseVars>(i);
    }

    [Fact]
    public void CopyFrom()
    {
        RenderConfigBaseVars baseVars = new();
        RenderConfigBaseVars otherBaseVars = new()
        {
            FileTop = "FileTop",
            AutoExpandedVars = "AutoExpandedVars",
            DefaultVarExpTemplate = "DefaultVarExpTemplate",
            DefaultFuncExpTemplate = "DefaultFuncExpTemplate",
            DefaultAnyExpTemplate = "DefaultAnyExpTemplate",
            EventCommaList = "EventCommaList",
            VariableDeclarations = "VariableDeclarations",
            TriggerMap = "TriggerMap"
        };

        baseVars.CopyFrom(otherBaseVars);

        int i = 0;
        i++; baseVars.FileTop.Should().Be("FileTop");
        i++; baseVars.AutoExpandedVars.Should().Be("AutoExpandedVars");
        i++; baseVars.DefaultVarExpTemplate.Should().Be("DefaultVarExpTemplate");
        i++; baseVars.DefaultFuncExpTemplate.Should().Be("DefaultFuncExpTemplate");
        i++; baseVars.DefaultAnyExpTemplate.Should().Be("DefaultAnyExpTemplate");
        i++; baseVars.EventCommaList.Should().Be("EventCommaList");
        i++; baseVars.VariableDeclarations.Should().Be("VariableDeclarations");
        i++; baseVars.TriggerMap.Should().Be("TriggerMap");
        TestHelper.ExpectFieldCount<RenderConfigBaseVars>(i);
    }
}
