using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SummonMinionData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.SUMMON_MINION;
    public override string name { get { return "Summon Minion"; } }
    public override string description { get { return "Summon Minion"; } }

    public SummonMinionData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        //if (structure is Inner_Maps.Location_Structures.ThePortal portal) {
        //    portal.SummonMinion();
        //}
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        bool canPerform = base.CanPerformAbilityTowards(structure);
        if (canPerform) {
            return PlayerManager.Instance.player.mana >= EditableValuesManager.Instance.summonMinionManaCost;
        }
        return canPerform;
    }
    #endregion
}