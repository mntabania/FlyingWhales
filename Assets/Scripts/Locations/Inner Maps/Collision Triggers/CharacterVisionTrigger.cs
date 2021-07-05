using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterVisionTrigger : POIVisionTrigger {

    [SerializeField] private bool _ignoreStructureDifference = false;
    [SerializeField] private bool _ignoreRoomDifference = false;
    
    public override void Initialize(IDamageable damageable) {
        base.Initialize(damageable);
        VoteToMakeVisibleToCharacters(); //characters, by default, can be seen by everything.
    }
    public override bool IgnoresStructureDifference() {
        return _ignoreStructureDifference;
    }
    public override bool IgnoresRoomDifference() {
        return _ignoreRoomDifference;
    }
}
