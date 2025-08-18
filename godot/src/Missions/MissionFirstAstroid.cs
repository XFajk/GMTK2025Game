using Godot;
using System;
using System.Collections.Generic;

public partial class MissionFirstAstroid : TimedMission, IMission {
    // measured in 0.2% per second
    private const float CarbonDumpPerSecond = 20f;
    [Export]
    public int RequiredDisposables;
    [Export(PropertyHint.Range, "0,6")]
    public int CrewCount;
    [Export]
    public Node3D AirLock;

    public float OxygenDrainPerSecond;
    public float OxygenDrainTotal;
    public float CarbonDioxideReturnQuantity;

    private ulong _preparationStartTime;
    private ulong _startTime;

    public IMission.Properties Properties;
    private Ship _ship;
    private List<Engine> _engines = new();

    void IMission.MissionReady(Ship ship, MissionManager.Clock missionClock) {
        base.MissionReady(ship, missionClock);
        _ship = ship;
        var carbonToOxygen = Resources.GetRatio(Resource.CarbonDioxide, Resource.Oxygen);
        var disposablesToCarbon = Resources.GetRatio(Resource.Disposables, Resource.CarbonDioxide);

        CarbonDioxideReturnQuantity = RequiredDisposables * disposablesToCarbon.Key / disposablesToCarbon.Value;
        OxygenDrainTotal = (CarbonDioxideReturnQuantity * carbonToOxygen.Key / carbonToOxygen.Value);
        OxygenDrainPerSecond = OxygenDrainTotal / Duration;

        _preparationStartTime = Time.GetTicksUsec();

        Properties = new() {
            Title = "Mission: Astroid mining",
            Briefing = [
                "Before its... interruption, the scan picked up a nearby astroid. We will be scanning the astroid for its material composition."
                + $"For this mission we will need {Resources.ToUnit(Resource.Disposables, RequiredDisposables)} materials from the carbon scrubber, "
                + $"and during the operation we will channel an additional {Resources.ToUnit(Resource.Oxygen, Mathf.CeilToInt(OxygenDrainPerSecond * 60))} oxygen per minute to the crew outside the ship."
                + $"The operation begins in {PreparationTime} seconds, make sure the supplies are available then",
                "Naturally, the engine will be turned off for the duration",
                "End of Brief"
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource.Disposables, RequiredDisposables)],
            Debrief = [
                "Mission completed! "
                + $"We will be emptying our cylinders of carbon dioxide for the next {(int) CarbonDioxideReturnQuantity / CarbonDumpPerSecond} seconds or so."
            ]
        };

        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                _engines.Add(e);
            }
        }
    }
    
    public override IMission.Properties GetMissionProperties() => Properties;

    public override void OnStart(Ship ship) {
        base.OnStart(ship);
        foreach (var pair in Properties.ResourceMinimumRequirements) {
            ship.RemoveResource(pair.Key, pair.Value);
        }

        for (int i = 0; i < CrewCount; i++) {
            ship.ScheduleCrewTask(new CrewTask() {
                Duration = Duration,
                Location = AirLock.Position,
                ActionType = CrewTask.Type.Disappear
            });
        }

        ship.ActiveEffects.Add(new EventEffectResourceAdd() {
            AdditionPerSecond = -OxygenDrainPerSecond,
            MaxResourcesToAdd = OxygenDrainTotal,
            Target = ship.GetFloatingResource(Resource.Oxygen)
        });

        foreach (Engine e in _engines) {
            e.SetEnginePower(0);
        }
    }

    public override bool IsPreparationFinished() {
        return base.IsPreparationFinished() && (this as IMission).CheckMaterialRequirements(_ship);
    }

    public override void OnCompletion(Ship ship) {
        base.OnCompletion(ship);
        ship.ActiveEffects.Add(new EventEffectResourceAdd() {
            AdditionPerSecond = CarbonDumpPerSecond,
            MaxResourcesToAdd = CarbonDioxideReturnQuantity,
            Target = ship.GetFloatingResource(Resource.CarbonDioxide)
        });

        foreach (Engine e in _engines) {
            e.SetEnginePower(Engine.DefaultEnginePower);
        }
    }


    public override bool IsDelayed() {
        return base.IsPreparationFinished() && !(this as IMission).CheckMaterialRequirements(_ship);
    }
}
