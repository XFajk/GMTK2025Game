using Godot;
using System;

public partial class CarbonScrubber : Machine {
    public bool IsActive = false;

    private float _standardProcessingPerSecond;
    private ActivationLight _activationLight;
    private GpuParticles3D _smoke;

    public override void _Ready() {
        base._Ready();

        _standardProcessingPerSecond = _processingPerSecond;
        _activationLight = GetNode<ActivationLight>("ActivationLight");
        _smoke = GetNode<GpuParticles3D>("VFX/Smoke");

        _hoverDetectionArea.InputEvent += (_, eventType, _, _, _) => {
            if (eventType.IsActionPressed("interact")) {
                SetMachineActive(!IsActive);
            }
        };

        SetMachineActive(IsActive);
    }

    private void SetMachineActive(bool active) {
        _processingPerSecond = active ? _standardProcessingPerSecond : 0;
        IsActive = active;
        _activationLight.LightSetActive(IsActive);
        // we use AmountRatio for a fluid on/off behavior
        _smoke.AmountRatio = active ? 1 : 0;
    }
}
