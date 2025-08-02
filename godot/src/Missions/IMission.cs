

using System.Collections.Generic;
using System.Linq;


/// 0: `Ready` is called
/// 1: the player sees the Briefing
/// 2: we wait until `IsPreparationFinished`
/// 4: `ApplyEffect` is called
/// 2: we wait until `IsMissionFinised`
/// 6: `OnCompletion` is called
/// 7: the player sees the Debrief
public interface IMission : IEvent {
    // will be called just before the Briefing. _Ready is called way earlier
    void Ready(Ship ship) { }
    // TODO maybe return something smarter
    string[] Briefing();
    bool IsPreparationFinished();
    IList<KeyValuePair<Resource, int>> GetMaterialRequirements() => [];
    bool IsMissionFinised();
    void OnCompletion(Ship ship);
    string[] Debrief();

    bool CheckMaterialRequirements(Ship ship) {
        var allResources = ship.GetTotalResourceQuantities();
        return GetMaterialRequirements().All(req => allResources[req.Key] >= req.Value);
    }
}