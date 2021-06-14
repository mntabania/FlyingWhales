using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EyeWardVisionTrigger : TileObjectVisionTrigger {
    public override void SetVisionTriggerCollidersState(bool state) {
        _mainCollider.enabled = false;
    }
    public override void SetFilterVotes(int votes) {
        //do nothing
    }
    
    public override void VoteToMakeVisibleToCharacters() {
        //do nothing
    }

    public override void VoteToMakeInvisibleToCharacters() {
        //do nothing
    }

    public override void SetAllCollidersState(bool state) {
        _mainCollider.enabled = false;
        _projectileReceiver.SetColliderState(state);
    }
}
