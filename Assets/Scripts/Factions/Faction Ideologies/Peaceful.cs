using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class Peaceful : FactionIdeology {
    public Peaceful() : base(FACTION_IDEOLOGY.Peaceful) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        return true;
    }
    public override bool DoesCharacterFitIdeology(PreCharacterData character) {
        return true;
    }
    public override string GetIdeologyDescription() {
        return "Typically keeps to itself. Does not declare war but will fight back when attacked.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Warmonger);
    }
    #endregion
}
