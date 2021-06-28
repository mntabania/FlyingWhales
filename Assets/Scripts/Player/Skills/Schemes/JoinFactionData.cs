using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class JoinFactionData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.JOIN_FACTION;
    public override string name => "Join Faction";
    public override string description => "This Ability instructs the character to join a target Faction if possible. Only available on Cultists.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public JoinFactionData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character sourceCharacter) {
            Faction sourceFaction = sourceCharacter.faction;
            List<Faction> choices = ObjectPoolManager.Instance.CreateNewFactionList();
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction != sourceFaction && faction.factionType.type != FACTION_TYPE.Vagrants && faction.factionType.type != FACTION_TYPE.Demons && faction.factionType.type != FACTION_TYPE.Wild_Monsters
                    && faction.factionType.type != FACTION_TYPE.Undead
                    && faction.HasMemberThatIsNotDead()
                    && !faction.isDisbanded) {
                    choices.Add(faction);
                }
            }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseFaction(o, sourceCharacter), validityChecker: t => CanJoinFaction(sourceCharacter, t), onHoverAction: t => OnHoverEnter(sourceCharacter, t), onHoverExitAction: OnHoverExit, showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 25);
            ObjectPoolManager.Instance.ReturnFactionListToPool(choices);
        }
    }
    //public override bool IsValid(IPlayerActionTarget target) {
    //    if (target is Character character) {
    //        bool isValid = base.IsValid(target);
    //        return isValid && character.isVagrant;
    //    }
    //    return false;
    //}
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (!targetCharacter.isVagrant) {
                return false;
            }
            return true;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (!targetCharacter.isVagrant) {
            reasons += "Target is not a vagrant.";
        }
        return reasons;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        if(target is Faction targetFaction) {
            Character targetFactionMember = targetFaction.characters[0];
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, targetFactionMember, "join_faction_normal");
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        if (target is Faction targetFaction) {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData($"Why don't you try and join {targetFaction.nameWithColor}?", null, DialogItem.Position.Right);
            conversationList.Add(data);
        }
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    #endregion

    private bool CanJoinFaction(Character source, Faction target) {
        if (!target.ideologyComponent.DoesCharacterFitCurrentIdeologies(source)) {
            return false;
        }
        if (target.IsCharacterBannedFromJoining(source)) {
            return false;
        }
        return true;
    }
    private void OnHoverEnter(Character source, Faction target) {
        string text = string.Empty;
        if (!target.ideologyComponent.DoesCharacterFitCurrentIdeologies(source)) {
            text += UtilityScripts.Utilities.InvalidColorize($"This faction will not accept {source.name}.");
        }
        if (target.IsCharacterBannedFromJoining(source)) {
            if(text != string.Empty) { text += "\n"; }
            text += UtilityScripts.Utilities.InvalidColorize($"This faction already banned {source.name} from joining.");
        }
        if (text != string.Empty) {
            UIManager.Instance.ShowSmallInfo(text);
        }
    }
    private void OnHoverExit(Faction source) {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnChooseFaction(object obj, Character source) {
        UIManager.Instance.HideObjectPicker();
        if (obj is Faction targetFaction) {
            //Show Scheme UI
            UIManager.Instance.ShowSchemeUI(source, targetFaction, this);
        }
    }
}
