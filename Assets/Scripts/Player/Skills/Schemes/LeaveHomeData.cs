using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;
using Locations.Settlements;
using Logs;

public class LeaveHomeData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEAVE_HOME;
    public override string name => "Leave Home";
    public override string description => "Convince a Villager to leave their current Home.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public LeaveHomeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            UIManager.Instance.ShowSchemeUI(targetCharacter, null, this);
        }
    }
    //public override bool IsValid(IPlayerActionTarget target) {
    //    if (target is Character character) {
    //        bool isValid = base.IsValid(target);
    //        return isValid && character.homeStructure != null && !character.isConsideredRatman;
    //    }
    //    return false;
    //}
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.homeStructure == null || targetCharacter.isConsideredRatman) {
                return false;
            }
            return true;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.homeStructure == null) {
            reasons += "Target is already homeless.";
        }
        if (targetCharacter.isConsideredRatman) {
            reasons += "Ratmen cannot leave home.";
        }
        return reasons;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Home, character);

        BaseSettlement homeSettlement = character.homeSettlement;
        if (homeSettlement != null) {
            LocationStructure chosenStructure = homeSettlement.GetRandomStructureThatCharacterHasPathTo(character, character.previousCharacterDataComponent.previousHomeStructure, character.homeStructure);
            if (chosenStructure != null) {
                LocationGridTile chosenTile = chosenStructure.GetRandomPassableTile();
                if(chosenTile != null) {
                    character.jobComponent.CreateGoToJob(chosenTile);
                }
            }
        }
        
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character targetCharacter, object target, bool isSuccessful) {
        ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("I want you to abandon your current home.", null, DialogItem.Position.Right);
        conversationList.Add(data);
        base.PopulateSchemeConversation(conversationList, targetCharacter, target, isSuccessful);
    }
    public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, ref float p_newSuccessRate) {
        p_newSuccessRate *= 2f;
    }
    public override string GetSuccessRateMultiplierText(Character p_targetCharacter) {
        return $"Leave Home: <color=white>x2</color>";
    }
    #endregion
}
