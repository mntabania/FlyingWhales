using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using Inner_Maps;

public class LeaveFactionData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEAVE_FACTION;
    public override string name => "Leave Faction";
    public override string description => "Convince a Villager to leave their current Faction.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public LeaveFactionData() : base() {
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
    //            bool isNonVagrant = character.faction != null && character.faction.isMajorNonPlayer;
    //            bool isRatman = character.isConsideredRatman;
    //            if (isNonVagrant || isRatman) { // && !character.isFactionLeader && !character.isSettlementRuler
    //                return true;
    //            } else {
    //                return false;
    //            }
    //        }
    //    }
    //    return false;
    //}
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            bool isNonVagrant = targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer;
            bool isRatman = targetCharacter.isConsideredRatman;
            if (!isNonVagrant && !isRatman) {
                return false;
            }
            return true;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        bool isNonVagrant = targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer;
        bool isRatman = targetCharacter.isConsideredRatman;
        if (!isNonVagrant && !isRatman) {
            reasons += "Target is already a vagrant and is not considered a ratman.";
        }
        return reasons;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
        Area chosenArea = character.currentRegion.GetRandomAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage();
        if (chosenArea != null) {
            LocationGridTile chosenTile = chosenArea.gridTileComponent.GetRandomPassableTile();
            if (chosenTile != null) {
                character.jobComponent.CreateGoToJob(chosenTile);
            }
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("You have to leave your current faction immediately.", null, DialogItem.Position.Right);
        conversationList.Add(data);
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, ref float p_newSuccessRate) {
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            p_newSuccessRate *= 2f;
        }
        if (p_targetCharacter.isFactionLeader || p_targetCharacter.isSettlementRuler) {
            p_newSuccessRate *= 0.25f;
        }
        base.ProcessSuccessRateWithMultipliers(p_targetCharacter, ref p_newSuccessRate);
    }
    public override string GetSuccessRateMultiplierText(Character p_targetCharacter) {
        string text = string.Empty;
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Treacherous: <color=white>x2</color>";
        }
        if (p_targetCharacter.isFactionLeader) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is the Faction Leader: <color=white>x0.25</color>";
        } else if (p_targetCharacter.isSettlementRuler) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is a Settlement Ruler: <color=white>x0.25</color>";
        }
        if (text != string.Empty) {
            return text;
        }
        return base.GetSuccessRateMultiplierText(p_targetCharacter);
    }
    #endregion
}
