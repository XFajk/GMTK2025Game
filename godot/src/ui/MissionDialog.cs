using Godot;
using System;

public partial class MissionDialog : Control {
    [Signal]
    public delegate void DialogClosedEventHandler();

    private RichTextLabel _description;
    private Label _missionTitle;
    private Button _closeButton;

    public override void _Ready() {
        _description = GetNode<RichTextLabel>("BackGround/Description");
        _missionTitle = GetNode<Label>("BackGround/MissionTitle");
        _closeButton = GetNode<Button>("BackGround/Close");
        _closeButton.Pressed += () => {
            EmitSignalDialogClosed();
            QueueFree();
        };
    }

    public void ShowMission(string title, string[] description) {
        _description.Text = string.Join(System.Environment.NewLine, description);
        _missionTitle.Text = title;
        Visible = true;
    }
}
