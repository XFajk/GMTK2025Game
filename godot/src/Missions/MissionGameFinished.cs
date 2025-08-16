using System.Collections.Generic;
using Godot;

// This mission is supposed to be combined with an EventFire
public partial class MissionGameFinished : Node, IMission {
    public IMission.Properties Properties;

    public void MissionReady(Ship ship, MissionManager.Clock missionClock) {
        Properties = new() {
            Title = "Game completed!",
            Briefing = [
                "Thanks for playing our game!",
                "Originally made for the GMTK game jam 2025.",
                "With textures, balancing and many bugfixes added afterwards."
            ],
            Debrief = [
                "With the combined efforts of the team:",
                "XFajk (XFajk) - Programmer",
                "Jozko (juzekk) - Programmer/3D artist",
                "Myani (myani_only) - 2D artist/concept artist",
                "ieperen3039 (ieperen3039) - Programmer",
                "Kai (GuppyKai) - SFX/composer",
                "Dora (dora_gamejam_explorer) - 3D artist ",
            ],
        };
    }

    public void OnStart(Ship ship) {
    }

    public IMission.Properties GetMissionProperties() => Properties;

    public bool IsDelayed() => false;

    public bool IsPreparationFinished() => true;

    public bool IsMissionFinised() => true;

    public void OnCompletion(Ship ship) {
    }
}
