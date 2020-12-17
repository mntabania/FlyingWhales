using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class ResignData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RESIGN;
    public override string name => "Resign";
    public override string description => "Resign";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public ResignData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            UIManager.Instance.ShowSchemeUI(targetCharacter, null, this);
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            if(character.faction?.factionType.type == FACTION_TYPE.Undead) {
                return false;
            }
            bool isFactionLeaderOrSettlementRuler = character.isFactionLeader || character.isSettlementRuler;
            if (!isFactionLeaderOrSettlementRuler) {
                return false;
            }
        }
        return false;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Resign, character);
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character targetCharacter, object target, bool isSuccessful) {
        ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("You must abandon your current leadership role.", null, DialogItem.Position.Right);
        conversationList.Add(data);
        base.PopulateSchemeConversation(conversationList, targetCharacter, target, isSuccessful);
    }
    #endregion
}
