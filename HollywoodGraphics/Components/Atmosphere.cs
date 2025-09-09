using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using EFT.Weather;
using UnityEngine;

namespace HollywoodGraphics.Components;

public class Atmosphere
{
    private readonly WeatherController _weatherController;
    private readonly Gradient _defaultLightColors;

    public Atmosphere()
    {
        var weather = GameObject.Find("Weather");
        
        if (weather == null)
            return;
            
        _weatherController = weather.GetComponent<WeatherController>();
        
        _defaultLightColors = _weatherController.TimeOfDayController.LightColor;

        // Increase sun brightness a lot since the default value is not nearly enough to trigger a decent flare
        _weatherController.TOD_Sky_0.Sun.MeshBrightness = 10f;
        _weatherController.TOD_Sky_0.Moon.MeshSize = 0.5f;
        _weatherController.TOD_Sky_0.Moon.HaloSize = 0.5f;
        _weatherController.TOD_Sky_0.Moon.MeshBrightness = 2.5f;
        _weatherController.TOD_Sky_0.Moon.HaloColor = new Gradient()
        {
            alphaKeys =
            [
                new(1f, 0.0f),
                new(1f, 1f)
            ],
            colorKeys =
            [
                new GradientColorKey(new Color( 0.45f,  0.50f,  0.6f, 1f), 0.0f),
                new GradientColorKey(new Color( 0.45f,  0.50f,  0.6f, 1f), 1f)
            ]
        };
        
        // _weatherController.TOD_Sky_0.Day.LightIntensity = 0.75f;
        // _weatherController.TimeOfDayController.ScatteringBrightnessMultiplier = 0.75f;

        UpdateSettings();
    }

    public void UpdateSettings()
    {
        /* Original color curve:
         * [Info   :Janky's HollywoodFX] Light color time: 0 key: RGBA(0.809, 0.881, 1.000, 1.000)
           [Info   :Janky's HollywoodFX] Light color time: 0.5115129 key: RGBA(0.000, 0.000, 0.000, 1.000)
           [Info   :Janky's HollywoodFX] Light color time: 0.5266652 key: RGBA(1.000, 0.457, 0.322, 1.000)
           [Info   :Janky's HollywoodFX] Light color time: 0.5535668 key: RGBA(0.859, 0.631, 0.392, 1.000)
           [Info   :Janky's HollywoodFX] Light color time: 0.6971694 key: RGBA(1.000, 0.867, 0.537, 1.000)
           [Info   :Janky's HollywoodFX] Light color time: 0.9992523 key: RGBA(0.585, 0.530, 0.361, 1.000)
         */
        // 0 -> 0.31, 6 -> 0.46, 12 -> 0.9, 18 - 0.76
        if (Plugin.GraphicsConfig.SunColorEnabled.Value)
        {
            _weatherController.TimeOfDayController.LightColor = new Gradient()
            {
                alphaKeys =
                [
                    new GradientAlphaKey(1f, 0.0f),
                    new GradientAlphaKey(1f, 1f)
                ],
                colorKeys =
                [
                    FromConfig(Plugin.GraphicsConfig.SunColor1),
                    FromConfig(Plugin.GraphicsConfig.SunColor2),
                    FromConfig(Plugin.GraphicsConfig.SunColor3),
                    FromConfig(Plugin.GraphicsConfig.SunColor4),
                    FromConfig(Plugin.GraphicsConfig.SunColor5)
                ]
            };            
        }
        else
        {
            _weatherController.TimeOfDayController.LightColor = _defaultLightColors;
        }
    }

    private static GradientColorKey FromConfig(ConfigEntry<Vector4> config)
    {
        return new GradientColorKey(new Color(config.Value.x, config.Value.y, config.Value.z), config.Value.w);
    }
}
