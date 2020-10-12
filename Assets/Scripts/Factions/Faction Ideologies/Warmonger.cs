using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class Warmonger : FactionIdeology {

    public Warmonger() : base(FACTION_IDEOLOGY.Warmonger) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) { return true; }
    public override bool DoesCharacterFitIdeology(PreCharacterData character) {
        return true;
    }
    public override string GetIdeologyDescription() {
        return "Likely to declare war at its Leader's behest.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Peaceful);
    }
    #endregion
}
