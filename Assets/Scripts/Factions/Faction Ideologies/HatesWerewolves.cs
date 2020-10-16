using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class HatesWerewolves : FactionIdeology {
    public HatesWerewolves() : base(FACTION_IDEOLOGY.Hates_Werewolves) {

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
        return "May hunt down known Werewolves.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Reveres_Werewolves);
        factionType.AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
    }
    #endregion
}