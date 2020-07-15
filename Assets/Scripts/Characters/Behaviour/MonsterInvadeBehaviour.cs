using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MonsterInvadeBehaviour : CharacterBehaviourComponent {
    public MonsterInvadeBehaviour() {
        priority = 900;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-Character is monster invading";
        Party monsterInvadeParty = character.partyComponent.currentParty;
        if (!monsterInvadeParty.isWaitTimeOver) {
            log += $"\n-Party is waiting, Roam";
            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        } else {
            log += $"\n-Party is not waiting";
            if(monsterInvadeParty.target != null) {
                log += $"\n-Party has target structure";
                if (character.currentStructure.settlementLocation == monsterInvadeParty.target.targetSettlement) {
                    log += $"\n-Character is already in target settlement";
                    Character target = character.currentStructure.settlementLocation.GetRandomAliveResidentInsideSettlement();
                    if (target != null) {
                        log += $"\n-Chosen target is {target.name}";
                        character.combatComponent.Fight(target, CombatManager.Hostility);
                    } else {
                        log += $"\n-Roam around";
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                } else {
                    log += $"\n-Character is not in target structure, go to it";
                    if (monsterInvadeParty.target is LocationStructure targetStructure) {
                        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                        character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                    }
                }
            } else {
                log += $"\n-Party has no target structure";
                MonsterInvadeParty party = monsterInvadeParty as MonsterInvadeParty;
                if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == party.targetHex) {
                    log += "\n-Already in the target hex, will try to combat residents";
                    Character target = party.targetHex.GetRandomAliveResidentInsideHex<Character>();
                    if (target != null) {
                        log += $"\n-Chosen target is {target.name}";
                        character.combatComponent.Fight(target, CombatManager.Hostility);
                    } else {
                        log += $"\n-Roam around";
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                } else {
                    log += $"\n-Character is not in target hex, go to it";
                    HexTile targetHex = party.targetHex;
                    LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob, targetTile);
                }
            }
        }
        return true;
    }
}
