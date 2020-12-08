using System.Collections;
using System.Collections.Generic;
using System;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SchemeData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SCHEME;
    public override string name => "Scheme";
    public override string description => $"Scheme";

    public SchemeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Virtuals
    protected virtual void OnSuccessScheme(Character character, object target) { }
    protected virtual void OnFailScheme(Character character, object target) { }
    protected virtual void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        if (isSuccessful) {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("If you say so.", character, DialogItem.Position.Left);
            conversationList.Add(data);
        } else {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("You can't make me.", character, DialogItem.Position.Left);
            conversationList.Add(data);
        }
    }
    #endregion

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //TODO: On Hover Right Click action activated here? Show all schemes
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.faction != null && targetCharacter.faction.isPlayerFaction) {
                //Characters already part of player faction cannot be targeted by any schemes
                return false;
            }
            return targetCharacter.isDead == false && targetCharacter.limiterComponent.canPerform;
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.faction != null && targetCharacter.faction.isPlayerFaction) {
            reasons += "Characters who are part of the demon faction cannot be targeted.";
        }
        if (targetCharacter.isDead) {
            reasons += "Characters who are dead cannot be targeted.";
        }
        if (!targetCharacter.limiterComponent.canPerform) {
            reasons += "Characters who cannot perform cannot be targeted.";
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is Character character) {
            if (!character.isNormalCharacter || (character.race == RACE.RATMAN && (character.faction == null || !character.faction.isMajorNonPlayer))) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    #endregion

    public void ProcessScheme(Character character, object target, float successRate) {
        bool isSuccessful = GameUtilities.RollChance(successRate);
        if (isSuccessful) {
            OnSuccessScheme(character, target);
        } else {
            OnFailScheme(character, target);
        }
        List<ConversationData> conversationList = ObjectPoolManager.Instance.CreateNewConversationDataList();
        PopulateSchemeConversation(conversationList, character, target, isSuccessful);
        ShowSchemeConversation(conversationList, name);
        OnExecutePlayerSkill();

        //Also start cooldown of SchemeData itself
        //Called this because calling OnExecutePlayerSkill() directly will only start the cooldown of the specific scheme itself (ex. Resign), but will not start the cooldown of SchemeData
        PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME).OnExecutePlayerSkill();
    }

    protected void ShowSchemeConversation(List<ConversationData> conversationList, string titleText) {
        UIManager.Instance.OpenConversationMenu(conversationList, titleText);

        for (int i = 0; i < conversationList.Count; i++) {
            ObjectPoolManager.Instance.ReturnConversationDataToPool(conversationList[i]);
        }
        ObjectPoolManager.Instance.ReturnConversationDataListToPool(conversationList);
    }
}