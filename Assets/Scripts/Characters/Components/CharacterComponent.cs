using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterComponent {
    //Base class for all character components
    public Character owner { get; private set; }

    public virtual void SetOwner(Character owner) {
        this.owner = owner;
    }
    public virtual void CleanUp() {
        SetOwner(null);
    }
}
