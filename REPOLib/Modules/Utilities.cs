using REPOLib.Extensions;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace REPOLib.Modules;

public static class Utilities
{
    private static readonly List<GameObject> _prefabsToFix = [];
    private static readonly List<GameObject> _fixedPrefabs = [];
    
    internal static void FixAudioMixerGroupsOnPrefabs()
    {
        foreach (var prefab in _prefabsToFix)
        {
            prefab.FixAudioMixerGroups();
            _fixedPrefabs.Add(prefab);
        }

        _prefabsToFix.Clear();
    }

    /// <summary>
    /// Fixes the audio mixer groups of all audio sources in the game object and its children.
    /// Assigns the audio mixer group based on the audio group found assigned in the prefab.
    /// - Vyrus + Zehs
    /// </summary>
    public static void FixAudioMixerGroups(GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (_prefabsToFix.Contains(prefab) || _fixedPrefabs.Contains(prefab))
        {
            return;
        }

        if (AudioManager.instance == null)
        {
            _prefabsToFix.Add(prefab);
            return;
        }

        prefab.FixAudioMixerGroups();
        _fixedPrefabs.Add(prefab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetCallingAssemblyName()
    {
        return Assembly.GetCallingAssembly().GetName().Name;
    }
}