using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HollywoodGraphics.Components;

public class AmbientOcclusion
{
    private readonly Camera _camera;
    private readonly HBAO _hbao;
    private readonly HBAO_Core.AOSettings _defaultAOSettings;
    private readonly HBAO_Core.ColorBleedingSettings _defaultColorBleedingSettings;

    // (field report: night vision reads darker than it should) image effects run in
    // component order, and HBAO sits ahead of NightVision on the camera - so ambient
    // occlusion darkens crevices/shadows in the frame BEFORE night vision amplifies
    // it, leaving the goggles with less to work with than intended. no safe runtime
    // API exists to reorder components (that's editor-only, and would touch every
    // other effect after HBAO too), so instead: suppress AO outright while NVG is
    // actually on, and hand it straight back the moment it's off.
    private BSG.CameraEffects.NightVision _nightVision;
    private bool _nightVisionSearched;
    private bool _suppressedForNvg;

    // FIELD-DUMP DIAGNOSTIC (2026-09): two field reports confirmed .enabled fires on
    // toggle-ON but never fires on toggle-OFF (HBAO stayed suppressed the whole raid
    // after one NVG use) - .enabled tracks something like "NVG gear is equipped",
    // not "the goggles are actually flipped down/active right now". Since the real
    // signal's name isn't known without a decompile, walk every bool field on the
    // component by reflection and log whichever ones actually change value when the
    // player toggles NVG - that tells us the right field to switch to.
    private FieldInfo[] _nvBoolFields;
    private bool[] _nvBoolLast;
    private float _nextNvFieldPoll;

    public AmbientOcclusion()
    {
        var camera = CameraClass.Instance?.Camera;

        if (camera == null)
        {
            Plugin.Log.LogError("AmbientOcclusion: No camera found!");
            return;
        }

        _camera = camera;
        _hbao = camera.GetComponent<HBAO>();

        // same class of bug as Bloom's missing check: a camera without a
        // pre-configured HBAO (ours doesn't ship one) leaves this null, and every
        // line below unconditionally dereferences it.
        if (_hbao == null)
        {
            Plugin.Log.LogError("AmbientOcclusion: No HBAO component on the camera!");
            return;
        }

        _defaultAOSettings = _hbao.aoSettings;
        _defaultColorBleedingSettings = _hbao.colorBleedingSettings;

        UpdateSettings();
    }

    public void UpdateSettings()
    {
        if (_hbao == null) return;
        if (Plugin.GraphicsConfig.AOEnabled.Value)
        {
            var settings = _hbao.aoSettings;
            settings.intensity = Plugin.GraphicsConfig.AOIntensity.Value;
            settings.radius = Plugin.GraphicsConfig.AORadius.Value;
            settings.bias = Plugin.GraphicsConfig.AOBias.Value;
            settings.useMultiBounce = Plugin.GraphicsConfig.AOMultiBounceEnabled.Value;
            settings.multiBounceInfluence = Plugin.GraphicsConfig.AOMultiBounceInfluence.Value;
            _hbao.aoSettings = settings;

            var colorbleedSettings = _hbao.colorBleedingSettings;
            colorbleedSettings.enabled = Plugin.GraphicsConfig.AOColorBleedEnabled.Value;
            colorbleedSettings.saturation = Plugin.GraphicsConfig.AOColorBleedSaturation.Value;
            colorbleedSettings.albedoMultiplier = Plugin.GraphicsConfig.AOColorBleedAlbedoMul.Value;
            _hbao.colorBleedingSettings = colorbleedSettings;
        }
        else
        {
            _hbao.aoSettings = _defaultAOSettings;
            _hbao.colorBleedingSettings = _defaultColorBleedingSettings;
        }
    }

    // called every frame from GraphicsController.Update() alongside Bloom's own
    // Update() - cheap (one bool read once NightVision is cached, a component
    // enabled-flag write only on an actual on/off transition).
    public void Update()
    {
        if (_hbao == null || _camera == null) return;

        if (!_nightVisionSearched)
        {
            _nightVisionSearched = true;
            _nightVision = _camera.GetComponent<BSG.CameraEffects.NightVision>();
            // DIAGNOSTIC (2026-09, field report: rebuilt with the AO/NVG guard above and
            // NVG still reads exactly as dark as before - checking whether the guard is
            // firing at all, or whether NightVision.enabled just never reflects real NVG
            // state on this build).
            Plugin.Log.LogWarning(_nightVision == null
                ? "[NvgAoDiag] NightVision component not found on camera — the AO/NVG guard can never fire"
                : $"[NvgAoDiag] NightVision component found, initial enabled={_nightVision.enabled}");

            if (_nightVision != null) BuildNvBoolFieldSnapshot();
        }

        PollNvBoolFields();

        var nvOn = _nightVision != null && _nightVision.enabled;

        if (nvOn && !_suppressedForNvg)
        {
            _suppressedForNvg = true;
            _hbao.enabled = false;
            Plugin.Log.LogWarning("[NvgAoDiag] NVG turned on — HBAO suppressed");
        }
        else if (!nvOn && _suppressedForNvg)
        {
            _suppressedForNvg = false;
            _hbao.enabled = true;
            Plugin.Log.LogWarning("[NvgAoDiag] NVG turned off — HBAO restored");
        }
    }

    // collects every bool field (any access level, this type and its base types) on
    // the live NightVision instance once, and logs the starting snapshot so the poll
    // below has a baseline to diff against.
    private void BuildNvBoolFieldSnapshot()
    {
        try
        {
            var fields = new List<FieldInfo>();
            for (var t = _nightVision.GetType(); t != null && t != typeof(object); t = t.BaseType)
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.FieldType == typeof(bool)));
            _nvBoolFields = fields.Distinct().ToArray();
            _nvBoolLast = _nvBoolFields.Select(f => (bool)f.GetValue(_nightVision)).ToArray();

            var dump = string.Join(", ", _nvBoolFields.Select((f, i) => $"{f.DeclaringType?.Name}.{f.Name}={_nvBoolLast[i]}"));
            Plugin.Log.LogWarning($"[NvgAoDiag] NightVision bool fields at baseline: {dump}");
        }
        catch (Exception e)
        {
            Plugin.Log.LogWarning($"[NvgAoDiag] bool field snapshot failed: {e.Message}");
        }
    }

    // ~2x/sec: cheap enough to leave running for a whole raid, frequent enough to
    // catch a manual N-key toggle. Logs only fields whose value actually flipped
    // since the last poll, tagged so a real toggle test shows exactly which field(s)
    // moved together with the player's own action.
    private void PollNvBoolFields()
    {
        if (_nvBoolFields == null || _nightVision == null) return;
        if (Time.time < _nextNvFieldPoll) return;
        _nextNvFieldPoll = Time.time + 0.5f;

        try
        {
            for (int i = 0; i < _nvBoolFields.Length; i++)
            {
                bool now = (bool)_nvBoolFields[i].GetValue(_nightVision);
                if (now != _nvBoolLast[i])
                {
                    Plugin.Log.LogWarning($"[NvgAoDiag] {_nvBoolFields[i].DeclaringType?.Name}.{_nvBoolFields[i].Name} changed {_nvBoolLast[i]} -> {now}");
                    _nvBoolLast[i] = now;
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogWarning($"[NvgAoDiag] bool field poll failed: {e.Message}");
        }
    }
}
