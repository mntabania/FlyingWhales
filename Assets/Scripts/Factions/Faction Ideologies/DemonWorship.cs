using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonWorship : FactionIdeology {
    public DemonWorship() : base(FACTION_IDEOLOGY.Demon_Worship) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    #endregion
}
