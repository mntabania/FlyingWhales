using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class PlayerAction : SpellData, IContextMenuItem {

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
            return character.marker != null;
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
        if(target is IPointOfInterest targetPOI) {
            ActivateAbility(targetPOI);
        } else if (target is HexTile targetHex) {
            ActivateAbility(targetHex);
        } else if (target is LocationStructure targetStructure) {
            ActivateAbility(targetStructure);
        } else if (target is StructureRoom room) {
            ActivateAbility(room);
        }
        Messenger.Broadcast(SpellSignals.PLAYER_ACTION_ACTIVATED, this);
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
        }
        return CanPerformAbility();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if(!canBeCastOnBlessed && targetCharacter.traitContainer.IsBlessed()) {
            return false;
        }
        return CanPerformAbility();
    }

    #region IContextMenuItem Implementation
    public void OnPickAction() {
        if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null) {
            Activate(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
        }
    }
    public bool CanBePickedRegardlessOfCooldown() {
        if (!isInCooldown) {
            if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null && !CanPerformAbilityTo(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget)) {
                return false;
            }    
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
    #endregion
}