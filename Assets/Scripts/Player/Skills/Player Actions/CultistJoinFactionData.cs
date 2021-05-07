using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class CultistJoinFactionData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION;
    public override string name => "Join Faction";
    public override string description => "This Action instructs the character to join a target Faction. This is a special action available only on Cultists.";
    public override bool canBeCastOnBlessed => true;

    public CultistJoinFactionData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character sourceCharacter) {
            Faction sourceFaction = sourceCharacter.faction;
            List<Faction> choices = ObjectPoolManager.Instance.CreateNewFactionList();
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.factionType.type != FACTION_TYPE.Vagrants 
                    && faction.factionType.type != FACTION_TYPE.Demons 
                    && faction.factionType.type != FACTION_TYPE.Wild_Monsters
                    && faction.factionType.type != FACTION_TYPE.Undead
                    && faction.factionType.type != FACTION_TYPE.Ratmen
                    && faction.HasMemberThatIsNotDead()
                    && !faction.isDisbanded
                    && faction.isMajorNonPlayer) {
                    choices.Add(faction);
                }
            }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseFaction(o, sourceCharacter), validityChecker: t => CanJoinFaction(sourceCharacter, t), onHoverAction: t => OnHoverEnter(sourceCharacter, t), onHoverExitAction: OnHoverExit, showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 25);
            ObjectPoolManager.Instance.ReturnFactionListToPool(choices);
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            if (!character.traitContainer.HasTrait("Cultist")) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.traitContainer.HasTrait("Enslaved")) {
                return false;
            }
            return targetCharacter.isDead == false; //&& targetCharacter.homeSettlement != null;
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Enslaved")) {
            reasons += "Cannot target Slaves,";
        }
        return reasons;
    }
    #endregion

    private bool CanJoinFaction(Character source, Faction target) {
        if (!target.ideologyComponent.DoesCharacterFitCurrentIdeologies(source)) {
            return false;
        }
        if (target.IsCharacterBannedFromJoining(source)) {
            return false;
        }
        if (target == source.faction) {
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
        if (target == source.faction) {
            if (text != string.Empty) { text += "\n"; }
            text += UtilityScripts.Utilities.InvalidColorize($"This faction is the current faction of {source.name}.");
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
        base.ActivateAbility(source);
        if (obj is Faction targetFaction) {
            Character targetFactionMember = targetFaction.characters[0];
            source.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, targetFactionMember, "join_faction_normal");
        }
    }
}
