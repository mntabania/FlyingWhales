using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class BreedMonsterData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.BREED_MONSTER;
    public override string name { get { return "Breed Monster"; } }
    public override string description { get { return "Breed Monster"; } }

    public BreedMonsterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is Inner_Maps.Location_Structures.TheKennel theKennel) {
            theKennel.OnClickBreedMonster();
        }
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        if (structure is Inner_Maps.Location_Structures.TheKennel theKennel) {
            return theKennel.CanDoBreedMonster();
        }
        return false;
    }
    #endregion
}