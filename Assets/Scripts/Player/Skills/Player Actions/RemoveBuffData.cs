using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class RemoveBuffData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REMOVE_BUFF;
    public override string name => "Remove Buff";
    public override string description => "This Ability can be used to remove any positive Trait from a character." +
        "\nRemoving a buff from a hostile Villager will produce a Chaos Orb.";

    public RemoveBuffData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        base.ActivateAbility(targetPOI);
        Messenger.Broadcast(PlayerSkillSignals.REMOVE_BUFF_ACTIVATED, targetPOI);
    }

	public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            if (character.isDead) {
                return false;
            }
            if (!character.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF)) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems) {
        if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character targetCharacter) {
            p_contextMenuItems.Clear();
            for (int i = 0; i < targetCharacter.traitContainer.traits.Count; i++) {
                Trait trait = targetCharacter.traitContainer.traits[i];
                if(!trait.isHidden && trait.type == TRAIT_TYPE.BUFF) {
                    p_contextMenuItems.Add(trait);
                }
            }
            return p_contextMenuItems;    
        }
        return null;
    }
    #endregion
    
    #region Remove Trait
    public void ActivateRemoveBuff(string traitName, Character p_character) {
        if (RollSuccessChance(p_character)) {
            p_character.traitContainer.RemoveTrait(p_character, traitName);
            Activate(p_character);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", name, "activated", null, LOG_TAG.Player);
            log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, traitName, LOG_IDENTIFIER.STRING_1);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        } else {
            //Go into cooldown but do not activate ability
            OnExecutePlayerSkill();
            //PlayerUI.Instance.ShowGeneralConfirmation("Action Failed", p_character.name + " resisted the power of the Ruinarch!");
            p_character.reactionComponent.ResistRuinarchPower();
        }
    }

    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if(currentLevel >= 3) {
            if (targetCharacter is Character character) {
                if (character.traitContainer.HasTrait("Dark Blessing")) {
                    return false;
                }
            }
            return base.CanPerformAbility();
        } else {
            return base.CanPerformAbilityTowards(targetCharacter);
        }
    }

    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        if (currentLevel >= 3) {
            if (targetCharacter is Character character) {
                if (character.traitContainer.HasTrait("Dark Blessing")) {
                    return "You promised not to touch this one, remember?";
                } else {
                    return base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
                }
            }
        } else {
            return base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        }
        return string.Empty;
    }
    #endregion
}