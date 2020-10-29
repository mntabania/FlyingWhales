using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class InfestorBehaviour : CharacterBehaviourComponent {
    public InfestorBehaviour() {
        priority = 8;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is an infestor";
        if (character.faction != null) {
            JobQueueItem jobToAssign = character.faction.GetFirstUnassignedJobToCharacterJob(character);
            if (jobToAssign != null) {
                producedJob = jobToAssign;
                return true;
            }
        }
        if (!character.behaviourComponent.hasLayedAnEgg) {
            log += $"\n-1% chance to lay an egg if current tile has no object and character is in home, and if there are less than 8 same characters in home";
            if(character.gridTileLocation != null && character.gridTileLocation.objHere == null && (character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory())) {
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: {roll}";
                if(roll < 2) {
                    int currentCapacity = 0;
                    if(character.homeSettlement != null) {
                        currentCapacity = character.homeSettlement.GetNumOfResidentsThatMeetCriteria(c => c.race == character.race && c.characterClass.className == character.characterClass.className);
                    } else if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        HexTile hex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                        currentCapacity = hex.GetNumOfCharactersInsideHexThatMeetCriteria(c => c.race == character.race && c.characterClass.className == character.characterClass.className);
                    }
                    if(currentCapacity < 8) {
                        character.jobComponent.TriggerLayEgg(out producedJob);
                        return true;
                    }
                }
            }
        }
        if (character.IsInTerritory() || character.IsInHomeSettlement() || character.isAtHomeStructure) {
            log += $"\n-7% chance to attack village if there are 5 or more same characters in hex";
            int roll = UnityEngine.Random.Range(0, 100);
            log += $"\n-Roll: {roll}";
            if (roll < 7) { //7
                int currentCapacity = 0;
                if (character.homeSettlement != null) {
                    currentCapacity = character.homeSettlement.GetNumOfResidentsThatMeetCriteria(c => c.race == character.race && c.characterClass.className == character.characterClass.className && !c.behaviourComponent.HasBehaviour(typeof(MonsterInvadeBehaviour)));
                } else if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                    HexTile hex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                    currentCapacity = hex.GetNumOfCharactersInsideHexThatMeetCriteria(c => c.race == character.race && c.characterClass.className == character.characterClass.className && !c.behaviourComponent.HasBehaviour(typeof(MonsterInvadeBehaviour)));
                }
                if (currentCapacity >= 5) {
                    List<HexTile> targets = character.behaviourComponent.GetVillageTargetsByPriority();
                    if (targets != null && targets.Count > 0) {
                        HexTile targetHex = targets[0];
                        log += $"\n-Will attack";
                        if (targetHex.settlementOnTile != null && targetHex.settlementOnTile is NPCSettlement settlement) {
                            return character.jobComponent.TriggerMonsterInvadeJob(settlement.mainStorage, out producedJob);
                        } else {
                            return character.jobComponent.TriggerMonsterInvadeJob(targetHex, out producedJob);
                        }
                    }
                }
            }
        } else {
            log += $"\n-Will return to territory if he has one";
            if (character.homeStructure != null || character.HasTerritory()) {
                log += $"\n-Return to territory";
                character.jobComponent.TriggerReturnTerritory(out producedJob);
                return true;
            }
        }
        log += $"\n-Will roam";
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
    }
}
