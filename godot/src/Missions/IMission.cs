

using System.Collections.Generic;


/// 0: Ready is called
/// 1: the player sees the Briefing
/// 2: we wait `GetPreparationTime` seconds
/// 3: `GetMaterialRequirements` are taken from the ship's resources
/// 4: `ApplyEffect` is called
/// 5: we wait `GetDuration` seconds
/// 6: `OnCompletion` is called
/// 7: the player sees the Debrief
public interface IMission : IEvent {
    // gives some time 
    void Ready() { }
    // TODO maybe return something smarter
    string[] Briefing();
    IList<KeyValuePair<Resource, int>> GetMaterialRequirements() => [];
    float GetPreparationTime();
    float GetDuration();
    string[] Debrief();

    // TODO
    // void RemoveMaterialRequirements(Ship ship) {
    //     foreach (var pair in mission.GetMaterialRequirements()) {
    //         ship.RemoveResource(pair.Key, pair.Value);
    //     }
    // }
}