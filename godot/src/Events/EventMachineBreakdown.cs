using System;
using Godot;

public partial class EventMachineBreakdown : Node, IEvent {
    [Export]
    public float SecondsToRepair = 5;

    [Export]
    public Machine Target;
    private Node3D _explosion;
    private static readonly PackedScene ExplosionVfx = GD.Load<PackedScene>("res://scenes/vfx/fire.tscn");

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"{Target.Name} has broken down. A crewmember is on its way to repair it.",
        IconPosition = Target.Position,
    };

    public void ApplyEffect(Ship ship) {
        Node3D _explosionSfx = ExplosionVfx.Instantiate<Node3D>();
        ship.AddChild(_explosionSfx);
        _explosionSfx.GlobalPosition = Target.GlobalPosition;

        // only one second
        GetTree().CreateTween()
            .TweenCallback(Callable.From(() => _explosionSfx.QueueFree()))
            .SetDelay(1.0f);

        Target.MachineIsWorking = false;
        ScheduleRepair(ship);
    }

    private void ScheduleRepair(Ship ship) {
        ship.ScheduleCrewTask(new CrewTask() {
            Location = Target.GlobalPosition,
            Duration = SecondsToRepair,
            OnTaskComplete = p => SetRepaired(),
            OnTaskAbort = (p => ScheduleRepair(ship))
        });
    }

    private void SetRepaired() {
        Target.MachineIsWorking = true;
    }


    public bool DoPanic() => false;
}
