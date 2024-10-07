using System.IO;

namespace StateSmith.CliTest;

public static class TestHelper
{
    public static string GetThisDir([System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        return Path.GetDirectoryName(callerFilePath) + "/";
    }

    /// <summary>
    /// This is a helper method to get around NSubstitute matcher limitations.
    /// Can't use a generic lambda in the Arg.Is method. Must be an expression.
    /// See https://github.com/nsubstitute/NSubstitute/issues/637
    /// Use to capture argument and then inspect it later.
    /// 
    /// Use like:
    /// <code><![CDATA[ManifestData? data = null;
    /// manifestPersistence.Received(1).Write(Arg.Is<ManifestData>(md => Capture(md, out data)), overWrite: true);]]>
    /// </code>
    /// </summary>
    public static bool Capture<T>(T input, out T output)
    {
        output = input;
        return true;
    }
}
