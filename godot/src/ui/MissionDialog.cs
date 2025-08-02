using Godot;
using System;

public partial class MissionDialog : AcceptDialog {
    private RichTextLabel _description;
    private Label _missionTitle;

    public override void _Ready() {
        _description = GetNode<RichTextLabel>("UI/Description");
        _missionTitle = GetNode<Label>("UI/MissionTitle");
    }

    public void ShowMission(string windowTitle, string title, string description) {
        Title = title;
        _description.Text = description;
        _missionTitle.Text = title;
        Popup();
    }
}
