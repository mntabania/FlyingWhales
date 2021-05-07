﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SpawnEyeWardData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_EYE_WARD;
    public override string name => "Spawn Eye";
    public override string description => $"Spawn a demon eye that will monitor all actions within its radius.";
    public override bool shouldShowOnContextMenu => false;
    public SpawnEyeWardData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void Activate(IPlayerActionTarget target) {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(this);
    }
    public override void ActivateAbility(LocationGridTile p_targetTile) {
        Snooper beholder = UIManager.Instance.structureInfoUI.activeStructure as Snooper;
        if (beholder.eyeWards.Count >= beholder.GetCurrentMaxEyeCount()) {
            return;
		}
        TileObject currentTileObject = p_targetTile.tileObjectComponent.hiddenObjHere;
        if (currentTileObject != null) {
            p_targetTile.structure.RemovePOI(currentTileObject);
        }
        DemonEye ward = InnerMapManager.Instance.CreateNewTileObject<DemonEye>(TILE_OBJECT_TYPE.DEMON_EYE);
        ward.SetBeholderOwner(beholder);
        p_targetTile.structure.AddPOI(ward, p_targetTile);

        if (UIManager.Instance.structureInfoUI.isShowing) {
            beholder.AddEyeWard(ward);
        }

        base.ActivateAbility(p_targetTile);
        Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, this as PlayerAction);
    }

    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Snooper) {
            return true;
        }
        return false;
    }

    public override void ShowValidHighlight(LocationGridTile tile) {
        if (UIManager.Instance.structureInfoUI.activeStructure is Snooper beholder) {
            TileHighlighter.Instance.PositionHighlight(beholder.GetEyeWardRadius(), tile);
        }
    }
    public override bool CanPerformAbilityTowards(LocationStructure target) {
        Snooper beholder = UIManager.Instance.structureInfoUI.activeStructure as Snooper;
        if(beholder == null) {
            return false;
		}
        if (beholder.eyeWards.Count >= beholder.GetCurrentMaxEyeCount()) {
            return false;
        }
        return true;
    }

    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool dontUsePerForm = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        Snooper beholder = UIManager.Instance.structureInfoUI.activeStructure as Snooper;
        bool canPerform = true;
        if (canPerform) {
            if (beholder.eyeWards.Count >= beholder.GetCurrentMaxEyeCount()) {
                return false;
            }
        }
        if (canPerform == true) {
            if(targetTile.tileObjectComponent.hiddenObjHere != null || (targetTile.tileObjectComponent.objHere != null && targetTile.tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT) || !targetTile.IsPassable()) {
                o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Skills", "Spawn Eye Ward", "invalid_already_has_hidden_object");
                return false;
            }
            return true;
        }
        return canPerform;
    }
    public override void OnSetAsCurrentActiveSpell() {
        base.OnSetAsCurrentActiveSpell();
        PlayerManager.Instance.player.tileObjectComponent.ShowAllEyeWardHighlights();
        if (UIManager.Instance.structureInfoUI.isShowing) {
            ActionItem item = UIManager.Instance.structureInfoUI.GetActionItem(this);
            if (item != null) {
                item.SetHighlightState(true);
            }
        }
    }
    public override void OnNoLongerCurrentActiveSpell() {
        base.OnNoLongerCurrentActiveSpell();
        PlayerManager.Instance.player.tileObjectComponent.HideAllEyeWardHighlights();
        if (UIManager.Instance.structureInfoUI.isShowing) {
            ActionItem item = UIManager.Instance.structureInfoUI.GetActionItem(this);
            if (item != null) {
                item.SetHighlightState(false);
            }
        }
    }
    #endregion
}