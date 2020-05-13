using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class DefendData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.DEFEND;
    public override string name { get { return "Defend"; } }
    public override string description { get { return "Defend"; } }

    public DefendData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.HEX };
    }
    #region Overrides
    public override void ActivateAbility(LocationStructure targetStructure) {
        PlayerUI.Instance.OnClickHarassDefendInvade(targetStructure.occupiedHexTile.hexTileOwner, "defend");
        Messenger.Broadcast(Signals.DEFEND_ACTIVATED);
    }
    public override bool CanPerformAbilityTowards(LocationStructure targetStructure) {
        bool canPerform = base.CanPerformAbilityTowards(targetStructure);
        if (canPerform) {
            return targetStructure.hasBeenDestroyed == false && !targetStructure.occupiedHexTile.hexTileOwner.isBeingDefended;
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is LocationStructure targetStructure) {
            return targetStructure is DemonicStructure;
        }
        return false;
    }
    #endregion
}