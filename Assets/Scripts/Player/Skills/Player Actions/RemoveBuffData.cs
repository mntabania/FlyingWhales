using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class RemoveBuffData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REMOVE_BUFF;
    public override string name => "Remove Buff";
    public override string description => "This Action can be used to remove any one Buff from a character.";
    
    public RemoveBuffData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
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
        } else {
            //Go into cooldown but do not activate ability
            OnExecutePlayerSkill();
            PlayerUI.Instance.ShowGeneralConfirmation("Action Failed", p_character.name + " resisted the power of the Ruinarch!");
        }
    }
    #endregion

}