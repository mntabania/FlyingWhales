using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class OverthrowLeaderData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.OVERTHROW_LEADER;
    public override string name => "Overthrow Leader";
    public override string description => "Convince a Successor to depose the current Faction Leader.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public OverthrowLeaderData() : base() {
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
    //        return isValid && character.faction != null && character.faction.leader != null && character.faction.leader is Character && character.faction.successionComponent.IsSuccessor(character);
    //    }
    //    return false;
    //}
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.faction == null || targetCharacter.faction.leader == null || !(targetCharacter.faction.leader is Character)) {
                return false;
            }
            if (!targetCharacter.faction.successionComponent.IsSuccessor(targetCharacter)) {
                return false;
            }
            return true;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.faction == null || targetCharacter.faction.leader == null || !(targetCharacter.faction.leader is Character)) {
            reasons += "Target has no faction or has no faction leader.";
        }
        if (!targetCharacter.faction.successionComponent.IsSuccessor(targetCharacter)) {
            reasons += "Target is not a successor.";
        }
        return reasons;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        Character previousLeader = character.faction.leader as Character;
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, character);

        //Overthrown leaders should also not become settlement rulers
        //Reason: It's weird that if a character is overthrown as faction leader they will stay as settlement rulers
        if(previousLeader != null && previousLeader.isSettlementRuler) {
            previousLeader.homeSettlement.SetRuler(null);
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        if(character.faction != null && character.faction.leader != null) {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData($"You should usurp {character.faction.leader.name}'s authority.", null, DialogItem.Position.Right);
            conversationList.Add(data);
        }
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, ref float p_newSuccessRate) {
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            p_newSuccessRate *= 2f;
        }
        if (p_targetCharacter.traitContainer.HasTrait("Ambitious")) {
            p_newSuccessRate *= 2f;
        }
        base.ProcessSuccessRateWithMultipliers(p_targetCharacter, ref p_newSuccessRate);
    }
    public override string GetSuccessRateMultiplierText(Character p_targetCharacter) {
        string text = string.Empty;
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Treacherous: <color=white>x2</color>";
        }
        if (p_targetCharacter.traitContainer.HasTrait("Ambitious")) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Ambitious: <color=white>x2</color>";
        }
        if (text != string.Empty) {
            return text;
        }
        return base.GetSuccessRateMultiplierText(p_targetCharacter);
    }
    #endregion
}
