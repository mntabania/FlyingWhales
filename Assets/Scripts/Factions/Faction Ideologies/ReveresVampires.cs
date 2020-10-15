using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class ReveresVampires : FactionIdeology {
    public ReveresVampires() : base(FACTION_IDEOLOGY.Reveres_Vampires) {

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
        return "Has a high opinion towards Vampires.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Hates_Vampires);
        factionType.RemoveCrime(CRIME_TYPE.Vampire);
    }
    #endregion
}