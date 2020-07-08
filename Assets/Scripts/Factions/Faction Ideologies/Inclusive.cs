using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inclusive : FactionIdeology {

    public Inclusive() : base(FACTION_IDEOLOGY.Inclusive) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    #endregion
}
