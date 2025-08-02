using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node {
    private Ship _shipNode;
    private Node _missionsNode;

    private GameUi _ui;

    private Connectable _selectedMachine;
    private ConnectionNode _selectedNode;

    private long _gameTimeSecond = 0;
    private Timer _missionTimer;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");
        _missionsNode = GetNode("MissionsAndEvents");
        _ui = GetNode<GameUi>("Player/GameUI");

        _missionTimer = GetNode<Timer>("MissionTimer");

        _missionTimer.WaitTime = 1.0f;
        _missionTimer.Timeout += () => {
            _gameTimeSecond++;
            Node eventNode = _missionsNode.GetNodeOrNull($"{_gameTimeSecond}");
            ExecuteEventsOfNode(eventNode);
        };

        // _Ready of child nodes will always be first
        foreach (Connectable connectable in _shipNode.Connectables) {
            connectable.OnConnectionClick += OnConnectionClick;
        }
    }

    private void ExecuteEventsOfNode(Node eventNode) {
        if (eventNode is IMission newMission) {
            // 0: `Ready` is called
            newMission.Ready(_shipNode);

            // 1: the player sees the Briefing
            
            // 2: we wait until `IsPreparationFinished`
            // 4: `ApplyEffect` is called
            // 2: we wait until `IsMissionFinised`
            // 6: `OnCompletion` is called
            // 7: the player sees the Debrief
        } else if (eventNode is IEvent newEvent) {
            newEvent.ApplyEffect(_shipNode);
        } else {
            // multiple events in one node
            foreach (Node subNode in eventNode.GetChildren()) {
                ExecuteEventsOfNode(subNode);
            }
        }
    }


    public override void _Process(double delta) {
        var quantities = _shipNode.GetTotalResourceQuantities();
        foreach (KeyValuePair<Resource, float> pair in quantities) {
            _ui.ResourceLables[pair.Key].SetAmount((int) Mathf.Round(pair.Value));
        }
    }

    private void OnConnectionClick(Connectable machine, ConnectionNode node) {
        if (_selectedMachine == null || _selectedMachine == machine) {
            _selectedMachine = machine;
            _selectedNode = node;
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
