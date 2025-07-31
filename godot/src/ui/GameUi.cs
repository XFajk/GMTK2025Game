using Godot;
using System.Collections.Generic;

public partial class GameUi : Control {

    public Dictionary<Resource, ResourceLable> ResourceLables = new();

    public override void _Ready() {
        Node resourcesNode = GetNode("ResourceExpandButton/Resources");
        foreach (Node node in resourcesNode.GetChildren()) {
            if (node is ResourceLable resourceLable) {
                ResourceLables.Add(resourceLable.Resource, resourceLable);
            }
        }
    }
}
