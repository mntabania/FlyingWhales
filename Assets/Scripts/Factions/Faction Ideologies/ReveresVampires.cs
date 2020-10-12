using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return "Loves Vampires.";
    }
    #endregion
}