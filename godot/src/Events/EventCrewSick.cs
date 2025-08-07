using Godot;
using System;
using System.Collections.Generic;

public partial class EventCrewSick : Node, IEvent {
    [Export(PropertyHint.Range, "0, 600, 1")]
    public int RecoveryTimeSeconds = 60;

    [Export]
    public Person Target;

    public Node3D CrewQuarters;

    [Export(PropertyHint.Range, "0, 2")]
    public float DisposablesUsedPerSecond;

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"One of our crew has fallen ill. We will be using some additional disposables for hygiene",
        IconPosition = CrewQuarters.Position
    };

    public void ApplyEffect(Ship ship) {
        CrewQuarters = ship.GetNode<Node3D>("CrewQuarters");
        Target ??= ship.GetRandomPerson();

        ship.ScheduleCrewTask(
            new CrewTask() {
                Location = CrewQuarters.GlobalPosition,
                Duration = RecoveryTimeSeconds,
                ActionType = CrewTask.Type.Disappear,
            },
            Target
        );

        // set an effect to make it simpler
        var ratio = Resources.GetRatio(Resource.Disposables, Resource.Garbage);

        EventEffectResourceConvert _conversionEffect = new() {
            ConversionPerSecond = DisposablesUsedPerSecond,
            Conversion = [
                new InputOutput() {
                    QuantityChangeInReceipe = -ratio.Key,
                    Container = ship.GetContainer(Resource.Disposables, Ship.Select.OnlyOutputs),
                },
                new InputOutput() {
                    QuantityChangeInReceipe = ratio.Value,
                    Container = ship.GetContainer(Resource.Garbage, Ship.Select.OnlyInputs),
                },
            ],
            MaxCycles = (RecoveryTimeSeconds * DisposablesUsedPerSecond)
        };

        ship.ActiveEffects.Add(_conversionEffect);
    }
}
