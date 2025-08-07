using Godot;

public partial class EventOxygenLeak : Node3D, IEvent, IRepairable {
    [Export]
    public float LossPerSecond = 1;

    [Export]
    public float SecondsToRepair = 5;

    private Ship _ship;
    private Node3D _leakSfx;

    private EventEffectResourceAdd _effect;
    private static readonly PackedScene ExplosionSfx = GD.Load<PackedScene>("res://scenes/vfx/explosion.tscn");
    private static readonly PackedScene LeakSfx = GD.Load<PackedScene>("res://scenes/vfx/air_lock_air.tscn");

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"Oxygen is leaking through the hull! A crewmember is on its way to close the hole.",
        IconPosition = Position,
    };

    public void ApplyEffect(Ship ship) {
        _ship = ship;

        _leakSfx = LeakSfx.Instantiate<Node3D>();
        ship.AddChild(_leakSfx);
        _leakSfx.GlobalPosition = GlobalPosition;
        _leakSfx.GlobalRotation = GlobalRotation;

        Node3D _explosionSfx = ExplosionSfx.Instantiate<Node3D>();
        ship.AddChild(_explosionSfx);
        _explosionSfx.GlobalPosition = GlobalPosition;

        // only one second
        GetTree().CreateTween()
            .TweenCallback(Callable.From(() => _explosionSfx.QueueFree()))
            .SetDelay(1.0f);

        FloatingResource oxygen = ship.GetFloatingResource(Resource.Oxygen);

        _effect = new EventEffectResourceAdd() {
            Target = oxygen,
            AdditionPerSecond = -LossPerSecond
        };
        ship.ActiveEffects.Add(_effect);

        ScheduleRepairTask(ship);

        ship.AddChild(this);
    }

    private void ScheduleRepairTask(Ship ship) {
        ship.ScheduleCrewTask(new CrewTask() {
            Location = GlobalPosition,
            Duration = SecondsToRepair,
            ActionType = CrewTask.Type.Repair,
            OnTaskComplete = p => SetRepaired(),
            OnTaskAbort = p => ScheduleRepairTask(ship),
        });
    }


    public bool IsWorking() => false;

    public Node3D AsNode() => this;

    public void SetRepaired() {
        _ship.RemoveChild(this);
        _ship.ActiveEffects.Remove(_effect);
        _leakSfx.QueueFree();
    }

}
