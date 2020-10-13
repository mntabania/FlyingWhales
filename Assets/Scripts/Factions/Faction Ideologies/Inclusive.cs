using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class Inclusive : FactionIdeology {

    public Inclusive() : base(FACTION_IDEOLOGY.Inclusive) {

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
        return "Open to everyone.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive);
    }
    #endregion
}
