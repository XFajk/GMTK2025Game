using Godot;
using System;
using System.Collections.Generic;

public partial class MissionManager : Node {
    // we only have one active mission at any time, but we can prepare multiple in parallel
    public List<IMission> MissionsInPreparation = new();
    public IMission ActiveMission = null;

    public Action<IMission> ShowBriefCallback;
    public Action<IMission> ShowDebriefCallback;
    public Ship Ship;

    private double _gameTimeSecond = 0;
    private int progress = -1;

    [Export]
    int InitialTimeDelay = 0;

    private Delay _currentDelay;

    public override void _Ready() {
        _currentDelay = new Delay() {
            TimeDelaySeconds = InitialTimeDelay
        };
    }


    public override void _Process(double delta) {
        _gameTimeSecond += delta;

        if (_currentDelay.AreWeThereYet(MissionsInPreparation, ActiveMission, _gameTimeSecond)) {
            _currentDelay = null;
        }

        while (_currentDelay == null) {
            progress++;
            Node eventNode = GetChild(progress);
            // game end?
            if (eventNode == null) break;

            GD.Print($"Time = {_gameTimeSecond}, Node = {progress} ({eventNode.Name})");
            ExecuteEventsOfNode(eventNode);
        }

        if (ActiveMission == null) {
            foreach (IMission mission in MissionsInPreparation) {
                // 3: we wait until `IsPreparationFinished`
                if (!mission.IsPreparationFinished()) continue;
                /// 4: `ApplyEffect` is called
                mission.OnStart(Ship);
            }
        } else {
            /// 5: we wait until `IsMissionFinised`
            if (ActiveMission.IsMissionFinised()) {
                /// 6: `OnCompletion` is called
                ActiveMission.OnCompletion(Ship);
                /// 7: the player sees the Debrief
                ShowDebriefCallback(ActiveMission);
                ActiveMission = null;
            }
        }
    }

    private void ExecuteEventsOfNode(Node eventNode) {
        if (eventNode is IMission newMission) {
            // 0: `Ready` is called
            newMission.Ready(Ship);

            /// 1: `GetTitle` is called
            /// 2: the player sees the Briefing
            ShowBriefCallback.Invoke(newMission);
            MissionsInPreparation.Add(newMission);
            /// 3: we wait until `IsPreparationFinished`

        } else if (eventNode is IEvent newEvent) {
            newEvent.ApplyEffect(Ship);
        } else if (eventNode is Delay delay) {
            _currentDelay = delay;
        }
    }
}
