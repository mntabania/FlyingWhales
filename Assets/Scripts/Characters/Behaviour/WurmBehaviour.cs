
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class WurmBehaviour : BaseMonsterBehaviour {
	public WurmBehaviour() {
		priority = 8;
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a wurm";
        log += $"\n-Do nothing";
        return false;
        //if (character.gridTileLocation != null) {
        //    if (character.reactionComponent.isHidden) {
        //        log += $"\n-1% chance to move to another place in the current region";
        //        int roll = UnityEngine.Random.Range(0, 100);
        //        log += $"\n-Roll: " + roll;
        //        if (roll < 1) {
        //            HexTile chosenHex = null;
        //            if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
        //                chosenHex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetRandomAdjacentHextileWithinRegion(true);
        //                if(chosenHex == null) {
        //                    chosenHex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
        //                }
        //            } else {
        //                chosenHex = character.gridTileLocation.GetNearestHexTileWithinRegion();
        //            }
        //            if(chosenHex != null) {
        //                log += $"\n-Character will teleport to hex: " + chosenHex.name;
        //                LocationGridTile chosenTile = chosenHex.GetRandomTile();
        //                if(chosenTile != character.gridTileLocation) {
        //                    CharacterManager.Instance.Teleport(character, chosenTile);
        //                    Log historyLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Summon", "Wurm", "burrow");
        //                    historyLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //                    historyLog.AddLogToInvolvedObjects();
        //                }
        //            }
        //        }
        //    }
        //}
        //return false;
	}
	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
		p_log = $"{p_log}\n-Will try to transfer to a random unoccupied wilderness or cave spot in current settlement";
		if (GameUtilities.RollChance(15, ref p_log)) {
			LocationGridTile targetBurrowTile = GetBurrowTargetTile(p_character);
			if (targetBurrowTile != null && p_character.jobComponent.TriggerIdleBurrow(targetBurrowTile, out p_producedJob)) {
				p_log = $"{p_log}\n-Will burrow to {targetBurrowTile}";
				return true;
			}
		}
		p_log = $"{p_log}\n-Will stay idle for 8 hours";
		return p_character.jobComponent.PlanIdleLongStandStill(out p_producedJob);
	}

	private LocationGridTile GetBurrowTargetTile(Character p_character) {
		if (p_character.homeSettlement != null) {
			List<LocationGridTile> settlementTiles = ObjectPoolManager.Instance.CreateNewGridTileList();
			for (int i = 0; i < p_character.homeSettlement.tiles.Count; i++) {
				HexTile hexTile = p_character.homeSettlement.tiles[i];
				for (int j = 0; j < hexTile.locationGridTiles.Count; j++) {
					LocationGridTile tile = hexTile.locationGridTiles[j];
					if (tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS || tile.structure.structureType == STRUCTURE_TYPE.CAVE) {
						settlementTiles.Add(tile);
					}
				}
			}
			if (settlementTiles.Count > 0) {
				LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(settlementTiles);
				ObjectPoolManager.Instance.ReturnGridTileListToPool(settlementTiles);
				return chosenTile;
			} else {
				ObjectPoolManager.Instance.ReturnGridTileListToPool(settlementTiles);
			}
		} else if (p_character.homeStructure != null) {
			if (p_character.homeStructure.tiles.Count > 0) {
				return CollectionUtilities.GetRandomElement(p_character.homeStructure.tiles);
			}
		} else if (p_character.HasTerritory()) {
			List<LocationGridTile> territoryTiles = ObjectPoolManager.Instance.CreateNewGridTileList();
			for (int j = 0; j < p_character.territory.locationGridTiles.Count; j++) {
				LocationGridTile tile = p_character.territory.locationGridTiles[j];
				if (tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS || tile.structure.structureType == STRUCTURE_TYPE.CAVE) {
					territoryTiles.Add(tile);
				}
			}
			if (territoryTiles.Count > 0) {
				LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(territoryTiles);
				ObjectPoolManager.Instance.ReturnGridTileListToPool(territoryTiles);
				return chosenTile;
			} else {
				ObjectPoolManager.Instance.ReturnGridTileListToPool(territoryTiles);
			}
		}
		return null;
	}
}
