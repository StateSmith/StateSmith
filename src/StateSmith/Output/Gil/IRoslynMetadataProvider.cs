#nullable enable

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace StateSmith.Output.Gil;

/// <summary>
/// This is required for when StateSmith.Cli is compiled into a single executable
/// and can't reference the system assemblies directly via files.
/// </summary>
public interface IRoslynMetadataProvider
{
    /// <summary>
    /// Allows C# predefined types to be used in the transpiler.
    /// </summary>
    IEnumerable<MetadataReference> GetSystemReferences();
}
