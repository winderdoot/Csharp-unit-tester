using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This class inherits from <see cref="AssemblyLoadContext"/> and it allows for recursive loading of dependency assemblies.
/// <para> Notably, it avoids loading in assemblies that are already present in the calling assembly context. </para>
/// </summary>
/// <remarks>
/// <b>NOTICE!</b> - This class is almost word for word copied from an example provided by the lecturer:
/// <see href="https://github.com/tomasz-herman/Programming3/blob/master/Assembly/Plugin.Host/PluginLoadContext.cs"/>
/// </remarks>
class MyContextLoader : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly AssemblyDependencyResolver _runningAssemblyResolver;

    public MyContextLoader(string assemblyPath, bool collectible = true)
        : base(name: assemblyPath, isCollectible: collectible)
    {
        _resolver = new AssemblyDependencyResolver(assemblyPath);
        _runningAssemblyResolver = new AssemblyDependencyResolver(Assembly.GetCallingAssembly().Location);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? runningAssemblyPath = _runningAssemblyResolver.ResolveAssemblyToPath(assemblyName);
        if (runningAssemblyPath != null)
        {
            return null;
        }
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    //protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    //{
    //    string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
    //    if (libraryPath != null)
    //    {
    //        return LoadUnmanagedDllFromPath(libraryPath);
    //    }

    //    return IntPtr.Zero;
    //}
}
