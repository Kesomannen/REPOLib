using BepInEx;
using BepInEx.Configuration;
using REPOLib.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace REPOLib;

internal static class ConfigManager
{
    public static ConfigFile MainConfigFile { get; private set; }

    private static readonly Dictionary<string, ConfigFile> _contentConfigFiles = new();
    
    public static ConfigEntry<bool> ExtendedLogging { get; private set; }
    public static ConfigEntry<bool> DeveloperMode { get; private set; }

    public static void Initialize(ConfigFile configFile)
    {
        TomlTypeConverter.AddConverter(typeof(ConfigList),
            new TypeConverter
            {
                ConvertToString = (obj, _) =>
                {
                    var list = (ConfigList)obj;
                    return string.Join(',', list.Items);
                },
                ConvertToObject = (str, _) => new ConfigList(str.Split(',').ToList())
            });
        
        MainConfigFile = configFile;
        BindGeneralConfigs();
    }

    private static void BindGeneralConfigs()
    {
        ExtendedLogging = MainConfigFile.Bind("General", "ExtendedLogging", defaultValue: false, "Enable extended logging.");
        DeveloperMode = MainConfigFile.Bind("General", "DeveloperMode", defaultValue: false, "Enable developer mode cheats for testing.");
    }

    private static string CleanName(string name)
    {
        return name
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("[", "")
            .Replace("]", "");
    }

    public static void BindValuableConfig(string modName, ValuableObject valuableObject, ref List<string> presetNames)
    {
        if (!_contentConfigFiles.TryGetValue(modName, out var file))
        {
            string path = Path.Combine(Paths.ConfigPath, "REPOLib", modName + ".cfg");
            file = new ConfigFile(path, saveOnInit: false);
            _contentConfigFiles.Add(modName, file);
        }

        string name = valuableObject.gameObject.name;
        string section = CleanName(name);

        var defaultValue = valuableObject.valuePreset;
        
        ConfigEntry<float> minValueConfig = file.Bind(
            section,
            "MinValue",
            defaultValue: defaultValue.valueMin,
            ""
        );
        
        ConfigEntry<float> maxValueConfig = file.Bind(
            section,
            "MaxValue",
            defaultValue: defaultValue.valueMax,
            ""
        );

        if (
            !Mathf.Approximately(minValueConfig.Value, defaultValue.valueMin) ||
            !Mathf.Approximately(maxValueConfig.Value, defaultValue.valueMax)
        )
        {
            Logger.LogInfo($"Creating custom value preset for {name} ({modName})", extended: true);
            
            var value = ScriptableObject.CreateInstance<Value>();
            value.name = $"{name}_CustomValuePreset";
            value.valueMin = minValueConfig.Value;
            value.valueMax = maxValueConfig.Value;
            valuableObject.valuePreset = value;
        }

        var defaultDurability = valuableObject.durabilityPreset;
        
        ConfigEntry<float> durabilityConfig = file.Bind(
            section,
            "Durability",
            defaultValue: defaultDurability.durability,
            ""
        );
        
        ConfigEntry<float> fragilityConfig = file.Bind(
            section,
            "Fragility",
            defaultValue: defaultDurability.fragility,
            ""
        );
        
        if (
            !Mathf.Approximately(durabilityConfig.Value, defaultDurability.durability) ||
            !Mathf.Approximately(fragilityConfig.Value, defaultDurability.fragility)
        )
        {
            Logger.LogInfo($"Creating custom durability preset for {name} ({modName})", extended: true);
            
            var value = ScriptableObject.CreateInstance<Durability>();
            value.name = $"{name}_CustomDurabilityPreset";
            value.durability = durabilityConfig.Value;
            value.fragility = fragilityConfig.Value;
            valuableObject.durabilityPreset = value;
        }
        
        ConfigEntry<ConfigList> presetsConfig = file.Bind(
            section, 
            "Presets", 
            defaultValue: new ConfigList(presetNames), 
            ""
        );
        
        presetNames = presetsConfig.Value.Items;
    }
}
