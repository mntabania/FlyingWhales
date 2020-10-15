using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class HatesVampires : FactionIdeology {
    public HatesVampires() : base(FACTION_IDEOLOGY.Hates_Vampires) {

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
        return "May hunt down known Vampires.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Reveres_Vampires);
        factionType.AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
    }
    #endregion
}