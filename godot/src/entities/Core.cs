using Godot;
using System;

public partial class Core : Node3D {
    [Export(PropertyHint.ColorNoAlpha)]
    public Color HighPowerCoreColor;

    private GpuParticles3D _aura;
    ParticleProcessMaterial _auraMaterial;
    private float _auraHueDeviation;

    private GpuParticles3D _sparks;
    private ParticleProcessMaterial _sparksMaterial;

    private OmniLight3D _light;
    private float _lightSaturation;

    private float _lowPowerHue;
    private float _highPowerHue;

    private float _currentPowerFactor;

    public override void _Ready() {
        _aura = GetNode<GpuParticles3D>("Aura");
        _sparks = GetNode<GpuParticles3D>("Sparks");
        _light = GetNode<OmniLight3D>("OmniLight3D");

        _auraMaterial = _aura.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        _aura.ProcessMaterial = _auraMaterial;
        _auraHueDeviation = (_auraMaterial.HueVariationMax - _auraMaterial.HueVariationMin) / 2;

        _sparksMaterial = _sparks.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        _sparks.ProcessMaterial = _sparksMaterial;

        _light.LightColor.ToHsv(out _lowPowerHue, out _lightSaturation, out _);
        _highPowerHue = HighPowerCoreColor.H;

        SetEnginePowerIntern(Engine.DefaultEnginePower);
    }

    public void SetEnginePower(float powerFactor) {
        Tween tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<float>(SetEnginePowerIntern), _currentPowerFactor, powerFactor, Engine.ChangeDuration);
    }

    private void SetEnginePowerIntern(float powerFactor) {
        _currentPowerFactor = powerFactor;
        float hue;
        float particleRatio;
        if (powerFactor <= Engine.DefaultEnginePower) {
            float relativeFactor = powerFactor / Engine.DefaultEnginePower;
            hue = _lowPowerHue;
            particleRatio = Mathf.Lerp(0, 1f, relativeFactor);
        } else {
            float relativeFactor = (powerFactor - Engine.DefaultEnginePower) / (1.0f - Engine.DefaultEnginePower);
            hue = Mathf.Lerp(_lowPowerHue, _highPowerHue, relativeFactor);
            particleRatio = Mathf.Lerp(1, 1.5f, relativeFactor);
        }

        _sparks.AmountRatio = particleRatio;
        _aura.AmountRatio = particleRatio;
        _auraMaterial.HueVariationMin = hue - _auraHueDeviation;
        _auraMaterial.HueVariationMax = hue + _auraHueDeviation;
        _light.LightColor = Color.FromHsv(hue, _lightSaturation, 1);
        _light.LightEnergy = particleRatio;
    }
}
