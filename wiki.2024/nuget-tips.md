Sometimes your computer caches the nuget package info and it takes while before it is willing to check again. You can force it to check the server again with this:

```
dotnet nuget locals http-cache --clear
```

You can also inspect the nuget local data with this:

```
dotnet nuget locals all --list
```
sample output:
```
http-cache: /home/afk/.local/share/NuGet/http-cache
global-packages: /home/afk/.nuget/packages/
temp: /tmp/NuGetScratchafk
plugins-cache: /home/afk/.local/share/NuGet/plugin-cache
```