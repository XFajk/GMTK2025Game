

using System.Collections.Generic;
using System.Linq;


/// 0: `Ready` is called
/// 1: `GetMissionProperties` is called
/// 2: the player sees the Briefing
/// 3: we wait until `IsPreparationFinished`
/// 4: `OnStart` is called
/// 5: we wait until `IsMissionFinised`
/// 6: `OnCompletion` is called
/// 7: the player sees the Debrief
public interface IMission {
    Properties GetMissionProperties();

    // will be called just before the Briefing. _Ready is called way earlier
    virtual void MissionReady(Ship ship) { }
    bool IsPreparationFinished();
    void OnStart(Ship ship);
    bool IsMissionFinised();
    void OnCompletion(Ship ship);

    bool CheckMaterialRequirements(Ship ship) {
        Properties props = GetMissionProperties();
        var allResources = ship.GetTotalResourceQuantities();
        return props.ResourceMinimumRequirements.All(req => allResources[req.Key] >= req.Value)
            && props.ResourceMaximumRequirements.All(req => allResources[req.Key] <= req.Value);
    }

    public class Properties {
        public string Title;
        public string[] Briefing;
        public KeyValuePair<Resource, int>[] ResourceMinimumRequirements = [];
        public KeyValuePair<Resource, int>[] ResourceMaximumRequirements = [];
        public string[] Debrief;
        public bool Popup = true;
    }
}