using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warmonger : FactionIdeology {

    public Warmonger() : base(FACTION_IDEOLOGY.Warmonger) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) { return true; }
    #endregion
}
