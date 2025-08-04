using Godot;
using System;

public partial class Core : Node3D {
    [Export]
    public Color HighPowerCoreColor;

    private GpuParticles3D _aura;
    ParticleProcessMaterial _auraMaterial;

    private GpuParticles3D _sparks;
    private ParticleProcessMaterial _sparksMaterial;

    private OmniLight3D _light;
    private Color _baseColor;

    public override void _Ready() {
        _aura = GetNode<GpuParticles3D>("Aura");
        _sparks = GetNode<GpuParticles3D>("Sparks");
        _light = GetNode<OmniLight3D>("OmniLight3D");

        _auraMaterial = _aura.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        _aura.ProcessMaterial = _auraMaterial;

        _sparksMaterial = _sparks.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        _sparks.ProcessMaterial = _sparksMaterial;

        _baseColor = _light.LightColor;
        SetEnginePower(Engine.DefaultEnginePower);
    }

    public void SetEnginePower(float powerFactor) {
        
        Color newColor = _baseColor.LinearToSrgb().Lerp(HighPowerCoreColor.LinearToSrgb(), powerFactor).SrgbToLinear();

        if (powerFactor < Engine.DefaultEnginePower) {
            // dim the light
            newColor.A = powerFactor * Engine.DefaultEnginePower;
            _sparks.AmountRatio = powerFactor * Engine.DefaultEnginePower;
        } else {
            _sparks.AmountRatio = Mathf.Lerp(0.8f, 1.8f, powerFactor);
        }
        
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(_auraMaterial, "color", newColor, Engine.ChangeDuration);
        tween.TweenProperty(_light, "light_color", newColor, Engine.ChangeDuration);
    }
}
