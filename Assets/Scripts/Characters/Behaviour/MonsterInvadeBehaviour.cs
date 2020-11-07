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
        log += $"\n-Character is monster invading";
        Gathering monsterInvadeGathering = character.gatheringComponent.currentGathering;
        if (!monsterInvadeGathering.isWaitTimeOver) {
            log += $"\n-Party is waiting, Roam";
            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        } else {
            log += $"\n-Party is not waiting";
            if(monsterInvadeGathering.target != null) {
                log += $"\n-Party has target structure";
                BaseSettlement targetSettlement = monsterInvadeGathering.target.currentSettlement;
                if (character.gridTileLocation != null && targetSettlement != null) {
                    if (character.gridTileLocation.IsPartOfSettlement(targetSettlement)) {
                        log += $"\n-Character is already in target settlement";
                        Character target = targetSettlement.GetRandomAliveResidentInsideSettlement();
                        if (target != null) {
                            log += $"\n-Chosen target is {target.name}";
                            character.combatComponent.Fight(target, CombatManager.Hostility);
                        } else {
                            log += $"\n-Roam around";
                            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        }
                    } else {
                        log += $"\n-Character is not in target structure, go to it";
                        if (monsterInvadeGathering.target is LocationStructure targetStructure) {
                            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                        }
                    }
                } else {
                    log += $"\n-Character has no tile/target settlement, roam";
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
                log += $"\n-Party has no target structure";
                MonsterInvadeGathering gathering = monsterInvadeGathering as MonsterInvadeGathering;
                if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == gathering.targetHex) {
                    log += "\n-Already in the target hex, will try to combat residents";
                    Character target = gathering.targetHex.GetRandomCharacterInsideHexThatMeetCriteria<Character>(c => !c.isDead && c.IsTerritory(gathering.targetHex));
                    if (target != null) {
                        log += $"\n-Chosen target is {target.name}";
                        character.combatComponent.Fight(target, CombatManager.Hostility);
                    } else {
                        log += $"\n-Roam around";
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                } else {
                    log += $"\n-Character is not in target hex, go to it";
                    HexTile targetHex = gathering.targetHex;
                    LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
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
