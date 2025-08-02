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

    private Connectable _selectedMachine;
    private ConnectionNode _selectedNode;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");
        _ui = GetNode<GameUi>("Player/GameUI");

        _missionsNode = GetNode<MissionManager>("MissionsAndEvents");
        _missionsNode.Ship = _shipNode;
        _missionsNode.ShowBriefCallback = (mission) => ShowMissionDialog(mission, true);
        _missionsNode.ShowDebriefCallback = (mission) => ShowMissionDialog(mission, false);

        // _Ready of child nodes will always be first
        foreach (Connectable connectable in _shipNode.Connectables) {
            connectable.OnConnectionClick += OnConnectionClick;
        }
    }

    public void ShowMissionDialog(IMission mission, bool briefing) {
        MissionDialog dialog = _missionPopup.Instantiate<MissionDialog>();
        AddChild(dialog);

        void unpause() => GetTree().Paused = false;
        dialog.Confirmed += unpause;
        dialog.Canceled += unpause;
        GetTree().Paused = true;

        IMission.Properties properties = mission.GetMissionProperties();
        dialog.ShowMission(briefing ? "New Mission" : "Mission Success", properties.Title, briefing ? properties.Briefing : properties.Debrief);
    }

    public override void _Process(double delta) {
        var quantities = _shipNode.GetTotalResourceQuantities();
        foreach (KeyValuePair<Resource, float> pair in quantities) {
            _ui.ResourceLables[pair.Key].SetAmount((int)Mathf.Round(pair.Value));
        }

        Node eventNode;
        while ((eventNode = _shipNode.TryTakeEvent()) != null) {
            AddChild(eventNode);
            if (eventNode is IMission || eventNode is IEvent) {
                // we adopt it as our own
                _missionsNode.AddEvent(eventNode);
            }
        }
    }

    private void OnConnectionClick(Connectable machine, ConnectionNode node) {
        if (_selectedMachine == null) {
            _selectedMachine = machine;
            _selectedNode = node;
        } else if (_selectedMachine == machine) {
            _selectedNode.DisconnectNode();
            _selectedMachine = null;
            _selectedNode = null;
        } else if (Connectable.CanConnect(_selectedMachine, machine)) {
            _shipNode.AddConnection(_selectedMachine, machine);
            ConnectionNode.ConnectNodes(_selectedNode, node);

            _selectedMachine = null;
            _selectedNode = null;
        } else {
            _selectedNode.DeclineConnection();
            node.DeclineConnection();

            _selectedMachine = null;
            _selectedNode = null;
        }

        string nameOfSelectedMachine = (_selectedMachine != null) ? _selectedMachine.Name : "null";
        string nameOfSelectedNode = (_selectedNode != null) ? _selectedNode.Name : "null";
        GD.Print($"_selectedMachine = {nameOfSelectedMachine}, _selectedNode = {nameOfSelectedNode}");
    }

}
