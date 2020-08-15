using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DemonWorship : FactionIdeology {
    public DemonWorship() : base(FACTION_IDEOLOGY.Demon_Worship) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    public override string GetIdeologyDescription() {
        return "Worships you.";
    }
    #endregion
}
