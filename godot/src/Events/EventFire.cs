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
    private EventEffectResourceConvert _fireConversionEffect;

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"A fire has broken out! A crewmember is on its way to extinguish it.",
        IconPosition = Position,
    };

    public void ApplyEffect(Ship ship) {
        _ship = ship;
        FloatingResource oxygen = ship.GetFloatingResource(Resource.Oxygen);
        FloatingResource co2 = ship.GetFloatingResource(Resource.CarbonDioxide);

        var ratio = Resources.GetRatio(Resource.Oxygen, Resource.CarbonDioxide);

        _fireConversionEffect = new EventEffectResourceConvert() {
            ConversionPerSecond = OxygenLossPerSecond,
            Conversion = [
                new InputOutput() {
                    QuantityChangeInReceipe = -ratio.Key,
                    Container = oxygen,
                },
                new InputOutput() {
                    QuantityChangeInReceipe = ratio.Value,
                    Container = co2,
                }
            ]
        };

        ship.ActiveEffects.Add(_fireConversionEffect);

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
        _ship.ActiveEffects.Remove(_fireConversionEffect);

        float co2Taken = -_fireConversionEffect.TotalChangeOf(Resource.CarbonDioxide);
        var ratio = Resources.GetRatio(Resource.CarbonDioxide, Resource.Disposables);
        float equivalentDisposableCost = (co2Taken * ratio.Value) / ratio.Key;

        _ship.ScheduleMission(new MissionFireRepair() {
            // ceil, so it costs at least as much as the carbon gained
            Quantity = Mathf.CeilToInt(equivalentDisposableCost),
            Location = Position,
            SecondsToRepair = SecondsToRepair,
        });
    }
}
