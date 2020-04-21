using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarassData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.HARASS;
    public override string name { get { return "Harass"; } }
    public override string description { get { return "Harass"; } }

    public HarassData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.HEX };
    }
    #region Overrides
    public override void ActivateAbility(HexTile targetHex) {
        PlayerUI.Instance.OnClickHarassDefendInvade(targetHex, "harass");
        Messenger.Broadcast(Signals.HARASS_ACTIVATED);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            if (targetHex.settlementOnTile != null && targetHex.settlementOnTile is NPCSettlement npcSettlement) {
                return !npcSettlement.isBeingHarassed;
            }
            return false;
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is HexTile targetHex) {
            return targetHex.settlementOnTile != null && targetHex.settlementOnTile.owner != null && targetHex.settlementOnTile.owner.isMajorNonPlayer;
        }
        return false;
    }
    #endregion
}