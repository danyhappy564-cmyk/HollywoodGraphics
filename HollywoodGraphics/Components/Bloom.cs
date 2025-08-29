using System;
using EFT.Weather;
using UnityEngine;

namespace HollywoodGraphics.Components;

public class Bloom
{
    private float _sunLightFactor = 10000f;
    
    private readonly UltimateBloom _ultimateBloom;
    private readonly WeatherController _weatherController;

    public Bloom()
    {
        // Find the main camera
        var targetCamera = CameraClass.Instance?.Camera;

        if (targetCamera == null)
        {
            Plugin.Log.LogError("UltimateBloomController: No camera found!");
            return;
        }

        // Check if Ultimate Bloom is already on the camera
        _ultimateBloom = targetCamera.GetComponent<UltimateBloom>();

        if (_ultimateBloom == null)
        {
            // Add Ultimate Bloom component to camera
            _ultimateBloom = targetCamera.gameObject.AddComponent<UltimateBloom>();
            Plugin.Log.LogInfo("UltimateBloomController: Added Ultimate Bloom component to camera");
        }

        _ultimateBloom.m_IntensityManagement = UltimateBloom.BloomIntensityManagement.FilmicCurve;
        _ultimateBloom.m_SamplingMode = UltimateBloom.SamplingMode.HeightRelative;
        _ultimateBloom.m_SamplingMinHeight = 768;
        // Reduces flicker
        _ultimateBloom.m_AnamorphicSmallVerticalBlur = true;

        Plugin.Log.LogInfo("Resetting Main Bloom intensities");
        ResetIntensities(_ultimateBloom.m_BloomIntensities);
        Plugin.Log.LogInfo("Resetting Anamorphic Bloom intensities");
        ResetIntensities(_ultimateBloom.m_AnamorphicBloomIntensities);
        Plugin.Log.LogInfo("Resetting Star Bloom intensities");
        ResetIntensities(_ultimateBloom.m_StarBloomIntensities);
        
        // Turn these off as they form the "blob" part of the bloom and can oversaturate things.
        _ultimateBloom.m_BloomUsages[0] = _ultimateBloom.m_BloomUsages[1] = false;
        _ultimateBloom.m_AnamorphicBloomUsages[0] = false;
        _ultimateBloom.m_AnamorphicBloomUsages[1] = true;
        _ultimateBloom.m_StarBloomUsages[0] = false;

        // Disable high order star blooms because they end up applying everywhere on the screen
        for (var i = 3; i < _ultimateBloom.m_StarBloomUsages.Length; i++)
        {
            _ultimateBloom.m_StarBloomUsages[i] = false;
        }
        
        Plugin.GraphicsConfig.Bloom.ApplyConfig(_ultimateBloom);
        Plugin.GraphicsConfig.Bloom.ApplyLensDust(_ultimateBloom);
        
        Plugin.GraphicsConfig.Bloom.ConfigChanged += UpdateSettings;
        Plugin.GraphicsConfig.Bloom.LensDirtChanged += UpdateLensDirt;
        Plugin.Log.LogInfo($"UltimateBloomController: Ultimate Bloom effect applied to camera {targetCamera.name}");

        var weather = GameObject.Find("Weather");
        
        if (weather == null)
            return;
        
        _weatherController = weather.GetComponent<WeatherController>();
    }

    public void Update()
    {
        if (_weatherController == null)
            return;

        var nightFactor = Mathf.InverseLerp(0f, -0.1f, _weatherController.SunHeight);
        
        if (Mathf.Abs(nightFactor - _sunLightFactor) < 0.05f)
            return;

        var bloomConfig = Plugin.GraphicsConfig.Bloom;
        // Decrease streak size at night
        var streakScale = 1f - 0.5f * nightFactor;
        _ultimateBloom.m_AnamorphicScale = bloomConfig.AnamorphicScale.Value * streakScale;
        _ultimateBloom.m_DirtLightIntensity = bloomConfig.DirtLightIntensity.Value + nightFactor;
        _ultimateBloom.m_StarFlareIntensity = bloomConfig.DirtLightIntensity.Value + 0.5f * nightFactor;

        var highlightScaling = 1f + 0.1f * nightFactor;
        _ultimateBloom.SetFilmicCurveParameters(
            bloomConfig.BloomMid.Value,
            bloomConfig.BloomDark.Value,
            bloomConfig.BloomBright.Value,
            bloomConfig.BloomHighlight.Value * highlightScaling
        );

        _sunLightFactor = nightFactor;
    }
    
    public void Destroy()
    {
        Plugin.GraphicsConfig.Bloom.ConfigChanged -= UpdateSettings;
        Plugin.GraphicsConfig.Bloom.LensDirtChanged -= UpdateLensDirt;
    }
    
    private static void ResetIntensities(float[] intensities)
    {
        for (var i = 0; i < intensities.Length; i++)
        {
            Plugin.Log.LogInfo($"Intensity: {intensities[i]}");
            intensities[i] = 1f;
        }
    }
    
    private void UpdateSettings(object sender, EventArgs e)
    {
        Plugin.GraphicsConfig.Bloom.ApplyConfig(_ultimateBloom);

        // Force the recalculation of the sunlight factor
        _sunLightFactor = 10000f;
    }

    private void UpdateLensDirt(object sender, EventArgs e)
    {
        Plugin.GraphicsConfig.Bloom.ApplyLensDust(_ultimateBloom);
    }
}