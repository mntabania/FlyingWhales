using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bandit : CharacterRole {

    public Bandit() : base(CHARACTER_ROLE.BANDIT, "Normal") {
        //allowedInteractions = new INTERACTION_TYPE[] {
        //    INTERACTION_TYPE.OBTAIN_RESOURCE,
        //    INTERACTION_TYPE.ASSAULT,
        //};
    }
}
