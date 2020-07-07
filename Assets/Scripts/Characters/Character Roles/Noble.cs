using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noble : CharacterRole {
    public override int reservedSupply { get { return 30; } }

    public Noble() : base(CHARACTER_ROLE.NOBLE, "Noble") {
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
