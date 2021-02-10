using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;

public class AttackVillageBehaviour : CharacterBehaviourComponent {
    public AttackVillageBehaviour() {
        priority = 30;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} will attack village";
        if (character.gridTileLocation.parentArea.settlementOnTile == character.behaviourComponent.attackVillageTarget || character.gridTileLocation.parentArea == character.behaviourComponent.attackHexTarget) {
            log += "\n-Already in the target npcSettlement, will try to combat residents";
            //It will only go here if the invader is not combat anymore, meaning there are no more hostiles in his vision, so we must make sure that he attacks a resident in the settlement even though he can't see it
            BaseSettlement settlement = character.behaviourComponent.attackVillageTarget;
            if(settlement != null) {
                Character chosenNonCombatantTarget = null;
                Character chosenCombatantTarget = null;
                for (int i = 0; i < settlement.residents.Count; i++) {
                    Character resident = settlement.residents[i];
                    if (character != resident && !resident.isDead && resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(settlement)) {
                        if (resident.traitContainer.HasTrait("Combatant")) {
                            chosenCombatantTarget = resident;
                            break;
                        } else {
                            if (chosenNonCombatantTarget == null) {
                                chosenNonCombatantTarget = resident;
                            }
                        }
                    }
                }
                if (chosenCombatantTarget != null) {
                    log += "\n-Will attack combatant resident: " + chosenCombatantTarget.name;
                    character.combatComponent.Fight(chosenCombatantTarget, CombatManager.Hostility);
                } else if (chosenNonCombatantTarget != null) {
                    log += "\n-Will attack non-combatant resident: " + chosenNonCombatantTarget.name;
                    character.combatComponent.Fight(chosenNonCombatantTarget, CombatManager.Hostility);
                } else {
                    log += "\n-No resident found in settlement, remove behaviour";
                    character.behaviourComponent.SetAttackVillageTarget(null);
                    character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
                    if (character.behaviourComponent.isAgitated) {
                        character.behaviourComponent.SetIsAgitated(false);
                        character.movementComponent.SetEnableDigging(false);
                    }
                }
            } else {
                if(character.behaviourComponent.attackHexTarget != null) {
                    Character chosenTarget = character.behaviourComponent.attackHexTarget.GetRandomCharacterInsideHexThatMeetCriteria<Character>(c => !c.isDead && c.IsTerritory(character.behaviourComponent.attackHexTarget));
                    if(chosenTarget != null) {
                        log += "\n-Will attack resident: " + chosenTarget.name;
                        character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
                    } else {
                        log += "\n-No resident found in settlement, remove behaviour";
                        character.behaviourComponent.SetAttackVillageTarget(null);
                        character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
                        if (character.behaviourComponent.isAgitated) {
                            character.behaviourComponent.SetIsAgitated(false);
                            character.movementComponent.SetEnableDigging(false);
                        }
                    }
                } else {
                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                }
            }
        } else {
            log += "\n-Is not in the target npcSettlement";
            log += "\n-Roam there";
            HexTile targetHex = character.behaviourComponent.attackHexTarget;
            if (character.behaviourComponent.attackVillageTarget != null) {
                targetHex = character.behaviourComponent.attackVillageTarget.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.attackVillageTarget.tiles.Count)];
            }
            LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Length)];
            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
        }
        return true;
    }
}
