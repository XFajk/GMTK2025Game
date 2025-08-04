using Godot;
using System;

public partial class JetEngineFire : Node3D {

    [Export]
    public Color HighPowerFireColor;
    
    private GpuParticles3D _fire;
    private GpuParticles3D _sparks;
    private OmniLight3D _light;
    private OmniLight3D _light2;
    private StandardMaterial3D _fireMaterial;
    private ParticleProcessMaterial _sparksMaterial;

    private float _lightSaturation;
    private float _lowPowerHue;
    private float _highPowerHue;
    private float _currentPowerFactor;
    private float _fireSaturation;


    public override void _Ready() {
        _fire = GetNode<GpuParticles3D>("Fire");
        _sparks = GetNode<GpuParticles3D>("Sparks");
        _light = GetNode<OmniLight3D>("OmniLight3D");
        _light2 = GetNode<OmniLight3D>("OmniLight3D2");

        _fireMaterial = _fire.DrawPass1.SurfaceGetMaterial(0).Duplicate() as StandardMaterial3D;
        _fire.DrawPass1.SurfaceSetMaterial(0, _fireMaterial);

        _sparksMaterial = _sparks.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        _sparks.ProcessMaterial = _sparksMaterial;

        _fireMaterial.Emission.ToHsv(out _, out _fireSaturation, out _);
        _light.LightColor.ToHsv(out _lowPowerHue, out _lightSaturation, out _);
        _highPowerHue = HighPowerFireColor.H;

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
        _fire.AmountRatio = particleRatio;
        _fireMaterial.Emission = Color.FromHsv(hue, _fireSaturation, 1);
        _light.LightColor = Color.FromHsv(hue, _lightSaturation, 1);
        _light.LightEnergy = particleRatio;
        _light2.LightColor = Color.FromHsv(hue, _lightSaturation, 1);
        _light2.LightEnergy = particleRatio;
    }
}
