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
#if DEBUG_LOG
        log += $"\n-{character.name} will attack village";
#endif
        if (character.gridTileLocation.area.settlementOnArea == character.behaviourComponent.attackVillageTarget || character.gridTileLocation.area == character.behaviourComponent.attackAreaTarget) {
#if DEBUG_LOG
            log += "\n-Already in the target npcSettlement, will try to combat residents";
#endif
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
#if DEBUG_LOG
                    log += "\n-Will attack combatant resident: " + chosenCombatantTarget.name;
#endif
                    character.combatComponent.Fight(chosenCombatantTarget, CombatManager.Hostility);
                } else if (chosenNonCombatantTarget != null) {
#if DEBUG_LOG
                    log += "\n-Will attack non-combatant resident: " + chosenNonCombatantTarget.name;
#endif
                    character.combatComponent.Fight(chosenNonCombatantTarget, CombatManager.Hostility);
                } else {
#if DEBUG_LOG
                    log += "\n-No resident found in settlement, remove behaviour";
#endif
                    character.behaviourComponent.SetAttackVillageTarget(null);
                    character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
                    if (character.behaviourComponent.isAgitated) {
                        character.behaviourComponent.SetIsAgitated(false);
                        character.movementComponent.SetEnableDigging(false);
                    }
                }
            } else {
                if(character.behaviourComponent.attackAreaTarget != null) {
                    Character chosenTarget = character.behaviourComponent.attackAreaTarget.locationCharacterTracker.GetRandomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory(character.behaviourComponent.attackAreaTarget);
                    if(chosenTarget != null) {
#if DEBUG_LOG
                        log += "\n-Will attack resident: " + chosenTarget.name;
#endif
                        character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
                    } else {
#if DEBUG_LOG
                        log += "\n-No resident found in settlement, remove behaviour";
#endif
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
#if DEBUG_LOG
            log += "\n-Is not in the target npcSettlement";
            log += "\n-Roam there";
#endif
            Area targetArea = character.behaviourComponent.attackAreaTarget;
            if (character.behaviourComponent.attackVillageTarget != null) {
                targetArea = character.behaviourComponent.attackVillageTarget.areas[UnityEngine.Random.Range(0, character.behaviourComponent.attackVillageTarget.areas.Count)];
            }
            LocationGridTile targetTile = targetArea.gridTileComponent.gridTiles[UnityEngine.Random.Range(0, targetArea.gridTileComponent.gridTiles.Count)];
            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
        }
        return true;
    }
}
