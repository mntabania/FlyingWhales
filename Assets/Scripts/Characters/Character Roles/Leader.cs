using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : CharacterRole {
    public override int reservedSupply { get { return 50; } }

    public Leader() : base(CHARACTER_ROLE.LEADER, "Leader", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DIPLOMACY, INTERACTION_CATEGORY.DEFENSE }) {
        //allowedInteractions = new INTERACTION_TYPE[] {
        //    INTERACTION_TYPE.OBTAIN_RESOURCE,
        //    INTERACTION_TYPE.ASSAULT,
        //};
        // requiredItems = new SPECIAL_TOKEN[] {
        //     SPECIAL_TOKEN.HEALING_POTION,
        //     SPECIAL_TOKEN.HEALING_POTION
        // };
    }
}
