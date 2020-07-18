using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class WurmBehaviour : CharacterBehaviourComponent {
	public WurmBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a wurm";
        if (character.gridTileLocation != null) {
            if (character.reactionComponent.isHidden) {
                log += $"\n-4% chance to move to another place in the current region";
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: " + roll;
                if (roll < 4) {
                    HexTile chosenHex = null;
                    if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        chosenHex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetRandomAdjacentHextileWithinRegion(true);
                        if(chosenHex == null) {
                            chosenHex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                        }
                    } else {
                        chosenHex = character.gridTileLocation.collectionOwner.GetNearestHexTileWithinRegion();
                    }
                    if(chosenHex != null) {
                        log += $"\n-Character will teleport to hex: " + chosenHex.name;
                        LocationGridTile chosenTile = chosenHex.GetRandomTile();
                        if(chosenTile != character.gridTileLocation) {
                            CharacterManager.Instance.Teleport(character, chosenTile);
                            Log historyLog = new Log(GameManager.Instance.Today(), "Summon", "Wurm", "burrow");
                            historyLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            historyLog.AddLogToInvolvedObjects();
                        }
                    }
                }
            }
        }
        return false;
	}
}
