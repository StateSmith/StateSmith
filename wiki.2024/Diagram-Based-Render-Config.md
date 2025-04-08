# deprecated
See [toml settings](https://github.com/StateSmith/StateSmith/blob/main/docs/settings.md) instead.

This is now deprecated. It is still supported, but new render config settings will not be added to it.

---

Version 0.7.7 added support for diagram based render config settings as shown below. You can either place a `$RenderConfig` group at the document root (orange group), or directly inside a state machine (blue group). The orange group will apply to both `MySM1` and `MySm2`. The blue RenderConfig group will only apply to `MySm1`.

![image](https://user-images.githubusercontent.com/274012/216792278-cdd0763b-ce76-4a51-bde4-f40d655f1443.png)

Within a `$RenderConfig` group, you can add config options which can be expanded or collapsed.

![image](https://user-images.githubusercontent.com/274012/233734420-cd15eeb9-71f9-4091-b6e8-5de4af1fee2b.png)



<br>

# Language Specific Options

## C
StateSmith first launched with `C` support so those options don't need a prefix, but all other languages do.

Equivalency Mapping:
```
$CONFIG: HFileTop      <---> IRenderConfigC.HFileTop
$CONFIG: HFileIncludes <---> IRenderConfigC.HFileIncludes
$CONFIG: CFileTop      <---> IRenderConfigC.CFileTop
$CONFIG: CFileIncludes <---> IRenderConfigC.CFileIncludes
```

## C#
For example, you could write `string IRenderConfigCSharp.NameSpace => "LightController;";` in your .csx based RenderConfig, or you could do it in a diagram with:

![image](https://user-images.githubusercontent.com/274012/229974832-6e07eeb9-a723-4b35-bfc2-e98e37d44bde.png)

Equivalency Mapping:
```
$CONFIG: CSharpNameSpace       <---> IRenderConfigCSharp.NameSpace
$CONFIG: CSharpUsings          <---> IRenderConfigCSharp.Usings
$CONFIG: CSharpClassCode       <---> IRenderConfigCSharp.ClassCode
$CONFIG: CSharpBaseList        <---> IRenderConfigCSharp.BaseList
$CONFIG: CSharpUseNullable     <---> IRenderConfigCSharp.UseNullable
$CONFIG: CSharpUsePartialClass <---> IRenderConfigCSharp.UsePartialClass
```

## JavaScript
Same prefixing idea as C#.

Equivalency Mapping:
```
$CONFIG: JavaScriptClassCode         <---> IRenderConfigJavaScript.ClassCode
$CONFIG: JavaScriptExtendsSuperClass <---> IRenderConfigJavaScript.ExtendsSuperClass
$CONFIG: JavaScriptPrivatePrefix     <---> IRenderConfigJavaScript.PrivatePrefix
$CONFIG: JavaScriptUseExportOnClass  <---> IRenderConfigJavaScript.UseExportOnClass
```

<br>

<br/>
<br/>

Issue: https://github.com/StateSmith/StateSmith/issues/23
