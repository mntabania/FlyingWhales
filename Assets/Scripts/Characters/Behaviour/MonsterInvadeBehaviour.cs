using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class MonsterInvadeBehaviour : CharacterBehaviourComponent {
    public MonsterInvadeBehaviour() {
        priority = 900;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-Character is monster invading";
#endif
        Gathering monsterInvadeGathering = character.gatheringComponent.currentGathering;
        if (!monsterInvadeGathering.isWaitTimeOver) {
#if DEBUG_LOG
            log += $"\n-Party is waiting, Roam";
#endif
            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        } else {
#if DEBUG_LOG
            log += $"\n-Party is not waiting";
#endif
            if (monsterInvadeGathering.target != null) {
#if DEBUG_LOG
                log += $"\n-Party has target structure";
#endif
                BaseSettlement targetSettlement = monsterInvadeGathering.target.currentSettlement;
                if (character.gridTileLocation != null && targetSettlement != null) {
                    if (character.gridTileLocation.IsPartOfSettlement(targetSettlement)) {
#if DEBUG_LOG
                        log += $"\n-Character is already in target settlement";
#endif
                        Character target = targetSettlement.GetRandomResidentForInvasionTargetThatIsInsideSettlement(targetSettlement, character);
                        if (target != null) {
#if DEBUG_LOG
                            log += $"\n-Chosen target is {target.name}";
#endif
                            character.combatComponent.Fight(target, CombatManager.Hostility);
                        } else {
#if DEBUG_LOG
                            log += $"\n-Roam around";
#endif
                            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        }
                    } else {
#if DEBUG_LOG
                        log += $"\n-Character is not in target structure, go to it";
#endif
                        if (monsterInvadeGathering.target is LocationStructure targetStructure) {
                            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                        }
                    }
                } else {
#if DEBUG_LOG
                    log += $"\n-Character has no tile/target settlement, roam";
#endif
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
#if DEBUG_LOG
                log += $"\n-Party has no target structure";
#endif
                MonsterInvadeGathering gathering = monsterInvadeGathering as MonsterInvadeGathering;
                if (character.areaLocation == gathering.targetArea) {
#if DEBUG_LOG
                    log += "\n-Already in the target hex, will try to combat residents";
#endif
                    Character target = gathering.targetArea.locationCharacterTracker.GetRandomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory(gathering.targetArea);
                    if (target != null) {
#if DEBUG_LOG
                        log += $"\n-Chosen target is {target.name}";
#endif
                        character.combatComponent.Fight(target, CombatManager.Hostility);
                    } else {
#if DEBUG_LOG
                        log += $"\n-Roam around";
#endif
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                } else {
#if DEBUG_LOG
                    log += $"\n-Character is not in target hex, go to it";
#endif
                    Area targetArea = gathering.targetArea;
                    LocationGridTile targetTile = targetArea.gridTileComponent.gridTiles[UnityEngine.Random.Range(0, targetArea.gridTileComponent.gridTiles.Count)];
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob, targetTile);
                }
            }
        }
        if (producedJob != null) {
            producedJob.SetIsThisAGatheringJob(true);
        }
        return true;
    }
}
