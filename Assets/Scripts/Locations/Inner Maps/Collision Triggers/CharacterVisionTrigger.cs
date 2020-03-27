using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterVisionTrigger : POIVisionTrigger {
    public override void Initialize(IDamageable damageable) {
        base.Initialize(damageable);
        VoteToMakeVisibleToCharacters(); //characters, by default, can be seen by everything.
    }
    public override bool IgnoresStructureDifference() {
        return false;
    }
}
