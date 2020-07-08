using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peaceful : FactionIdeology {
    public Peaceful() : base(FACTION_IDEOLOGY.Peaceful) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        return true;
    }
    #endregion
}
