#nullable enable

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace StateSmith.Output.Gil;

#if !SS_SINGLE_FILE_APPLICATION

/// <summary>
/// This class is good when StateSmith is used as a library and not as a standalone single file executable.
/// When built as a single file executable, `.Assembly.Location` will not be available.
/// </summary>
internal class FileMetadataProvider : IRoslynMetadataProvider
{
    public IEnumerable<MetadataReference> GetSystemReferences()
    {
        yield return MetadataReference.CreateFromFile(typeof(string).Assembly.Location);
    }
}

#endif
