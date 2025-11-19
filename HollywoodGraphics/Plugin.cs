using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace HollywoodGraphics;

[BepInPlugin("com.janky.hollywoodgraphics", "Janky-HollywoodGraphics", HollywoodGraphicsVersion)]
[SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Evident")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Plugin : BaseUnityPlugin
{
    public const string HollywoodGraphicsVersion = "1.1.2";
    public static ManualLogSource Log;

    public static GraphicsConfig GraphicsConfig;
    private static ConfigEntry<bool> _loggingEnabled;
    public static ConfigEntry<bool> TestEnabled;
    private static bool _amandsDetected;
    
    public static ConfigEntry<float> lensDustIntensity => GraphicsConfig.Bloom.DustIntensity;

    private void Awake()
    {
        Log = Logger;
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        // We wait for 5 seconds to allow all the 500 shonky mods (incl. this one) an average user installs to load 
        yield return new WaitForSeconds(5);
        
        if (Chainloader.PluginInfos.ContainsKey("com.Amanda.Graphics"))
        {
            Log.LogInfo("Amand's Graphics Mod detected, disabling HollywoodGraphics!");
            _amandsDetected = true;
        }
        
        SetupConfig();

        if (!_amandsDetected)
        {
            new LampControllerAwakePostfixPatch().Enable();
            new GraphicsRaidInitPatch().Enable();
            new GraphicsControllerInitPatch().Enable();
            new TerrainDetailOverridePatch().Enable();
        }

        if (GraphicsConfig.FogFixScattering.Value)
        {
            new RayleighScatterringBumpPatch().Enable();
        }

        Log.LogInfo("Initialization finished");

        if (_loggingEnabled.Value)
        {
            Log.LogInfo("Logging enabled");
        }
        else
        {
            Log.LogInfo("Logging disabled");
            BepInEx.Logging.Logger.Sources.Remove(Log);
        }
    }

    private static void LoadTemplateDrawer(ConfigEntryBase entry)
    {
        if (_amandsDetected)
        {
            GUILayout.TextArea("Amand's Graphics mod detected, HollywoodGraphics disabled!");
        }
        else
        {
            if (GUILayout.Button("Janky's Special"))
            {
                ConfigurationTemplates.SetJanky(entry.ConfigFile);
            }
            
            if (GUILayout.Button("Disable All"))
            {
                ConfigurationTemplates.SetDisabled(entry.ConfigFile);
            }

            if (GUILayout.Button("Defaults"))
            {
                ConfigurationTemplates.SetDefaults(entry.ConfigFile);
            }            
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
            "Use a preset template for the settings. Requires restarting the game to ensure all the settings take effect.",
            null,
            new ConfigurationManagerAttributes { Order = 1, CustomDrawer = LoadTemplateDrawer }
        ));

        /*
         * Graphics
         */
        GraphicsConfig = new GraphicsConfig(Config);

        // Turn everything readonly if Amand's is detected to avoid conflicts
        if (_amandsDetected)
        {
            foreach (var pair in Config)
            {
                foreach (var configTag in pair.Value.Description.Tags)
                {
                    if (configTag is ConfigurationManagerAttributes configAttrs)
                    {
                        configAttrs.ReadOnly = true;
                    }
                }
            }
        }
        
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