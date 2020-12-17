using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class OverthrowLeaderData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.OVERTHROW_LEADER;
    public override string name => "Overthrow Leader";
    public override string description => "Overthrow Leader";
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
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            return character.faction != null && character.faction.leader != null && character.faction.leader is Character && character.faction.successionComponent.IsSuccessor(character);
        }
        return false;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, character);
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
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Treacherous: x2";
        }
        if (p_targetCharacter.traitContainer.HasTrait("Ambitious")) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Ambitious: x2";
        }
        if (text != string.Empty) {
            return text;
        }
        return base.GetSuccessRateMultiplierText(p_targetCharacter);
    }
    #endregion
}
