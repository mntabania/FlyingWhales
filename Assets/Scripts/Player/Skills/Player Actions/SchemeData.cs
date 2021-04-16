using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using Locations.Settlements;

public class SchemeData : PlayerAction {

    public static bool alwaysSuccessScheme = false;
    
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SCHEME;
    public override string name => "Scheme";
    public override string description => $"This Action can be used to start various different schemes to manipulate world events.";

    public virtual string verbName => name;

    public SchemeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Virtuals
    protected virtual void OnSuccessScheme(Character character, object target) {
        LogSchemeCharacter(character, true);
    }
    protected virtual void OnFailScheme(Character character, object target) {
        LogSchemeCharacter(character, false);
    }
    protected virtual void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        if (isSuccessful) {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("If you say so.", character, DialogItem.Position.Left);
            conversationList.Add(data);
        } else {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("You can't make me.", character, DialogItem.Position.Left);
            conversationList.Add(data);
        }
    }
    public virtual void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, ref float p_newSuccessRate) { }
    public virtual string GetSuccessRateMultiplierText(Character p_targetCharacter) { return string.Empty; }
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
        BaseSettlement targetSettlement = null;
        if (target is LocationStructure structure) {
            targetSettlement = structure.settlementLocation;
        } else if (target is BaseSettlement settlement) {
            targetSettlement = settlement;
        }
        if (target is Character character) {
            bool isNormalOrRatman = character.isNormalCharacter || character.isConsideredRatman;
            if (!isNormalOrRatman) {
                return false;
            }
        } else if (targetSettlement != null) {
            if (targetSettlement.locationType != LOCATION_TYPE.VILLAGE || !(targetSettlement is NPCSettlement)) {
                return false;
            }
        }
        return base.IsValid(target) && PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement != null && 
               PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER);
    }
    protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems) {
        if (type == PLAYER_SKILL_TYPE.SCHEME && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null) {
            p_contextMenuItems.Clear();
            List<PLAYER_SKILL_TYPE> schemeTypes = PlayerManager.Instance.player.playerSkillComponent.schemes;
            for (int i = 0; i < schemeTypes.Count; i++) {
                PLAYER_SKILL_TYPE spellType = schemeTypes[i];
                PlayerAction spellData = PlayerSkillManager.Instance.GetSkillData(spellType) as PlayerAction;
                if (spellData != null && spellData.IsValid(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget)) {
                    p_contextMenuItems.Add(spellData);
                }
            }
            return p_contextMenuItems;    
        }
        return null;
    }
    #endregion

    //Return true if Scheme is successful
    public bool ProcessScheme(Character character, object target, float successRate) {
        bool isSuccessful = alwaysSuccessScheme || GameUtilities.RollChance(successRate);

        List<ConversationData> conversationList = ObjectPoolManager.Instance.CreateNewConversationDataList();
        PopulateSchemeConversation(conversationList, character, target, isSuccessful);
        ShowSchemeConversation(conversationList, name);

        if (isSuccessful) {
            OnSuccessScheme(character, target);
        } else {
            OnFailScheme(character, target);
        }
        OnExecutePlayerSkill();

        //Also start cooldown of SchemeData itself
        //Called this because calling OnExecutePlayerSkill() directly will only start the cooldown of the specific scheme itself (ex. Resign), but will not start the cooldown of SchemeData
        PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME).OnExecutePlayerSkill();
        return isSuccessful;
    }
    private void ShowSchemeConversation(List<ConversationData> conversationList, string titleText) {
        UIManager.Instance.OpenConversationMenu(conversationList, titleText);

        for (int i = 0; i < conversationList.Count; i++) {
            ObjectPoolManager.Instance.ReturnConversationDataToPool(conversationList[i]);
        }
        ObjectPoolManager.Instance.ReturnConversationDataListToPool(conversationList);
    }
    private void LogSchemeCharacter(Character p_targetCharacter, bool isSuccessful) {
        string key = "success_scheme_character";
        if (!isSuccessful) {
            key = "fail_scheme_character";
        }
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", key, null, LOG_TAG.Player);
        log.AddToFillers(p_targetCharacter, p_targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(name), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_2);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
    }
    protected void LogSchemeVillage(BaseSettlement p_targetSettlement) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "player_scheme_village", null, LOG_TAG.Player);
        log.AddToFillers(p_targetSettlement, p_targetSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(name), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_2);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
    }
}