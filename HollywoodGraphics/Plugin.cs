using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace HollywoodGraphics;

[BepInPlugin("com.janky.hollywoodgraphics", "Janky's HollywoodGraphics", HollywoodGraphicsVersion)]
[SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Evident")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Plugin : BaseUnityPlugin
{
    public const string HollywoodGraphicsVersion = "1.0.0";

    public static ManualLogSource Log;

    public static GraphicsConfig GraphicsConfig;

    private static ConfigEntry<bool> _loggingEnabled;

    private void Awake()
    {
        Log = Logger;

        SetupConfig();

        new LampControllerAwakePostfixPatch().Enable();
        new GraphicsRaidInitPatch().Enable();
        new GraphicsControllerInitPatch().Enable();
        new TerrainDetailOverridePatch().Enable();

        Log.LogInfo("Initialization finished");

        if (_loggingEnabled.Value)
        {
            Log.LogInfo("Logging enabled");
        }
        else
        {
            Log.LogInfo("Logging disabled");
            BepInEx.Logging.Logger.Sources.Remove(Log);
        };
    }

    private static void LoadTemplateDrawer(ConfigEntryBase entry)
    {
        if (GUILayout.Button("Janky's Special"))
        {
            ConfigurationTemplates.SetJanky(entry.ConfigFile);
        }

        if (GUILayout.Button("Potato"))
        {
            ConfigurationTemplates.SetPotato(entry.ConfigFile);
        }

        if (GUILayout.Button("Defaults"))
        {
            ConfigurationTemplates.SetDefaults(entry.ConfigFile);
        }
    }

    private void SetupConfig()
    {
        const string general = "00. General";
        const string debug = "99. Debug";

        /*
         * General
         */
        Config.Bind(general, "Load Template (RESTART)", "", new ConfigDescription(
            "Use a preset template for the HFX settings. Requires restarting the game to ensure all the settings take effect.",
            null,
            new ConfigurationManagerAttributes { Order = 1, CustomDrawer = LoadTemplateDrawer }
        ));

        /*
         * Graphics
         */
        GraphicsConfig = new GraphicsConfig(Config);
        
        /*
         * Deboog
         */
        _loggingEnabled = Config.Bind(debug, "Enable Debug Logging (RESTART)", false, new ConfigDescription(
            "Duh. Requires restarting the game to take effect.",
            null,
            new ConfigurationManagerAttributes { Order = 1 }
        ));
    }
}