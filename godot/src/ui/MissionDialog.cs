using Godot;
using System;

public partial class MissionDialog : AcceptDialog {

    private RichTextLabel _description;

    public override void _Ready() {
        _description = GetNode<RichTextLabel>("Description");
    }

    public void ShowMission(string title, string description) {
        Title = title;
        _description.Text = description;
        Popup();
    }
 
}
