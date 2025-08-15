using Godot;
using System;
using System.Collections.Generic;

public abstract partial class TimedMission : Node, IMission {
    [Export]
    public int PreparationTime = 10;
    [Export]
    public int Duration;

    protected MissionManager.Clock _missionClock;
    protected double _preparationEndTime;
    protected double _missionEndTime;
    protected IMission.State _state { get; private set; }

    public virtual void MissionReady(Ship ship, MissionManager.Clock missionClock) {
        _missionClock = missionClock;
        _preparationEndTime = missionClock.time + PreparationTime;
        _state = IMission.State.Preparing;
    }

    public virtual void OnStart(Ship ship) {
        _missionEndTime = _missionClock.time + Duration;
        _state = IMission.State.Started;
    }

    public virtual bool IsPreparationFinished() {
        return _preparationEndTime != 0 && _missionClock.time > _preparationEndTime;
    }

    public virtual bool IsMissionFinised() {
        return _missionEndTime != 0 && _missionClock.time > _missionEndTime;
    }

    public abstract IMission.Properties GetMissionProperties();
    public virtual void OnCompletion(Ship ship) {
        _state = IMission.State.Finished;
    }

    public abstract bool IsDelayed();
}
