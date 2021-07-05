using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class InstigateWarData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INSTIGATE_WAR;
    public override string name => "Instigate War";
    public override string description => "Force a Faction Leader to declare war on another faction.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public InstigateWarData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character sourceCharacter) {
            Faction sourceFaction = sourceCharacter.faction;
            List<Faction> choices = ObjectPoolManager.Instance.CreateNewFactionList();
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if(faction != sourceFaction && faction.factionType.type != FACTION_TYPE.Vagrants && faction.factionType.type != FACTION_TYPE.Demons && faction.factionType.type != FACTION_TYPE.Wild_Monsters
                    && faction.HasMemberThatIsNotDead()) {
                    choices.Add(faction);
                }
            }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseFaction(o, sourceCharacter), validityChecker: t => CanInstigateWar(sourceFaction, t), onHoverAction: t => OnHoverEnter(sourceFaction, t), onHoverExitAction: OnHoverExit, showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 25);
            ObjectPoolManager.Instance.ReturnFactionListToPool(choices);
        }
    }
    //public override bool IsValid(IPlayerActionTarget target) {
    //    if (target is Character character) {
    //        bool isValid = base.IsValid(target);
    //        return isValid && character.isFactionLeader;
    //    }
    //    return false;
    //}
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (!targetCharacter.isFactionLeader) {
                return false;
            }
            return true;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (!targetCharacter.isFactionLeader) {
            reasons += "Target is not a faction leader.";
        }
        return reasons;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        if(target is Faction targetFaction) {
            Character targetFactionMember = targetFaction.characters[0];
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_War, targetFactionMember);
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        if (target is Faction targetFaction) {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData($"I need you to declare war on {targetFaction.nameWithColor}.", null, DialogItem.Position.Right);
            conversationList.Add(data);
        }
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, ref float p_newSuccessRate) {
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful)) {
            p_newSuccessRate *= 0.25f;
        } else if (p_targetCharacter.traitContainer.HasTrait("Diplomatic")) {
            p_newSuccessRate *= 0.5f;
        }
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
            p_newSuccessRate *= 3f;
        }
        base.ProcessSuccessRateWithMultipliers(p_targetCharacter, ref p_newSuccessRate);
    }
    public override string GetSuccessRateMultiplierText(Character p_targetCharacter) {
        string text = string.Empty;
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful)) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.faction.nameWithColor} is Peaceful: <color=white>x0.25</color>";
        } else if (p_targetCharacter.traitContainer.HasTrait("Diplomatic")) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Diplomatic: <color=white>x0.5</color>";
        }
        if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
            if (text != string.Empty) { text += "\n"; }
            text += $"{p_targetCharacter.faction.nameWithColor} is Warmonger: <color=white>x3</color>";
        }
        if (text != string.Empty) {
            return text;
        }
        return base.GetSuccessRateMultiplierText(p_targetCharacter);
    }
    #endregion

    private bool CanInstigateWar(Faction source, Faction target) {
        if (source.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, target)) {
            return false;
        }
        return true;
    }
    private void OnHoverEnter(Faction source, Faction target) {
        if (source.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, target)) {
            UIManager.Instance.ShowSmallInfo(UtilityScripts.Utilities.InvalidColorize("Already at war."));
        }
    }
    private void OnHoverExit(Faction source) {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnChooseFaction(object obj, Character source) {
        if (obj is Faction targetFaction) {
            UIManager.Instance.HideObjectPicker();

            //Show Scheme UI
            UIManager.Instance.ShowSchemeUI(source, targetFaction, this);
        }
    }
}
