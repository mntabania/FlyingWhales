using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyComponent {
    public Party owner { get; private set; }

    public void SetOwner(Party owner) {
        this.owner = owner;
    }
}
