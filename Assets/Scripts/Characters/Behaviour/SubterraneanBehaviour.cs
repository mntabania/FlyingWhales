using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SubterraneanBehaviour : CharacterBehaviourComponent {
	public SubterraneanBehaviour() {
		priority = 10;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} is a subterranean";
#endif
        if (!character.isDead && character.gridTileLocation != null && !character.isNormalCharacter && !IsTamedMonster(character)) {
            if (character.behaviourComponent.subterraneanJustExitedCombat) {
#if DEBUG_LOG
                log += $"\n-Just exited combat will teleport to new location";
#endif
                List<LocationStructure> caves = character.currentRegion.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
                List<LocationStructure> allCavesInTheRegion = RuinarchListPool<LocationStructure>.Claim();
                allCavesInTheRegion.AddRange(caves);
                allCavesInTheRegion.Remove(character.currentStructure);
                LocationStructure chosenCave = null;
                if (allCavesInTheRegion.Count > 0) {
                    chosenCave = UtilityScripts.CollectionUtilities.GetRandomElement(allCavesInTheRegion);
                }
                RuinarchListPool<LocationStructure>.Release(allCavesInTheRegion);

                if (chosenCave != null) {
#if DEBUG_LOG
                    log += $"\n-Will teleport to " + chosenCave.name;
#endif
                    LocationGridTile chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(chosenCave.passableTiles);
                    if (chosenTile != null) {
                        LeaveWurmHoles(character.gridTileLocation);

                        CharacterManager.Instance.Teleport(character, chosenTile);
                        Log historyLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Subterranean", "burrow", providedTags: LOG_TAG.Combat);
                        historyLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        historyLog.AddToFillers(chosenCave, chosenCave.GetNameRelativeTo(character), LOG_IDENTIFIER.LANDMARK_1);
                        historyLog.AddLogToDatabase(true);
                        return true;
                    } else {
#if DEBUG_LOG
                        log += $"\n-No passable tile in cave, will stay";
#endif
                    }
                } else {
#if DEBUG_LOG
                    log += $"\n-No other caves found, will stay";
#endif
                }
            }
        }
        return false;
	}

    private void LeaveWurmHoles(LocationGridTile point1) {
        LocationGridTile point2 = null;
        Region chosenRegion = GridMap.Instance.mainRegion;
        for (int i = 0; i < 3; i++) {
            Area chosenArea = chosenRegion.GetRandomAreaThatIsNotWater();
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
