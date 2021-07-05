using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps;
using Ruinarch;
using UtilityScripts;
public class SpawnPartyData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_PARTY;
    public override string name => "Spawn Party";
    public override string description => $"Spawn an eye ward that will monitor all actions within its radius.";
    public override bool shouldShowOnContextMenu => false;

	public SpawnPartyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

	#region Overrides
	public override bool IsValid(IPlayerActionTarget target) {
        return false;
	}

	public override void Activate(IPlayerActionTarget target) {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(this);
    }

    public void Activate() {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(this);
    }

    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }

    public override void ActivateAbility(LocationGridTile p_targetTile) { 
        LocationGridTile currentTileObject = p_targetTile;
        Messenger.Broadcast(PartySignals.PARTY_TILE_CHOSEN_FOR_SPAWNING, currentTileObject);
        base.ActivateAbility(p_targetTile);
    }

    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            targetTile.PopulateTilesInRadius(tiles, 2);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile eachTile = tiles[i];
                if (eachTile.structure.structureType == STRUCTURE_TYPE.CAVE) {
                    o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Party", "General", "invalid_build_at_cave");
                    RuinarchListPool<LocationGridTile>.Release(tiles);
                    return false;
                } else if (eachTile.groundType == LocationGridTile.Ground_Type.Water) {
                    o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Party", "General", "invalid_build_at_water");
                    RuinarchListPool<LocationGridTile>.Release(tiles);
                    return false;
                }
            }
            tiles.Clear();
            targetTile.PopulateTilesInRadius(tiles, 2, includeTilesInDifferentStructure: true, includeCenterTile: true);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile eachTile = tiles[i];
                if (eachTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                    o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Party", "General", "invalid_build_at_structure");
                    RuinarchListPool<LocationGridTile>.Release(tiles);
                    return false;
                }
            }
            /*if (targetTile.structure) {
                o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Party", "General", "invalid_build_at_structure");
                return false;
            }*/
            RuinarchListPool<LocationGridTile>.Release(tiles);
            return true;
        }
        return canPerform;
    }
    public override void OnNoLongerCurrentActiveSpell() {
        base.OnNoLongerCurrentActiveSpell();
        InputManager.Instance.SetAllHotkeysEnabledState(true);
        PlayerUI.Instance.EnableTopMenuButtons();
        UIManager.Instance.SetSpeedTogglesState(true);
    }
    public override void OnSetAsCurrentActiveSpell() {
        base.OnSetAsCurrentActiveSpell();
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        PlayerUI.Instance.CloseAllTopMenus();
        PlayerUI.Instance.DisableTopMenuButtons();
        UIManager.Instance.SetSpeedTogglesState(false);
    }
    #endregion
}