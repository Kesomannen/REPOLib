using BepInEx.Bootstrap;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

namespace REPOLib.Modules;

public interface IContentSource
{
    string Name { get; }
    string Version { get; }
    string Guid { get; }
}

internal class AssemblyContentSource : IContentSource
{
    public Assembly Assembly { get; }

    public string Name { get; }
    public string Version { get; }
    public string Guid { get; }
    
    public AssemblyContentSource(Assembly assembly)
    {
        Assembly = assembly;
        
        var pluginInfo = Chainloader.PluginInfos.FirstOrDefault(kvp => kvp.Value.Location == assembly.Location).Value;
            
        if (pluginInfo != null)
        {
            Name = pluginInfo.Metadata.Name;
            Version = pluginInfo.Metadata.Version.ToString();
            Guid = pluginInfo.Metadata.GUID;
        }
        else
        {
            var assemblyName = assembly.GetName();
            Logger.LogWarning($"Failed to get BepInEx plugin info from {assemblyName.FullName}");
            
            Name = assemblyName.Name;
            Version = assemblyName.Version.ToString();
            Guid = assemblyName.FullName;
        }
    }
}

public static class ContentRegistry
{
    private static readonly Dictionary<Assembly, AssemblyContentSource> _assemblySources = new();
    
    // Object here is different depending on the type of content;
    // - For enemies, it's the EnemySetup scriptable object
    // - For valuables, the ValuableObject component on the prefab
    // - For items, the Item scriptable object
    private static readonly Dictionary<IContentSource, List<Object>> _registry = new();

    internal static AssemblyContentSource GetAssemblySource(Assembly assembly)
    {
        if (_assemblySources.TryGetValue(assembly, out var source)) return source;
        
        source = new AssemblyContentSource(assembly);
        _assemblySources.Add(assembly, source);

        return source;
    }

    internal static void Add(Object obj, IContentSource source)
    {
        if (_registry.TryGetValue(source, out List<Object> prefabs))
        {
            prefabs.Add(obj);
        }
        else
        {
            _registry.Add(source, [obj]);
        }
    }
    
    [CanBeNull]
    public static IContentSource GetSource(string guid)
    {
        return _registry.Keys.FirstOrDefault(kvp => kvp.Guid == guid);
    }
    
    public static IEnumerable<(IContentSource Source, IReadOnlyList<Object> Content)> GetAll()
    {
        return _registry.Select(kvp => (Source: kvp.Key, Content: (IReadOnlyList<Object>)kvp.Value));
    }
    
    internal static IEnumerable<(IContentSource, IEnumerable<T>)> GetAll<T>()
    {
        return _registry.Select(kvp => (kvp.Key, kvp.Value.OfType<T>()));
    }

    public static IReadOnlyList<Object> GetAllFrom(IContentSource source)
    {
        return _registry.GetValueOrDefault(source);
    }

    internal static IEnumerable<T> GetAllFrom<T>(IContentSource source)
    {
        return GetAllFrom(source).OfType<T>();
    }

    internal static IContentSource GetSourceInternal(Object obj)
    {
        return _registry.FirstOrDefault(kvp => kvp.Value.Contains(obj)).Key;
    }

    public static IEnumerable<(IContentSource Source, IEnumerable<ValuableObject> Valuables)> GetAllValuables() => GetAll<ValuableObject>();
    public static IEnumerable<ValuableObject> GetAllValuablesFrom(IContentSource source) => GetAllFrom<ValuableObject>(source);
    [CanBeNull]
    public static IContentSource GetSource(ValuableObject valuable) => GetSourceInternal(valuable);
    
    public static IEnumerable<(IContentSource Source, IEnumerable<Item> Items)> GetAllItems() => GetAll<Item>();
    public static IEnumerable<Item> GetAllItemsFrom(IContentSource source) => GetAllFrom<Item>(source);
    [CanBeNull]
    public static IContentSource GetSource(Item item) => GetSourceInternal(item);
    
    public static IEnumerable<(IContentSource Source, IEnumerable<EnemySetup> Enemies)> GetAllEnemies() => GetAll<EnemySetup>();
    public static IEnumerable<EnemySetup> GetAllEnemiesFrom(IContentSource source) => GetAllFrom<EnemySetup>(source);
    [CanBeNull]
    public static IContentSource GetSource(EnemySetup enemy) => GetSourceInternal(enemy);
}
