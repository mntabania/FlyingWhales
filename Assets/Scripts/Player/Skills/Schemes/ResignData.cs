using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class ResignData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RESIGN;
    public override string name => "Resign";
    public override string description => "Force a Faction Leader or Settlement Ruler to resign.";
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
    //public override bool IsValid(IPlayerActionTarget target) {
    //    bool isValid = base.IsValid(target);
    //    if (isValid) {
    //        if (target is Character character) {
    //            if (character.faction?.factionType.type == FACTION_TYPE.Undead) {
    //                return false;
    //            }
    //            bool isFactionLeaderOrSettlementRuler = character.isFactionLeader || character.isSettlementRuler;
    //            if (!isFactionLeaderOrSettlementRuler) {
    //                return false;
    //            }
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.faction?.factionType.type == FACTION_TYPE.Undead) {
                return false;
            }
            bool isFactionLeaderOrSettlementRuler = targetCharacter.isFactionLeader || targetCharacter.isSettlementRuler;
            if (!isFactionLeaderOrSettlementRuler) {
                return false;
            }
            return true;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.faction?.factionType.type == FACTION_TYPE.Undead) {
            reasons += "Undead characters cannot resign.";
        }
        bool isFactionLeaderOrSettlementRuler = targetCharacter.isFactionLeader || targetCharacter.isSettlementRuler;
        if (!isFactionLeaderOrSettlementRuler) {
            reasons += "Only faction leaders and settlement rulers can resign.";
        }
        return reasons;
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
