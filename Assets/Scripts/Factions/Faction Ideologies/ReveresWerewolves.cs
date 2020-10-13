using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class ReveresWerewolves : FactionIdeology {
    public ReveresWerewolves() : base(FACTION_IDEOLOGY.Reveres_Werewolves) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    public override bool DoesCharacterFitIdeology(PreCharacterData character) {
        return true;
    }
    public override string GetIdeologyDescription() {
        return "Has a high opinion towards Werewolves.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Hates_Werewolves);
    }
    #endregion
}