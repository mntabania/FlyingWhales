using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Warmonger : FactionIdeology {

    public Warmonger() : base(FACTION_IDEOLOGY.Warmonger) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) { return true; }
    public override string GetIdeologyDescription() {
        return "Likely to declare war at its Leader's behest.";
    }
    #endregion
}
