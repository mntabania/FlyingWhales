using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Peaceful : FactionIdeology {
    public Peaceful() : base(FACTION_IDEOLOGY.Peaceful) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        return true;
    }
    public override string GetIdeologyDescription() {
        return "Typically keeps to itself. Does not declare war but will fight back when attacked.";
    }
    #endregion
}
