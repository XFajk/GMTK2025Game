using Godot;
using System;

public partial class ActivationLight : MeshInstance3D
{
    private StandardMaterial3D _material;


    public override void _Ready() {
        _material = GetSurfaceOverrideMaterial(0).Duplicate() as StandardMaterial3D;
        SetSurfaceOverrideMaterial(0, _material);
    }

    public void LightSetActive(bool active) {
        _material.EmissionEnabled = active;
    }
}
