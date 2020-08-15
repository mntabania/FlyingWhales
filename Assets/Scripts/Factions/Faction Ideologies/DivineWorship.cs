using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DivineWorship : FactionIdeology {
    public DivineWorship() : base(FACTION_IDEOLOGY.Divine_Worship) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    public override string GetIdeologyDescription() {
        return "Worships the Goddess.";
    }
    #endregion
}
