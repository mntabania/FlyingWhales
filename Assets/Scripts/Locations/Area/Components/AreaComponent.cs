using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaComponent {
    public Area owner { get; private set; }

    public void SetOwner(Area owner) {
        this.owner = owner;
    }
}
