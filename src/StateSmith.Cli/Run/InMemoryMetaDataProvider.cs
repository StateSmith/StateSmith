using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using StateSmith.Output.Gil;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

/// <summary>
/// This allows the StateSmith.Cli to be compiled into a single executable.
/// For that to work, we can't rely on `MetadataReference.CreateFromFile(typeof(string).Assembly.Location)`
/// because `.Assembly.Location` will not be available.
/// </summary>
public class InMemoryMetaDataProvider : IRoslynMetadataProvider
{
    public IEnumerable<MetadataReference> GetSystemReferences()
    {
        return ReferenceAssemblies.Net80;
    }
}
