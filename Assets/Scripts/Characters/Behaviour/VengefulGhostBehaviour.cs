using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class VengefulGhostBehaviour : CharacterBehaviourComponent {
	public VengefulGhostBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a vengeful ghost";
        if (character.gridTileLocation != null) {
            if (!character.HasTerritory()) {
                log += "\n-No territory, will set nearest hex tile as territory";
                HexTile hex = character.gridTileLocation.collectionOwner.GetNearestHexTileWithinRegion();
                character.AddTerritory(hex);
            }
            if (!character.behaviourComponent.isAttackingDemonicStructure) {
                log += $"\n-15% chance to attack a demonic structure";
                int rollAttackDemonicStructure = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: " + rollAttackDemonicStructure;
                if (rollAttackDemonicStructure < 15) {
                    LocationStructure targetStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructureInRegion(character.currentRegion);
                    if (targetStructure != null) {
                        log += $"\n-Chosen target structure: " + targetStructure.name;
                        character.behaviourComponent.SetIsAttackingDemonicStructure(true, targetStructure as DemonicStructure);
                        return true;
                    } else {
                        log += $"\n-No demonic structure found in current region";
                    }
                }
            }

            log += $"\n-20% chance to a character of the player faction";
            int rollAttackMinion = UnityEngine.Random.Range(0, 100);
            log += $"\n-Roll: " + rollAttackMinion;
            if (rollAttackMinion < 20) {
                Character targetCharacter = character.currentRegion.GetRandomCharacterWithPathAndFaction(character);
                if (targetCharacter != null) {
                    log += $"\n-Chosen target character: " + targetCharacter.name;
                    character.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
                    return true;
                } else {
                    log += $"\n-No character found in current region";
                }
            }

            if (character.HasTerritory() && !character.IsInTerritory()) {
                log += "\n-Return to territory";
                return character.jobComponent.PlanIdleReturnHome(out producedJob);
            } else {
                log += "\n-Already in territory, Roam";
                return character.jobComponent.TriggerRoamAroundTile(out producedJob);
            }
        }
        return false;
	}
}
