using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class LeaveHomeData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEAVE_HOME;
    public override string name => "Leave Home";
    public override string description => "Leave Home";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public LeaveHomeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            return character.homeStructure != null;
        }
        return base.IsValid(target);
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Home, character);
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character targetCharacter, object target, bool isSuccessful) {
        ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("I want you to abandon your current home.", null, DialogItem.Position.Right);
        conversationList.Add(data);
        base.PopulateSchemeConversation(conversationList, targetCharacter, target, isSuccessful);
    }
    #endregion
}
