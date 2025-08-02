using System.Collections.Generic;
using Godot;

public partial class Delay : Node {
    public enum Trigger {
        NothingPrepared,
        AnyMissionIsActive,
        PreviousMissionActive,
        PreviousMissionFinished,
        SpecificMissionActive,
        SpecificMissionFinished,
        AllMissionsFinished,
    }

    [Export]
    public int TimeDelaySeconds = 0;
    [Export]
    public Trigger TimeDelayStartsWhen = Trigger.AllMissionsFinished;
    [Export]
    public Node MissionToWaitFor = null;

    private bool _isTriggered = false;
    private double _waitUntil = double.MaxValue;

    public override void _Ready() {
        if (MissionToWaitFor != null && MissionToWaitFor is not IMission) {
            throw new System.Exception($"Delay node {Name} has invalid MissionToWaitFor {MissionToWaitFor.Name}!");
        }

        if (TimeDelayStartsWhen == Trigger.PreviousMissionActive || TimeDelayStartsWhen == Trigger.PreviousMissionFinished) {
            var prevMission = GetParent().GetChild(GetIndex() - 1);
            if (prevMission != null && prevMission is not IMission) {
                throw new System.Exception($"Delay node {Name} has invalid previous mission {prevMission.Name}!");
            }

            MissionToWaitFor = prevMission;
        }

        Name = $"{Name} ({TimeDelayStartsWhen} + {TimeDelaySeconds})";
    }

    public bool AreWeThereYet(List<IMission> missionsInPreparation, IMission activeMission, double currentTime) {
        if (!_isTriggered) {
            bool canStart = TimeDelayStartsWhen switch {
                Trigger.NothingPrepared =>
                    (missionsInPreparation.Count == 0),
                Trigger.AnyMissionIsActive =>
                    (activeMission != null),
                Trigger.PreviousMissionActive or Trigger.SpecificMissionActive =>
                    (activeMission == MissionToWaitFor),
                Trigger.PreviousMissionFinished or Trigger.SpecificMissionFinished =>
                    (activeMission != MissionToWaitFor && !missionsInPreparation.Contains(MissionToWaitFor as IMission)),
                Trigger.AllMissionsFinished =>
                    (missionsInPreparation.Count == 0 && activeMission == null),
                    
                _ => throw new System.NotImplementedException()
            };

            if (!canStart) return false;

            _isTriggered = true;
            _waitUntil = currentTime + TimeDelaySeconds;
        }

        return (currentTime >= _waitUntil);
    }
}