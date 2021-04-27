using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class TriggerFlawData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TRIGGER_FLAW;
    public override string name => "Trigger Flaw";
    public override string description => "This Ability can be used to immediately activate an effect of a Villager's negative Trait. You may choose from the Villager's list of flaws that can be triggered." +
        "\nActivating Trigger Flaw produces a Chaos Orb. If the Villager successfully performs a task related to the Flaw, it will produce additional 2 Chaos Orbs.";

    private readonly List<string> _triggerFlawPool;
    private static readonly List<LogFiller> _triggerFlawLogFillers = new List<LogFiller>();
    
    public TriggerFlawData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
        _triggerFlawPool = new List<string>();
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        // if(targetPOI is Character character) {
        //     ShowTriggerFlawUI(character);
        // }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.limiterComponent.canPerform == false) {
                return false;
            }
            return targetCharacter.isDead == false;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            if (!character.isNormalCharacter || character.traitContainer.HasTrait("Cultist") || character.isConsideredRatman) {
                return false;
            }
            if (!character.traitContainer.HasTraitOf(TRAIT_TYPE.FLAW)) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter); 
        if (targetCharacter.limiterComponent.canPerform == false) {
            reasons += "Cannot be used while target is incapacitated,";
        }
        return reasons;
    }
    protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems) {
        if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character targetCharacter) {
            p_contextMenuItems.Clear();
            for (int i = 0; i < targetCharacter.traitContainer.traits.Count; i++) {
                Trait trait = targetCharacter.traitContainer.traits[i];
                if(trait.type == TRAIT_TYPE.FLAW) {
                    p_contextMenuItems.Add(trait);
                }
            }
            return p_contextMenuItems;    
        }
        return null;
    }
    #endregion
    
    #region Trigger Flaw
    // private void ShowTriggerFlawUI(Character p_character) {
    //     _triggerFlawPool.Clear();
    //     for (int i = 0; i < p_character.traitContainer.traits.Count; i++) {
    //         Trait trait = p_character.traitContainer.traits[i];
    //         if(trait.type == TRAIT_TYPE.FLAW) {
    //             _triggerFlawPool.Add(trait.name);
    //         }
    //     }
    //     UIManager.Instance.ShowClickableObjectPicker(_triggerFlawPool, (o) => ActivateTriggerFlawConfirmation(o, p_character), null, (s) => CanActivateTriggerFlaw(s, p_character),
    //         $"Select Flaw ({PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()})", 
    //         (s) => OnHoverEnterFlaw(s, p_character), OnHoverExitFlaw, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true, identifier: "Trigger Flaw");
    // }
    // private void ActivateTriggerFlawConfirmation(object o, Character p_character) {
    //     string traitName = (string) o;
    //     Trait trait = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
    //     string question = "Are you sure you want to trigger " + traitName + "?";
    //     string effect = $"<b>Effect</b>: {trait.GetTriggerFlawEffectDescription(p_character, "flaw_effect")}";
    //     string manaCost = $"{PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}";
    //
    //     UIManager.Instance.ShowTriggerFlawConfirmation(question, effect, manaCost, () => ActivateTriggerFlaw(trait, p_character), layer: 26, showCover: true, pauseAndResume: true);
    // }
    public static void ActivateTriggerFlaw(Trait trait, Character p_character) {
        UIManager.Instance.HideObjectPicker();
        string result = trait.TriggerFlaw(p_character);
        //When flaw is triggered, leave from party
        if (result == "flaw_effect") {
            if (p_character.partyComponent.hasParty) {
                p_character.partyComponent.currentParty.RemoveMemberThatJoinedQuest(p_character);
            }
            Messenger.Broadcast(PlayerSkillSignals.FLAW_TRIGGER_SUCCESS, p_character);
            PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).OnExecutePlayerSkill();
        } else {
            string log = "Failed to trigger flaw. Some requirements might be unmet.";
            if (LocalizationManager.Instance.HasLocalizedValue("Trigger Flaw", trait.name, result)) {
                _triggerFlawLogFillers.Clear();
                _triggerFlawLogFillers.Add(new LogFiller(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
                
                string reason = LocalizationManager.Instance.GetLocalizedValue("Trigger Flaw", trait.name, result);
                log = UtilityScripts.Utilities.StringReplacer(reason, _triggerFlawLogFillers);
            }
            PlayerUI.Instance.ShowGeneralConfirmation("Trigger Flaw Failed", log);
        }
        Messenger.Broadcast(PlayerSkillSignals.FLAW_TRIGGERED_BY_PLAYER, trait);
    }
    // private bool CanActivateTriggerFlaw(string traitName, Character p_character) {
    //     Trait trait = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
    //     if (trait != null) {
    //         return trait.CanFlawBeTriggered(p_character);
    //     }
    //     return false;
    // }
    #endregion
    
}