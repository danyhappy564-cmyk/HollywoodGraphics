using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using GPUInstancer;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HollywoodGraphics;

public class GraphicsControllerInitPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(GameWorld __instance)
    {
        var graphicsController = __instance.gameObject.AddComponent<GraphicsController>();
        Singleton<GraphicsController>.Create(graphicsController);
    }
}

public class GraphicsRaidInitPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_37));
    }

    [PatchPrefix]
    // ReSharper disable once InconsistentNaming
    public static void Prefix(RaidSettings ____raidSettings)
    {
        var mapName = ____raidSettings.LocationId.ToLower();
        Plugin.GraphicsConfig.SetCurrentMap(mapName);
        
        var overrides = Plugin.GraphicsConfig.Current;
        var message = $"Graphics overrides map: {mapName} - {overrides.Name} enabled: {overrides.Enabled.Value}";
        Plugin.Log.LogInfo(message);
    }
}

public class LampControllerAwakePostfixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LampController).GetMethod(nameof(LampController.Awake));
    }

    [PatchPrefix]
    // ReSharper disable InconsistentNaming
    public static void Prefix(LampController __instance, MultiFlareLight[] ___MultiFlareLights, MaterialEmission[] ____materialsWithEmission)
    {
        if (!Plugin.GraphicsConfig.LightFlareEnabled.Value)
            return;
        
        // Plugin.Log.LogInfo($"Found light: {__instance.name} lights: {___MultiFlareLights} alights: {__instance.CustomLights.Length}");
        
        foreach (var flareLight in ___MultiFlareLights)
        {
            // Plugin.Log.LogInfo($"Flare light: {__instance.name} alpha {flareLight.Alpha} scale {flareLight.Scale} flares {flareLight.Flares.Count}");

            // Apply a floor on the alpha and compress the range with an sqrt
            flareLight.Alpha *= Plugin.GraphicsConfig.LightFlareIntensity.Value;

            var scaleField = Traverse.Create(flareLight).Field("_scale");

            // ReSharper disable once HeapView.BoxingAllocation
            scaleField.SetValue(scaleField.GetValue<float>() * Plugin.GraphicsConfig.LightFlareSize.Value);
        }
    }
}

public class TerrainDetailOverridePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GPUInstancerDetailManager).GetMethod(nameof(GPUInstancerDetailManager.Awake));
    }

    [PatchPostfix]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(GPUInstancerDetailManager __instance)
    {
        var overrides = Plugin.GraphicsConfig.Current;
        
        Plugin.Log.LogInfo(
            $"Terrain detail overrides: {overrides.Name} enabled: {overrides.Enabled.Value} terrain: {__instance.terrain.name} DistT: {__instance.terrain.detailObjectDistance} Dist: {__instance.terrainSettings.maxDetailDistance} DistL: {__instance.terrainSettings.maxDetailDistanceLegacy} Dens: {__instance.terrainSettings.detailDensity}"
        );
        
        if (!overrides.Enabled.Value)
            return;

        var terrainDetailDistance = overrides.DetailDistance.Value;
        
        // __instance.terrain.detailObjectDistance *= Plugin.TerrainDetailDistance.Value;

        __instance.terrainSettings.maxDetailDistance *= terrainDetailDistance;
        __instance.terrainSettings.maxDetailDistanceLegacy *= terrainDetailDistance;

        foreach (var prototype in __instance.prototypeList)
        {
            var detailPrototype = prototype as GPUInstancerDetailPrototype;

            if (detailPrototype != null)
            {
                Plugin.Log.LogInfo($"DetailProto: {prototype.name} {prototype.maxDistance} {detailPrototype.densityFadeFactor}");
                detailPrototype.maxDistance *= terrainDetailDistance;
                detailPrototype.lodBiasAdjustment = terrainDetailDistance;
                detailPrototype.densityFadeFactor /= overrides.DetailDensity.Value;
            }
            else
            {
                Plugin.Log.LogInfo($"Proto: {prototype.name} {prototype.maxDistance}");
                prototype.maxDistance *= terrainDetailDistance;
                prototype.lodBiasAdjustment *= terrainDetailDistance;
            }
        }
    }
}