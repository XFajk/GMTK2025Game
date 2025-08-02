using Godot;
using System;
using System.Collections.Generic;

public partial class MissionEngineRev : TimedMission, IMission {

    [Export]
    public string TargetSolarSystemName = "nearest";

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float TargetEnginePower;
    public IMission.Properties Properties;

    private Ship _ship;

    private List<Engine> _engines = new();

    void IMission.MissionReady(Ship ship) {
        _ship = ship;

        Properties = new() {
            Title = "Mission: Getting around Quicker",
            Briefing = [
                $"We're done crawling around the solar system, we'll be running the engine at {TargetEnginePower * 100} from now on"
            ],
            Debrief = [
                $"We have arrived! The engine will return to {Engine.DefaultEnginePower * 100}% power."
            ],
        };

        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                _engines.Add(e);
                ship.ScheduleCrewTask(new CrewTask() {
                    Location = e.Position,
                    Duration = Duration + PreparationTime
                });
            }
        }
    }

    public override IMission.Properties GetMissionProperties() => Properties;

    public override void OnStart(Ship ship) {
        foreach (Engine e in _engines) {
            e.EnginePower = TargetEnginePower;
        }
    }

    public override void OnCompletion(Ship ship) {
        foreach (Engine e in _engines) {
            e.EnginePower = Engine.DefaultEnginePower;
        }
    }
}
