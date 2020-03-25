using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class LearnSpellData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.LEARN_SPELL;
    public override string name { get { return "Learn Spell"; } }
    public override string description { get { return "Learn Spell"; } }

    public LearnSpellData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is Inner_Maps.Location_Structures.TheSpire theSpire) {
            theSpire.TryLearnASpellOrAffliction();
        }
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        if (structure is Inner_Maps.Location_Structures.TheSpire theSpire) {
            return theSpire.CanLearnSpell();
        }
        return false;
    }
    #endregion
}