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
		if (character is Summon summon) {
			log += $"\n-{summon.name} is a vengeful ghost";
			if (summon.gridTileLocation != null) {
                if (!summon.HasTerritory()) {
                    log += "\n-No territory, will set nearest hex tile as territory";
                    HexTile hex = summon.gridTileLocation.collectionOwner.GetNearestHexTileWithinRegion();
                    summon.ClearTerritory();
                    summon.AddTerritory(hex);
                }
                if (!summon.behaviourComponent.isAttackingDemonicStructure) {
                    log += $"\n-15% chance to attack a demonic structure";
                    int rollAttackDemonicStructure = UnityEngine.Random.Range(0, 100);
                    log += $"\n-Roll: " + rollAttackDemonicStructure;
                    if (rollAttackDemonicStructure < 15) {
                        LocationStructure targetStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructureInRegion(summon.currentRegion);
                        if(targetStructure != null) {
                            log += $"\n-Chosen target structure: " + targetStructure.name;
                            summon.behaviourComponent.SetIsAttackingDemonicStructure(true, targetStructure as DemonicStructure);
                            return true;
                        } else {
                            log += $"\n-No demonic structure found in current region";
                        }
                    }
                }

                log += $"\n-20% chance to a character of the player faction";
                int rollAttackMinion = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: " + rollAttackMinion;
                if(rollAttackMinion < 20) {
                    Character targetCharacter = summon.currentRegion.GetRandomCharacterWithPathAndFaction(summon);
                    if (targetCharacter != null) {
                        log += $"\n-Chosen target character: " + targetCharacter.name;
                        summon.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
                        return true;
                    } else {
                        log += $"\n-No character found in current region";
                    }
                }

                if(summon.HasTerritory() && !summon.IsInTerritory()) {
                    log += "\n-Return to territory";
                    return summon.jobComponent.PlanIdleReturnHome(out producedJob);
                } else {
                    log += "\n-Already in territory, Roam";
                    return summon.jobComponent.TriggerRoamAroundTile(out producedJob);
                }
            }
		}
		return false;
	}
}
