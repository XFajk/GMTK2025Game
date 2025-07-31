using Godot;
using System;
using System.Collections.Generic;

public partial class MissionTravel : Node3D, IMission {

    [Export]
    public string TargetName = "nearest";

    [Export(PropertyHint.Range, "0,300,1")]
    public float PreparationTime = 10.0f;

    [Export(PropertyHint.Range, "0,600,10")]
    public float Duration;

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float TargetEnginePower;

    public void ApplyEffect(Ship ship) {
        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                e.EnginePower = TargetEnginePower;
            }
        }
    }

    public string[] Briefing() => [
        $"Today we will travel to the {TargetName} solar system. "
        + $"We will run the engine at {TargetEnginePower * 100}% capacity, rather than the usual {Engine.DefaultEnginePower * 100}%. "
        + $"Make sure the engine has enough coolant, and keep an eye on the water supply. "
        + $"We will leave in {PreparationTime} seconds",
        "End of Brief"
    ];

    public float GetDuration() => Duration;

    public float GetPreparationTime() => PreparationTime;

    public void OnCompletion(Ship ship) {
        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                e.EnginePower = Engine.DefaultEnginePower;
            }
        }
    }

    public string[] Debrief() => [
        $"We have arrived! The engine will return to {Engine.DefaultEnginePower * 100}% power."
    ];
}
