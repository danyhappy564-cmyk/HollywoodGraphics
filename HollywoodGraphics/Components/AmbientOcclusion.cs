namespace HollywoodGraphics.Components;

public class AmbientOcclusion
{
    private readonly HBAO _hbao;

    public AmbientOcclusion()
    {
        var camera = CameraClass.Instance?.Camera;

        if (camera == null)
        {
            Plugin.Log.LogError("AmbientOcclusion: No camera found!");
            return;
        }
        
        _hbao = camera.GetComponent<HBAO>();
    }

    public void UpdateSettings()
    {
        var settings = _hbao.aoSettings;
        settings.intensity = Plugin.GraphicsConfig.AOIntensity.Value;
        settings.radius =  Plugin.GraphicsConfig.AORadius.Value;
        settings.bias =  Plugin.GraphicsConfig.AOBias.Value;
        settings.useMultiBounce = Plugin.GraphicsConfig.AOMultiBounceEnabled.Value;
        settings.multiBounceInfluence = Plugin.GraphicsConfig.AOMultiBounceInfluence.Value;
        
        var colorbleedSettings = _hbao.colorBleedingSettings;
        colorbleedSettings.enabled = Plugin.GraphicsConfig.AOColorBleedEnabled.Value;
        colorbleedSettings.saturation = Plugin.GraphicsConfig.AOColorBleedSaturation.Value;
        colorbleedSettings.albedoMultiplier = Plugin.GraphicsConfig.AOColorBleedAlbedoMul.Value;
    }
}