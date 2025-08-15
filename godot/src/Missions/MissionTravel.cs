using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MissionTravel : TimedMission, IMission {

    [Export]
    public string TargetSolarSystemName = "nearest";

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float TargetEnginePower;
    public IMission.Properties Properties;

    private Ship _ship;

    private List<Engine> _engines = new();

    void IMission.MissionReady(Ship ship, MissionManager.Clock missionClock) {
        base.MissionReady(ship, missionClock);
        _ship = ship;

        Properties = new() {
            Title = "Mission: Travel",
            Briefing = [
                $"Today we will travel to the {TargetSolarSystemName} solar system. "
                + $"We will run the engine at {TargetEnginePower * 100}% capacity, rather than the usual {Engine.DefaultEnginePower * 100}%. "
                + $"Make sure the engine has enough coolant, and keep an eye on the water supply. "
                + $"We will leave in {PreparationTime} seconds",
                "End of Brief"
            ],
            Debrief = [
                $"We have arrived! The engine will return to {Engine.DefaultEnginePower * 100}% power."
            ],
        };

        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                _engines.Add(e);
                ship.ScheduleCrewTask(new CrewTask() {
                    Location = e.GlobalPosition,
                    Duration = Duration + PreparationTime
                });
            }
        }
    }

    public override IMission.Properties GetMissionProperties() => Properties;

    public override void _Process(double delta) {
        if (_state == IMission.State.Started && _engines.Any(e => !e.MachineIsProcessing)) _missionEndTime += delta;
    }


    public override void OnStart(Ship ship) {
        base.OnStart(ship);
        foreach (Engine e in _engines) {
            e.SetEnginePower(TargetEnginePower);
        }
    }

    public override void OnCompletion(Ship ship) {
        base.OnCompletion(ship);
        foreach (Engine e in _engines) {
            e.SetEnginePower(Engine.DefaultEnginePower);
        }
    }
}
