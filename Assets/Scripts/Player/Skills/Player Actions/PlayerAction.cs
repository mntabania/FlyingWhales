using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

public class PlayerAction : SkillData, IContextMenuItem {

    public virtual bool canBeCastOnBlessed => false;

    public virtual Sprite contextMenuIcon => PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(type)?.contextMenuIcon;
    public string contextMenuName => name;
    public virtual int contextMenuColumn  => PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(type)?.contextMenuColumn ?? 0;
    public List<IContextMenuItem> subMenus => GetSubMenus(_contextMenuItems);
    private List<IContextMenuItem> _contextMenuItems;
    
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

    public PlayerAction() {
        _contextMenuItems = new List<IContextMenuItem>();
    }
    
    #region Virtuals
    public virtual bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            return character.hasMarker;
        } else if (target is TileObject tileObject) {
            return tileObject.mapObjectVisual != null;
        }
        return true;
    }
    public string GetLabelName(IPlayerActionTarget target) {
        return name;
    }
    protected virtual List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems) {
        return null;
    }
    #endregion

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is TileObject tileObject) {
            IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        }
        base.ActivateAbility(targetPOI);
        Messenger.Broadcast(SpellSignals.PLAYER_ACTION_EXECUTED_TOWARDS_POI, this, targetPOI);
    }
    #endregion  

    public void Activate(IPlayerActionTarget target) {
        if (RollSuccessChance(target)) {
            if (target is IPointOfInterest targetPOI) {
                ActivateAbility(targetPOI);
            } else if (target is HexTile targetHex) {
                ActivateAbility(targetHex);
            } else if (target is LocationStructure targetStructure) {
                ActivateAbility(targetStructure);
            } else if (target is StructureRoom room) {
                ActivateAbility(room);
            } else if (target is BaseSettlement settlement) {
                ActivateAbility(settlement);
            }
            Messenger.Broadcast(SpellSignals.PLAYER_ACTION_ACTIVATED, this);
        } else {
            PlayerUI.Instance.ShowGeneralConfirmation("Action Failed", target.name + " resisted the power of the Ruinarch!");
        }
	}
    public bool CanPerformAbilityTo(IPlayerActionTarget target) {
        if (target is IPointOfInterest targetPOI) {
            return CanPerformAbilityTowards(targetPOI);
        } else if (target is HexTile targetHex) {
            return CanPerformAbilityTowards(targetHex);
        } else if (target is LocationStructure targetStructure) {
            return CanPerformAbilityTowards(targetStructure);
        } else if (target is StructureRoom room) {
            return CanPerformAbilityTowards(room);
        } else if (target is BaseSettlement settlement) {
            return CanPerformAbilityTowards(settlement);
        }
        return CanPerformAbility();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if(!canBeCastOnBlessed && targetCharacter.traitContainer.IsBlessed()) {
            return false;
        }
        return CanPerformAbility();
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (!canBeCastOnBlessed && targetCharacter.traitContainer.IsBlessed()) {
            reasons += $"Cannot target Blessed characters,";
        }
        return reasons;
    }

    //Calculate chance based on piercing and resistance if the player action would be a success if activated
    protected bool RollSuccessChance(IPlayerActionTarget p_target) {
        int baseChance = 100;
        if(p_target is Character targetCharacter) {
            PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(type);
            if(data.resistanceType != RESISTANCE.None) {
                float resistanceValue = targetCharacter.piercingAndResistancesComponent.GetResistanceValue(data.resistanceType);
                float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(type);
                CombatManager.ModifyValueByPiercingAndResistance(ref baseChance, piercing, resistanceValue);
            }
        }
        return GameUtilities.RollChance(baseChance);
    }

    #region IContextMenuItem Implementation
    public void OnPickAction() {
        if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null) {
            Activate(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
        }
    }
    public bool CanBePickedRegardlessOfCooldown() {
        if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null && !CanPerformAbilityTo(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget)) {
            return false;
        }    
        return true;
    }
    public bool IsInCooldown() {
        return isInCooldown;
    }
    public float GetCoverFillAmount() {
        if (isInCooldown) {
            return 1f - ((float)currentCooldownTick / cooldown);
        }
        return 1f;
    }
    public int GetCurrentRemainingCooldownTicks() {
        return cooldown - currentCooldownTick;
    }
    public int GetManaCost() {
        return manaCost;
    }
    #endregion
}