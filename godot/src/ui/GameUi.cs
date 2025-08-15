using Godot;
using System.Collections.Generic;

public partial class GameUi : Control {

    public Dictionary<Resource, ResourceLable> ResourceLables = new();

    public ProgressBar _satisfactionIndicator;

    public override void _Ready() {
        Node resourcesNode = GetNode("ResourceExpandButton/Resources");
        foreach (Node node in resourcesNode.GetChildren()) {
            if (node is ResourceLable resourceLable) {
                ResourceLables.Add(resourceLable.Resource, resourceLable);
            }
        }

        _satisfactionIndicator = GetNode<ProgressBar>("CrewSatisfactionBar");
    }

    public void SetSatisfaction(float fraction) {
        _satisfactionIndicator.Value = Mathf.Lerp(_satisfactionIndicator.MinValue, _satisfactionIndicator.MaxValue, fraction);
    }
}
