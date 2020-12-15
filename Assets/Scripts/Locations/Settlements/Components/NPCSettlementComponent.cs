using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSettlementComponent {
    //Base class for all npc settlement components
    public NPCSettlement owner { get; private set; }

    public void SetOwner(NPCSettlement owner) {
        this.owner = owner;
    }
}
