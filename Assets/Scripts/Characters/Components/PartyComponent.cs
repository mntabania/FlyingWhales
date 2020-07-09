using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyComponent {
    public Character owner { get; private set; }

    public Party currentParty { get; private set; }

    #region getters
    public bool hasParty => currentParty != null;
    #endregion

    public PartyComponent(Character owner) {
        this.owner = owner;
    }

    public void SetCurrentParty(Party party) {
        currentParty = party;
    }
}
