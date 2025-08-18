using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MissionEngineRev : TimedMission, IMission {

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float TargetEnginePower = 0.5f;
    [Export(PropertyHint.Range, "0,1,0.05")]
    public float FinalEnginePower = Engine.DefaultEnginePower * 2;
    public IMission.Properties Properties;

    private List<Engine> _engines = new();

    void IMission.MissionReady(Ship ship, MissionManager.Clock missionClock) {
        base.MissionReady(ship, missionClock);

        Properties = new() {
            Title = "Mission: Getting around Quicker",
            Briefing = [
                $"We're done crawling around the solar system, we'll be running the engine at {TargetEnginePower * 100} from now on"
            ],
            Debrief = [
                $"So as it turns out, running the engine requires water. We decide to run the engine on {FinalEnginePower * 100} instead"
            ],
        };

        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                _engines.Add(e);
            }
        }
    }

    public override bool IsDelayed() => _state == IMission.State.Started && _engines.Any(e => !e.MachineIsProcessing);

    public override IMission.Properties GetMissionProperties() => Properties;

    public override void OnStart(Ship ship) {
        base.OnStart(ship);
        foreach (Engine e in _engines) {
            e.SetEnginePower(TargetEnginePower);
        }
    }

    public override void OnCompletion(Ship ship) {
        base.OnCompletion(ship);
        foreach (Engine e in _engines) {
            e.SetEnginePower(FinalEnginePower);
        }
    }
}
