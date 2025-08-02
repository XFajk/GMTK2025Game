using Godot;
using System;
using System.Collections.Generic;

public partial class MissionTravel : Node, IMission {

    [Export]
    public string TargetName = "nearest";

    [Export(PropertyHint.Range, "0,300,1")]
    public float PreparationTime = 10.0f;

    [Export(PropertyHint.Range, "0,600,10")]
    public float Duration;

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float TargetEnginePower;
    public IMission.Properties Properties;
    
    private Ship _ship;
    private ulong _preparationStartTime;

    private ulong _startTime;

    public string GetTitle() => "Mission: Hyperspace jump";

    void IMission.Ready(Ship ship) {
        _ship = ship;
        _preparationStartTime = Time.GetTicksUsec();
        
        Properties = new() {
            Title = "Mission: Reparations",
            Briefing = [
                $"Today we will travel to the {TargetName} solar system. "
                + $"We will run the engine at {TargetEnginePower * 100}% capacity, rather than the usual {Engine.DefaultEnginePower * 100}%. "
                + $"Make sure the engine has enough coolant, and keep an eye on the water supply. "
                + $"We will leave in {PreparationTime} seconds",
                "End of Brief"
            ],
            Debrief = [
                $"We have arrived! The engine will return to {Engine.DefaultEnginePower * 100}% power."
            ],
        };
    }

    public IMission.Properties GetMissionProperties() => Properties;

    public void ApplyEffect(Ship ship) {
        _startTime = Time.GetTicksUsec();

        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                e.EnginePower = TargetEnginePower;
            }
        }
    }

    public void OnCompletion(Ship ship) {
        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                e.EnginePower = Engine.DefaultEnginePower;
            }
        }
    }

    public bool IsPreparationFinished() {
        double timePassed = (double)(Time.GetTicksUsec() - _preparationStartTime) / 1E6;
        return _preparationStartTime != 0 && timePassed > PreparationTime;
    }

    public bool IsMissionFinised() {
        double timePassed = (double)(Time.GetTicksUsec() - _startTime) / 1E6;
        return _startTime != 0 && timePassed > Duration;
    }
}
