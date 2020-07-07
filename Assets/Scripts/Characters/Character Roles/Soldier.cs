using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : CharacterRole {
    public override int reservedSupply { get { return 30; } }

    public Soldier() : base(CHARACTER_ROLE.SOLDIER, "Normal") {
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
