using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using Locations.Settlements;

public class RebellionData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REBELLION;
    public override string name => "Rebellion";
    public override string description => "Convince a Settlement Ruler to split off their entire Village from current Faction.";
    public override string verbName => "Rebel";
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
        if (WorldSettings.Instance.worldSettingsData.factionSettings.disableNewFactions) {
            return false;
        }
        return base.IsValid(target);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
            if (villagerFactionCount >= FactionManager.MaxActiveVillagerFactions) {
                return false;
            }
            if (targetCharacter.faction != null && !targetCharacter.isFactionLeader && targetCharacter.isSettlementRuler) {
                if (targetCharacter.faction.HasOwnedSettlementThatHasAliveResidentAndIsNotHomeOf(targetCharacter)) {
                    return true;
                }
            }
            return false;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (!(!targetCharacter.isFactionLeader && targetCharacter.isSettlementRuler)) {
            reasons += "Target should be a settlement ruler but not a faction leader.";
        }
        if (targetCharacter.faction == null || !targetCharacter.faction.HasOwnedSettlementThatHasAliveResidentAndIsNotHomeOf(targetCharacter)) {
            reasons += "Target is has no faction or simply cannot rebel.";
        }
        int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
        if (villagerFactionCount >= FactionManager.MaxActiveVillagerFactions) {
            reasons += "Maximum number of active factions have been reached,";
        }
        return reasons;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        Faction previousFaction = character.faction;
        BaseSettlement homeSettlement = character.homeSettlement;
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character, "own_settlement");
        if(homeSettlement != null) {
            for (int i = 0; i < homeSettlement.residents.Count; i++) {
                Character resident = homeSettlement.residents[i];
                if(resident.faction != character.faction && character != resident) {
                    resident.ChangeFactionTo(character.faction, true);
                }
            }
        }
        if (previousFaction != character.faction && previousFaction.leader is Character previousLeader) {
            previousLeader.traitContainer.AddTrait(previousLeader, "Betrayed", character);
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData($"You must rebel against {character.faction.nameWithColor} and start your own Faction.", null, DialogItem.Position.Right);
        conversationList.Add(data);
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, ref float p_newSuccessRate) {
        p_newSuccessRate *= 0.5f;
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.leader != null && p_targetCharacter.faction.leader is Character factionLeader && p_targetCharacter != factionLeader) {
            if (p_targetCharacter.relationshipContainer.IsFriendsWith(factionLeader)) {
                p_newSuccessRate *= 0.2f;
            } else if (p_targetCharacter.relationshipContainer.IsEnemiesWith(factionLeader)) {
                p_newSuccessRate *= 3f;
            }
        }
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            p_newSuccessRate *= 2f;
        }
        base.ProcessSuccessRateWithMultipliers(p_targetCharacter, ref p_newSuccessRate);
    }
    public override string GetSuccessRateMultiplierText(Character p_targetCharacter) {
        string text = string.Empty;
        text += $"Rebellion: <color=white>x0.5</color>";
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.leader != null && p_targetCharacter.faction.leader is Character factionLeader && p_targetCharacter != factionLeader) {
            if (p_targetCharacter.relationshipContainer.IsFriendsWith(factionLeader)) {
                if(text != string.Empty) { text += "\n"; }
                text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is friends with the Faction Leader: <color=white>x0.2</color>";
            } else if (p_targetCharacter.relationshipContainer.IsEnemiesWith(factionLeader)) {
                if (text != string.Empty) { text += "\n"; }
                text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is enemies with the Faction Leader: <color=white>x3</color>";
            }
        }
        if (p_targetCharacter.traitContainer.HasTrait("Treacherous")) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Treacherous: <color=white>x2</color>";
        }
        if (text != string.Empty) {
            return text;
        }
        return base.GetSuccessRateMultiplierText(p_targetCharacter);
    }
    #endregion
}
