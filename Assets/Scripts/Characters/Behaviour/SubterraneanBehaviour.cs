using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SubterraneanBehaviour : CharacterBehaviourComponent {
	public SubterraneanBehaviour() {
		priority = 10;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a subterranean";
        if (!character.isDead && character.gridTileLocation != null && !character.isNormalCharacter && !IsTamedMonster(character)) {
            if (character.behaviourComponent.subterraneanJustExitedCombat) {
                character.behaviourComponent.SetSubterraneanJustExitedCombat(false);
                log += $"\n-Just exited combat will teleport to new location";
                List<LocationStructure> allCavesInTheRegion = character.currentRegion.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.CAVE);
                if(allCavesInTheRegion != null) {
                    LocationStructure chosenCave = null;
                    while(chosenCave == null && allCavesInTheRegion.Count > 0) {
                        LocationStructure potentialCave = UtilityScripts.CollectionUtilities.GetRandomElement(allCavesInTheRegion);
                        if(potentialCave == character.currentStructure) {
                            allCavesInTheRegion.Remove(potentialCave);
                        } else {
                            chosenCave = potentialCave;
                        }
                    }
                    if(chosenCave != null) {
                        log += $"\n-Will teleport to " + chosenCave.name;
                        LocationGridTile chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(chosenCave.passableTiles);
                        if(chosenTile != null) {
                            LeaveWurmHoles(character.gridTileLocation);

                            CharacterManager.Instance.Teleport(character, chosenTile);
                            Log historyLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Subterranean", "burrow", providedTags: LOG_TAG.Combat);
                            historyLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            historyLog.AddToFillers(chosenCave, chosenCave.GetNameRelativeTo(character), LOG_IDENTIFIER.LANDMARK_1);
                            historyLog.AddLogToDatabase(true);
                            return true;
                        } else {
                            log += $"\n-No passable tile in cave, will stay";
                        }
                    } else {
                        log += $"\n-No other caves found, will stay";
                    }
                }
            }
        }
        return false;
	}

    private void LeaveWurmHoles(LocationGridTile point1) {
        LocationGridTile point2 = null;
        Region chosenRegion = GridMap.Instance.GetRandomRegion();
        for (int i = 0; i < 3; i++) {
            Area chosenArea = chosenRegion.GetRandomHexThatMeetCriteria(h => h.elevationType != ELEVATION.WATER);
            LocationGridTile chosenTile = chosenArea.gridTileComponent.GetRandomPassableUnoccupiedNonWaterTile();
            point2 = chosenTile;
            if(point2 != null) {
                break;
            }
        }
        if(point1 != null && point2 != null) {
            InnerMapManager.Instance.CreateWurmHoles(point1, point2);
        }
    }
    private bool IsTamedMonster(Character p_character) {
        if (p_character is Summon summon) {
            return summon.isTamed;
        }
        return false;
    }
}
