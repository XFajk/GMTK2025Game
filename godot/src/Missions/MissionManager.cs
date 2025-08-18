using Godot;
using System;
using System.Collections.Generic;

public partial class MissionManager : Node {

    // we only have one active mission at any time, but we can prepare multiple in parallel
    public List<IMission> MissionsInPreparation = new();
    public IMission ActiveMission = null;
    public List<IEvent> CausesForPanic = new();

    public Action<IMission> ShowBriefCallback;
    public Action<IMission> ShowDebriefCallback;
    public Action<IMission> MissionCompleteCallback;
    public Action<IMission, double> MissionDelayCallback;
    public Ship Ship;

    private Clock _missionClock = new();
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
        _missionClock.delta = delta;
        _missionClock.time += delta;

        if (_currentDelay == null) {
            GD.PrintErr($"_currentDelay = null, progress = {progress}, ActiveMission = {ActiveMission}");

            int offset = 0;
            while (_currentDelay == null) {
                Node eventNode = GetChild(progress - offset++);
                if (eventNode == null) return;
                if (eventNode is Delay delay) _currentDelay = delay;
            }
        }

        if (_currentDelay.AreWeThereYet(MissionsInPreparation, ActiveMission, _missionClock.time)) {
            _currentDelay = null;
        }

        while (_currentDelay == null) {
            progress++;
            Node eventNode = GetChild(progress);
            // game end?
            if (eventNode == null) break;

            GD.Print($"Time = {_missionClock.time}, Node = {progress} ({eventNode.Name})");
            ExecuteEventsOfNode(eventNode);
        }

        CausesForPanic.RemoveAll(e => !e.DoPanic());

        if (ActiveMission == null) {
            foreach (IMission mission in MissionsInPreparation) {
                // 3: we wait until `IsPreparationFinished`
                if (!mission.IsPreparationFinished()) continue;
                /// 4: `ApplyEffect` is called
                mission.OnStart(Ship);
                ActiveMission = mission;
                break;
            }
            MissionsInPreparation.Remove(ActiveMission);
        } else {
            /// 5: we wait until `IsMissionFinised`
            if (ActiveMission.IsMissionFinised()) {
                /// 6: `OnCompletion` is called
                ActiveMission.OnCompletion(Ship);
                MissionCompleteCallback(ActiveMission);
                /// 7: the player sees the Debrief
                if (ActiveMission.GetMissionProperties().Popup) {
                    ShowDebriefCallback.Invoke(ActiveMission);
                }
                ActiveMission = null;
            }
        }

        foreach (IMission mission in MissionsInPreparation) {
            if (mission.IsDelayed()) {
                MissionDelayCallback(mission, delta);
            }
        }
        if (ActiveMission != null && ActiveMission.IsDelayed() == true) {
            MissionDelayCallback(ActiveMission, delta);
        }
    }

    public bool DoPanic() {
        return CausesForPanic.Count > 0;
    }

    private void ExecuteEventsOfNode(Node eventNode) {
        if (eventNode is IMission newMission) {
            // 1: `Ready` is called
            newMission.MissionReady(Ship, _missionClock);

            /// 2: the player sees the Briefing
            if (newMission.GetMissionProperties().Popup) {
                ShowBriefCallback.Invoke(newMission);
            }

            MissionsInPreparation.Add(newMission);
            /// 3: we wait until `IsPreparationFinished`

        } else if (eventNode is IEvent newEvent) {
            newEvent.ApplyEffect(Ship);
            CausesForPanic.Add(newEvent);

        } else if (eventNode is Delay delay) {
            _currentDelay = delay;

        } else if (eventNode is JumpToMission jump && jump.ToNode != null) {
            progress = jump.ToNode.GetIndex() - 1;
        }
    }

    public void AddEvent(Node eventNode) => ExecuteEventsOfNode(eventNode);

    public class Clock {
        public double time = 0;
        public double delta = 0;
    }
}
