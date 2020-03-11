using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : CharacterRole {
    public override int reservedSupply { get { return 50; } }

    public Adventurer() : base(CHARACTER_ROLE.ADVENTURER, "Normal", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY, INTERACTION_CATEGORY.RECRUITMENT, INTERACTION_CATEGORY.EXPANSION, INTERACTION_CATEGORY.DEFENSE }) {
        //allowedInteractions = new INTERACTION_TYPE[] {
        //    INTERACTION_TYPE.OBTAIN_RESOURCE,
        //    INTERACTION_TYPE.ASSAULT,
        //};
        // requiredItems = new SPECIAL_TOKEN[] {
        //     SPECIAL_TOKEN.TOOL,
        //     SPECIAL_TOKEN.HEALING_POTION
        // };
    }
}
