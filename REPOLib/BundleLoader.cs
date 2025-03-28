﻿using REPOLib.Objects.Sdk;
using System;
using System.IO;
using UnityEngine;

namespace REPOLib;

public static class BundleLoader
{
    public static void LoadAllBundles(string root, string withExtension)
    {
        string[] files = Directory.GetFiles(root, "*" + withExtension, SearchOption.AllDirectories);

        foreach (string path in files)
        {
            string relativePath = path.Replace(root, "");
            try
            {
                LoadBundle(path, relativePath);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load bundle at {relativePath}: {e}");
            }
        }
    }

    public static void LoadBundle(string path, string relativePath)
    {
        var bundle = AssetBundle.LoadFromFile(path);
        Mod[] mods = bundle.LoadAllAssets<Mod>();

        switch (mods.Length)
        {
            case 0:
                throw new Exception("Bundle contains no mods.");
            case > 1:
                throw new Exception("Bundle contains more than one mod.");
        }

        var mod = mods[0];
        
        Logger.LogInfo($"Loading content from bundle at {relativePath} ({mod.Identifier})");
        
        Content[] contents = bundle.LoadAllAssets<Content>();
        
        foreach (var content in contents)
        {
            try
            {
                content.Initialize(mod);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load {content.Name} ({content.GetType().Name}): {e}");
            }
        }
    }
}
