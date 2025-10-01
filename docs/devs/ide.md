You don't need an IDE to compile, test and run C# code (see other .md files here), but it can be super helpful when working on something more involved and you want a debugger/intellisense.

There are a few different IDEs/extensions.

# 👉 vscode (fully free - no C# dev-kit) 👈
This is my general recommendation.

A solid IDE for C# development. Provides easy intellisense, code navigation, debugging, test running/debugging...

One click install.

* Rating: ⭐⭐⭐
* OS Support: all
* License: completely free
* Link: https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp

Cons:
* Doesn't work with C# script files (.csx). You can [re-enable globally](https://github.com/StateSmith/StateSmith/issues/221), or [potentially per workspace](https://github.com/dotnet/vscode-csharp/issues/6411#issuecomment-1756730824) (I haven't tried that yet).
* Lacks [syntax AST visualizer](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/syntax-visualizer?tabs=csharp), but you can use [sharplab.io online](https://sharplab.io/#v2:C4LghgzsA0AmIGoA+ABATARgLAChcoGYACdIgYSIG9cjaTiBLAO2CIA8iBeItNAbhp1CRZqwCeAnHSKDaw0UQDKAewC2AUwCy64AAtlsABQBKWVTPSUAdnZEERCWYC+uJ0A=).
* Lacks test explorer view, but you can use CLI for filtering tests to run
* Lacks some nice to have features



# vscode (C# dev-kit)
Adds more IDE features to vscode. Notably adds [Test Explorer view](https://code.visualstudio.com/docs/csharp/testing#_view-test-results).

Supposed to improve debugging, code completions/suggestions, ... not quite as good as full blown Visual Studio.

* Rating: ⭐⭐⭐⭐
* OS Support: all
* License: completely free for individuals (even commercial), free for open source, limits on commercial use in organizations.
* Link: https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit

Cons:
* License - although that doesn't affect StateSmith development as it is open source.
* Doesn't work with C# script files (.csx) [at all](https://github.com/StateSmith/StateSmith/issues/221).
* Lacks [syntax AST visualizer](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/syntax-visualizer?tabs=csharp), but you can use [sharplab.io online](https://sharplab.io/#v2:C4LghgzsA0AmIGoA+ABATARgLAChcoGYACdIgYSIG9cjaTiBLAO2CIA8iBeItNAbhp1CRZqwCeAnHSKDaw0UQDKAewC2AUwCy64AAtlsABQBKWVTPSUAdnZEERCWYC+uJ0A=).
* Lacks some nice to have features


# Visual Studio Community
Very full featured. Great [syntax AST visualizer](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/syntax-visualizer?tabs=csharp).

* Rating: ⭐⭐⭐⭐⭐
* OS Support: Windows only :(
* License: completely free for individuals (even commercial), free for open source, limits on commercial use in organizations.
* Link: https://visualstudio.microsoft.com/vs/community/

Cons:
* Windows only
* 7 GB download
* License - although that doesn't affect StateSmith development as it is open source.
* Doesn't work with C# script files? Fine for StateSmith development.


# JetBrains Rider
* Rating: ⭐⭐
* OS Support: all
* License: Free for non-commercial use

Shows a lot of promise as a rival to Visual Studio.

Cons:
* License - although that doesn't affect StateSmith development as it is open source.
* Fails to run StateSmith tests due to this [Rider bug](https://youtrack.jetbrains.com/issue/RSRP-495598).
* Even after implementing above workaround, wouldn't discover all tests.
