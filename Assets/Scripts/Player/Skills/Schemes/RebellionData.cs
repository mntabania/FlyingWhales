using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class RebellionData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REBELLION;
    public override string name => "Rebellion";
    public override string description => "Rebellion";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public RebellionData() : base() {
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
            return character.faction != null && !character.isFactionLeader && character.isSettlementRuler && character.faction.HasOwnedSettlementThatMeetCriteria(s => s != character.homeSettlement && s.HasResidentThatMeetsCriteria(c => !c.isDead));
        }
        return base.IsValid(target);
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
        if(character.homeSettlement != null) {
            for (int i = 0; i < character.homeSettlement.residents.Count; i++) {
                Character resident = character.homeSettlement.residents[i];
                if(resident.faction != character.faction && character != resident) {
                    resident.ChangeFactionTo(character.faction, true);
                }
            }
            if(character.homeSettlement.owner != character.faction) {
                LandmarkManager.Instance.OwnSettlement(character.faction, character.homeSettlement);
            }
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData($"You must rebel against {character.faction.name} and start your own Faction.", null, DialogItem.Position.Right);
        conversationList.Add(data);
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    public override float GetSuccessRateMultiplier(Character p_targetCharacter) {
        float rate = 0f;
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.leader != null && p_targetCharacter.faction.leader is Character factionLeader && p_targetCharacter != factionLeader) {
            if (p_targetCharacter.relationshipContainer.IsFriendsWith(factionLeader)) {
                rate += 0.2f;
            } else if (p_targetCharacter.relationshipContainer.IsEnemiesWith(factionLeader)) {
                rate += 3f;
            }
        }
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            rate += 2f;
        }
        if(rate != 0f) {
            return rate; 
        }
        return base.GetSuccessRateMultiplier(p_targetCharacter);
    }
    public override string GetSuccessRateMultiplierText(Character p_targetCharacter) {
        string text = string.Empty;
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.leader != null && p_targetCharacter.faction.leader is Character factionLeader && p_targetCharacter != factionLeader) {
            if (p_targetCharacter.relationshipContainer.IsFriendsWith(factionLeader)) {
                if(text != string.Empty) { text += "\n"; }
                text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is friends with the Faction Leader";
            } else if (p_targetCharacter.relationshipContainer.IsEnemiesWith(factionLeader)) {
                if (text != string.Empty) { text += "\n"; }
                text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is enemies with the Faction Leader";
            }
        }
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Treacherous";
        }
        if (text != string.Empty) {
            return text;
        }
        return base.GetSuccessRateMultiplierText(p_targetCharacter);
    }
    #endregion
}
