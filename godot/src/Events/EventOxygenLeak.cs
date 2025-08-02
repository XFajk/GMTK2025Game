using Godot;

public partial class EventOxygenLeak : Node3D, IEvent, IRepairable {
    [Export]
    public float LossPerSecond = 1;
    
    [Export]
    public float SecondsToRepair = 5;

    private Ship _ship;
    private EventEffectResourceAdd _effect;

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"Oxygen is leaking through the hull! A crewmember is on its way to close the hole.",
        IconPosition = Position,
    };

    public void ApplyEffect(Ship ship) {
        _ship = ship;
        FloatingResource oxygen = ship.GetFloatingResource(Resource.Oxygen);

        _effect = new EventEffectResourceAdd() {
            Target = oxygen,
            AdditionPerSecond = -LossPerSecond
        };
        ship.ActiveEffects.Add(_effect);

        ship.ScheduleCrewTask(new CrewTask() {
            Location = Position,
            Duration = SecondsToRepair
        });

        ship.AddChild(this);
    }

    bool IRepairable.IsWorking() => false;

    Node3D IRepairable.AsNode() => this;

    void IRepairable.SetRepaired() {
        _ship.RemoveChild(this);
        _ship.ActiveEffects.Remove(_effect);
    }

}
