
using System.Collections.Generic;
using Godot;

// This mission is supposed to be combined with an EventFire
public partial class MissionPirates : TimedMission {
    [Export]
    public int DisposablesQuantityToPrepare = 10;
    public IMission.Properties Properties;

    public override void MissionReady(Ship ship) {
        base.MissionReady(ship);

        Properties = new() {
            Title = "Mission: friendly ship",
            Briefing = [
                $"So uh... there is a ship on starboard with eh... with pirate markings.",
                "Surely they have no ill will, but just in case... "
                + "Can you prepare, like, reparation equipment?"
            ],
            Debrief = [
                "We've outran them! They're gone! Huzzah!"
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource.Disposables, DisposablesQuantityToPrepare)],
        };
    }

    public override void OnStart(Ship ship) {
        base.OnStart(ship);
        // all hell breaks loose, but mostly in the shape of externally scheduled events
    }

    public override IMission.Properties GetMissionProperties() => Properties;

    public override void OnCompletion(Ship ship) {
    }
}
