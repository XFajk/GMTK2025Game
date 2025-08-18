using Godot;
using System;
using System.Collections.Generic;

public partial class MissionIceAstroidMining : TimedMission, IMission {
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
            Title = "Mission: Get more water",
            Briefing = [
                "Ok so we notice we are running low on water, but don't worry. "
                + "We found an ice astroid, so we will be mining water today. "
                + "You get us the Disposables, and we will get you all the water you need. "
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource.Disposables, RequiredDisposables)],
            Debrief = [
                "Mission completed, enjoy the water!"
            ]
        };
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
                Location = AirLock.GlobalPosition,
                ActionType = CrewTask.Type.Disappear
            });
        }

        ship.ActiveEffects.Add(new EventEffectResourceAdd() {
            AdditionPerSecond = -OxygenDrainPerSecond,
            MaxResourcesToAdd = OxygenDrainTotal,
            Target = ship.GetFloatingResource(Resource.Oxygen)
        });
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

        // fill up everything with water
        ship.AddResource(Resource.Water, float.MaxValue);
    }

    public override bool IsDelayed() => false;
}
