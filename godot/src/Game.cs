using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node {
    [Export]
    private PackedScene _missionPopup;

    private Ship _shipNode;
    private MissionManager _missionsNode;

    private GameUi _ui;
    private Satisfaction _satisfaction;
    private Connectable _selectedMachine;
    private MusicPlayer _musicPlayer;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");
        _ui = GetNode<GameUi>("Player/GameUI");
        _satisfaction = GetNode<Satisfaction>("Satisfaction");
        _musicPlayer = GetNode<MusicPlayer>("MusicPlayer");

        _missionsNode = GetNode<MissionManager>("MissionsAndEvents");

        MissionManager testMissions = GetNodeOrNull<MissionManager>("TestMissions");
        if (testMissions != null) {
            _missionsNode.QueueFree();
            _missionsNode = testMissions;
        }

        _missionsNode.Ship = _shipNode;
        _missionsNode.ShowBriefCallback = (mission) => ShowMissionDialog(mission, true);
        _missionsNode.ShowDebriefCallback = (mission) => ShowMissionDialog(mission, false);
        _missionsNode.MissionCompleteCallback = (mission) => _satisfaction.CheckMissionComplete(mission);
        _missionsNode.MissionDelayCallback = (mission, delta) => _satisfaction.TriggerMissionDelay(delta);

        // _Ready of child nodes will always be first
        foreach (Connectable connectable in _shipNode.Connectables) {
            // some machines cannot be connected to directly
            if (connectable.IsPlayerConnectable) {
                connectable.OnConnectionClick += OnConnectionClick;
            }

            connectable.OnHoverStart += OnHoverStart;
            connectable.OnHoverEnd += OnHoverEnd;
        }

        foreach (Process process in _shipNode.Processes) {
            process.OnProcessingFailed += _satisfaction.CheckProcessFailure;
        }

        foreach (Machine machine in _shipNode.Machines) {
            machine.OnProcessingFailed += _satisfaction.CheckMachineFailure;
        }
    }

    public void ShowMissionDialog(IMission mission, bool briefing) {
        MissionDialog dialog = _missionPopup.Instantiate<MissionDialog>();
        AddChild(dialog);

        _ui.Visible = false;
        dialog.DialogClosed += () => {
            _ui.Visible = true;
            GetTree().Paused = false;
        };
        GetTree().Paused = true;

        IMission.Properties properties = mission.GetMissionProperties();
        dialog.ShowMission(properties.Title, briefing ? properties.Briefing : properties.Debrief);
    }

    public override void _Process(double delta) {
        var quantities = _shipNode.GetTotalResourceQuantities();
        foreach (KeyValuePair<Resource, float> pair in quantities) {
            _ui.ResourceLables[pair.Key].Amount = (int)Mathf.Round(pair.Value);
        }

        Node eventNode;
        while ((eventNode = _shipNode.TryTakeEvent()) != null) {
            AddChild(eventNode);
            if (eventNode is IMission || eventNode is IEvent) {
                // we adopt it as our own
                _missionsNode.AddEvent(eventNode);
            }
        }

        if (_missionsNode.DoPanic()) {
            _musicPlayer.SetTrack(MusicPlayer.MusicTrack.Alarm);
        } else {
            _musicPlayer.SetTrack(MusicPlayer.MusicTrack.Main);
        }

        float satisfaction = _satisfaction.GetSatisfactionLevel();
        _ui.SetSatisfaction(satisfaction);
        if (satisfaction <= 0) GameOver.TriggerGameOver(this, "Satisfaction reached 0%"); // would be nice if you could extract why the satisfaction dropped to 0 aka lack of food etc.
    }

    private void OnConnectionClick(Connectable machine, ConnectionNode node) {
        if (_selectedMachine == null) {
            _selectedMachine = machine;
            _selectedMachine.ShowOutline(true);
        } else if (_selectedMachine == machine) {
            _selectedMachine.ShowOutline(false);
            _selectedMachine = null;
        } else if (_shipNode.CanConnect(_selectedMachine, machine)) {

            Connection connection = new(_selectedMachine, machine);
            Connection disconnect = _shipNode.AddConnection(connection);

            GD.Print($"Connected {connection.aMachine.Name} and {connection.bMachine.Name}");

            if (disconnect != null) {
                GD.Print($"Disconnected {connection.aMachine.Name} and {connection.bMachine.Name}");
            }

            _selectedMachine.ShowOutline(false);
            machine.ShowOutline(false);

            _selectedMachine = null;
        } else {
            _selectedMachine.ShowOutline(false);
            machine.ShowOutline(false);

            _selectedMachine = null;
        }

        string nameOfSelectedMachine = (_selectedMachine != null) ? _selectedMachine.Name : "null";
        GD.Print($"_selectedMachine = {nameOfSelectedMachine}");
    }

    private void OnHoverStart(Connectable machine) {
        if (_selectedMachine == machine) {
            return;
        }

        if (_selectedMachine == null) {
            machine.ShowOutline(true, Connectable.HoverMaterial);
        } else if (!_shipNode.CanConnect(_selectedMachine, machine)) {
            _selectedMachine.ShowOutline(true, Connectable.HoverBadMaterial);
            machine.ShowOutline(true, Connectable.HoverBadMaterial);
        } else {
            _selectedMachine.ShowOutline(true, Connectable.HoverGoodMaterial);
            machine.ShowOutline(true, Connectable.HoverGoodMaterial);
        }
    }

    private void OnHoverEnd(Connectable machine) {
        if (_selectedMachine != null && _selectedMachine != machine) {
            _selectedMachine.ShowOutline(true, Connectable.HoverMaterial);
        } else if (_selectedMachine == machine) {
            return;
        }
        machine.ShowOutline(false);
    }
}
