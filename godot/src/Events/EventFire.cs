using Godot;
using System;
using System.Collections.Generic;

public partial class EventFire : Node3D, IEvent, IRepairable {
    [Export]
    public float OxygenLossPerSecond = 0.5f;
    [Export]
    public float SecondsToExtinguish = 2;
    
    [Export]
    public float SecondsToRepair = 10;

    private Ship _ship;
    private EventEffectResource _oxygenReductionEffect;
    private EventEffectResource _Co2AdditionEffect;

    public void ApplyEffect(Ship ship) {
        _ship = ship;
        FloatingResource oxygen = ship.GetFloatingResource(Resource.Oxygen);
        FloatingResource co2 = ship.GetFloatingResource(Resource.CarbonDioxide);

        _oxygenReductionEffect = new EventEffectResource() {
            Target = oxygen,
            AdditionPerSecond = -OxygenLossPerSecond
        };
        ship.ActiveEffects.Add(_oxygenReductionEffect);

        _Co2AdditionEffect = new EventEffectResource() {
            Target = co2,
            AdditionPerSecond = OxygenLossPerSecond * Resources.GetRatio(Resource.Oxygen, Resource.CarbonDioxide)
        };
        ship.ActiveEffects.Add(_Co2AdditionEffect);

        ship.ScheduleCrewTask(new CrewTask() {
            Location = Position,
            Duration = SecondsToExtinguish,
        });

        ship.AddChild(this);
    }

    bool IRepairable.IsWorking() => false;

    Node3D IRepairable.AsNode() => this;

    void IRepairable.SetRepaired() {
        _ship.RemoveChild(this);
        _ship.ActiveEffects.Remove(_oxygenReductionEffect);
        _ship.ActiveEffects.Remove(_Co2AdditionEffect);

        float equivalentDisposableCost = _Co2AdditionEffect.TotalResourcesAdded * Resources.GetRatio(Resource.CarbonDioxide, Resource.Disposables);

        _ship.ScheduleMission(new MissionFireRepair() {
            // ceil, so it costs at least as much as the carbon gained
            Quantity = Mathf.CeilToInt(equivalentDisposableCost),
            Location = Position,
            SecondsToRepair = SecondsToRepair,
        });
    }
}
