using Godot;
using System;
using System.Collections.Generic;

public partial class EventFire : Node3D, IEvent, IRepairable {
    [Export]
    public float OxygenLossPerSecond = 0.5f;
    [Export]
    public int SecondsToExtinguish = 2;

    [Export]
    public int SecondsToRepair = 10;
    [Export]
    public bool RepairMissionPopup = true;

    private Ship _ship;
    private EventEffectResourceConvert _fireConversionEffect;
    private Node3D _fireVfx;
    private bool _isFixed = false;

    private static readonly PackedScene FireVfx = GD.Load<PackedScene>("res://scenes/vfx/fire.tscn");

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"A fire has broken out! A crewmember is on its way to extinguish it.",
        IconPosition = Position,
    };

    public void ApplyEffect(Ship ship) {
        _ship = ship;

        _fireVfx = FireVfx.Instantiate<Node3D>();
        ship.AddChild(_fireVfx);
        _fireVfx.GlobalPosition = GlobalPosition;

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

        ScheduleExtinguishTask(ship);
    }

    private void ScheduleExtinguishTask(Ship ship) {
        ship.ScheduleCrewTask(new CrewTask() {
            Location = GlobalPosition,
            Duration = SecondsToExtinguish,
            ActionType = CrewTask.Type.Extinguish,
            OnTaskComplete = p => SetRepaired(),
            OnTaskAbort = p => ScheduleExtinguishTask(ship),
        });
    }

    public bool DoPanic() => !_isFixed;

    public bool IsWorking() => _isFixed;

    public Node3D AsNode() => this;

    public void SetRepaired() {
        _isFixed = true;
        _ship.ActiveEffects.Remove(_fireConversionEffect);
        _fireVfx.QueueFree();

        float co2Created = _fireConversionEffect.TotalChangeOf(Resource.CarbonDioxide);
        var ratio = Resources.GetRatio(Resource.CarbonDioxide, Resource.Disposables);
        float equivalentDisposableCost = (co2Created * ratio.Key) / ratio.Value;
        // floor, because otherwise it may be impossible
        int trueQuantity = Mathf.FloorToInt(equivalentDisposableCost);
        if (trueQuantity > 0) {
            _ship.ScheduleEvent(new MissionFireRepair() {
                Quantity = Mathf.FloorToInt(equivalentDisposableCost),
                Location = Position,
                SecondsToRepair = SecondsToRepair,
                RepairMissionPopup = RepairMissionPopup
            });
        }
    }
}
