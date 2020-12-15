using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionComponent {
    //Base class for all faction components
    public Faction owner { get; private set; }

    public void SetOwner(Faction owner) {
        this.owner = owner;
    }
}
