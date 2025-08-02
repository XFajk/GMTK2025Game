using Godot;
using System;

public partial class MissionDialog : AcceptDialog {
    private RichTextLabel _description;
    private Label _missionTitle;

    public override void _Ready() {
        _description = GetNode<RichTextLabel>("UI/Description");
        _missionTitle = GetNode<Label>("UI/MissionTitle");
    }

    public void ShowMission(string windowTitle, string title, string[] description) {
        Title = windowTitle;
        _description.Text = string.Join(System.Environment.NewLine, description);
        _missionTitle.Text = title;
        Popup();
    }
}
