using Godot;
using System;

public partial class JetEngineFire : Node3D {

    [Export]
    public Color HighPowerFireColor;
    private GpuParticles3D _fire;
    private GpuParticles3D _sparks;
    private OmniLight3D _light;
    private OmniLight3D _light2;
    private ParticleProcessMaterial _fireMaterial;
    private ParticleProcessMaterial _sparksMaterial;
    private Color _baseColor;


    public override void _Ready() {
        _fire = GetNode<GpuParticles3D>("Fire");
        _sparks = GetNode<GpuParticles3D>("Sparks");
        _light = GetNode<OmniLight3D>("OmniLight3D");
        _light2 = GetNode<OmniLight3D>("OmniLight3D2");

        _fireMaterial = _fire.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        _fire.ProcessMaterial = _fireMaterial;

        _sparksMaterial = _sparks.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        _sparks.ProcessMaterial = _sparksMaterial;

        _baseColor = _light.LightColor;
        SetEnginePower(Engine.DefaultEnginePower);
    }

    public void SetEnginePower(float powerFactor) {
        Color newColor = _baseColor.Lerp(HighPowerFireColor, powerFactor);

        if (powerFactor < Engine.DefaultEnginePower) {
            // dim the light
            newColor.A = powerFactor * Engine.DefaultEnginePower;
        }

        _sparks.AmountRatio = Mathf.Lerp(0.8f, 1.8f, powerFactor);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(_fireMaterial, "color", newColor, Engine.ChangeDuration);
        tween.TweenProperty(_sparksMaterial, "color", newColor, Engine.ChangeDuration);
        tween.TweenProperty(_light, "light_color", newColor, Engine.ChangeDuration);
        tween.TweenProperty(_light2, "light_color", newColor, Engine.ChangeDuration);
    }
}
