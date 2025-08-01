using Godot;
using System;
using System.Collections.Generic;

public partial class EventFire : Node3D, IEvent, IRepairable {
    [Export]
    public float OxygenLossPerSecond = 0.5f;
    [Export]
    public float SecondsToExtinguish = 5;
    
    [Export]
    public float SecondsToRepair = 10;

    private Ship _ship;
    private EventEffectResource _oxygenReductionEffect;
    private EventEffectResource _Co2AdditionEffect;

    public void ApplyEffect(Ship ship) {
        _ship = ship;
        FloatingResource oxygen = ship.GetFloatingResource(Resource.Oxygen);
        FloatingResource co2 = ship.GetFloatingResource(Resource.CarbonDioxide);

        _oxygenReductionEffect = new() {
            Target = oxygen,
            AdditionPerSecond = -OxygenLossPerSecond
        };
        ship.ActiveEffects.Add(_oxygenReductionEffect);

        _Co2AdditionEffect = new() {
            Target = co2,
            AdditionPerSecond = OxygenLossPerSecond
        };
        ship.ActiveEffects.Add(_Co2AdditionEffect);

        // ship.AddCrewTask(new CrewTaskRepair() {
        //     Target = this,
        //     Duration = SecondsToExtinguish
        // });

        ship.AddChild(this);
    }

    bool IRepairable.IsWorking() => false;

    Node3D IRepairable.AsNode() => this;

    void IRepairable.SetRepaired() {
        _ship.RemoveChild(this);
        _ship.ActiveEffects.Remove(_oxygenReductionEffect);
        _ship.ActiveEffects.Remove(_Co2AdditionEffect);

        // _ship.AddMission(new MissionResourceStockpile() {
        //     Resource = Resource.Disposables,
        //     Quantitiy = Mathf.Ceil(_Co2AdditionEffect.TotalResourcesAdded / 8),
        //     OnFinish = () => ship.AddCrewTask(new CrewTaskRepair() {
        //        Target = this,
        //        Duration = SecondsToRepair
        //     })
        // });
    }
}
