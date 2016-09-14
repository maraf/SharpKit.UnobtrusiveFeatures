# SharpKit.UnobtrusiveFeatures

This repository contains plugin for [SharpKit](http://www.sharpkit.net) that enables export to javascript without referencing any SharpKit library. The goal behide is to have reference-clear projects, that are "shared" between server and client.

When developing client-side only library, typically UI, there is no need for removing reference to SharpKit. But when such library is re-used in the server project, defining export in SharpKit standard way requires reference to SharpKit, and so requires distributing this library with the server application and also loading the assembly to the AppDomain.

## Unobtrusive export

With the UnobtrusiveFeatures plugin there is only XML configuration file and build targets import in the csproj. No reference required for defining export to the javascript.

The configuration XML follows XSD and so VisualStudio offers intellisense for it. 

```XML
<UnobtrusiveFeatures xmlns="http://schemas.neptuo.com/xsd/sharpkit-unobtrusivefeatures.xsd">
    <Export Filename="Neptuo.js" FilenameFormat="~/bin/{0}" />
    <Namespace Target="Neptuo.Compilers" Export="False" />
    <Namespace Target="Neptuo.Security.Cryptography" Export="False" />
    <Namespace Target="Neptuo.Threading" Export="False" />
    <Type Target="Neptuo.Activators.DependencyServiceProvider" Export="False" />
    <Type Target="Neptuo.CodeDom.Compiler.CsCodeDomCompiler" Export="False" />
    <Type Target="Neptuo.Collections.Specialized.NameValueReadOnlyDictionary" Export="False" />
</UnobtrusiveFeatures>
```

Also, the configuration files can hierarchically inherited. If the project directory is for example `D:\Development\Framework\MyFramework\src`, in every directory on the path can be configuration file, named `SharpKit.UnobtrusiveFeatures.xml` or as currenly exporting assembly, eg: `MyFramework.xml`, that can contain some properties. These file are then in-memory-merged into a single configuration during compilation.

Also, the `Filename` and `FilenameFormat` attributes of `Export` element can contain placeholder for assembly name. We can define single configuration file in the root of all development projects, define like:

```XML
<UnobtrusiveFeatures xmlns="http://schemas.neptuo.com/xsd/sharpkit-unobtrusivefeatures.xsd">
    <Export FilenameFormat="~/bin/{0}" Filename="{AssemblyName}.js" />
</UnobtrusiveFeatures>
```

And this configuration file will export every SharpKit-enabled project to the file named as assembly in the bin folder of the project.

## Constructor export

The plugin also exports some C# artifacts that are not export the SharpKit it self. The first one is a `IsAbstract` flag, that contains information whether the class can be instantiated. 

The second one is a list of constructors defined in the class. With this feature, we can simply build IoC container with constructor injection. The export looks like:

```JSON
var TestSharpKit$UI$Presenter = {
    ...
    ctors: [{
        name: "ctor",
        parameters: ["TestSharpKit.UI.Views.MainView"]
    }],
    ...
}
```

# License

[Apache 2.0](blob/master/LICENSE)
