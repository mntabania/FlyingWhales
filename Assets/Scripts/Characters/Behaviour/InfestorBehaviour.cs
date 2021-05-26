using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class InfestorBehaviour : CharacterBehaviourComponent {
    public InfestorBehaviour() {
        priority = 8;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} is an infestor";
#endif
        if (character.faction != null) {
            JobQueueItem jobToAssign = character.faction.GetFirstUnassignedJobToCharacterJob(character);
            if (jobToAssign != null) {
                producedJob = jobToAssign;
                return true;
            }
        }
        if (!character.behaviourComponent.hasLayedAnEgg) {
#if DEBUG_LOG
            log += $"\n-1% chance to lay an egg if current tile has no object and character is in home, and if there are less than 8 same characters in home";
#endif
            if (character.gridTileLocation != null && character.gridTileLocation.tileObjectComponent.objHere == null && (character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory())) {
                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += $"\n-Roll: {roll}";
#endif
                if (roll < 1) {
                    int currentCapacity = 0;
                    if(character.homeSettlement != null) {
                        currentCapacity = character.homeSettlement.GetNumOfResidentsThatHasRaceAndClassOf(character.race, character.characterClass.className);
                    } else {
                        Area area = character.areaLocation;
                        currentCapacity = area.locationCharacterTracker.GetNumOfCharactersInsideHexThatHasRaceAndClassOf(character.race, character.characterClass.className);
                    }
                    if(currentCapacity < 8) {
                        character.jobComponent.TriggerLayEgg(out producedJob);
                        return true;
                    }
                }
            }
        }
        if (character.IsInTerritory() || character.IsInHomeSettlement() || character.isAtHomeStructure) {
            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
            log += $"\n-7% chance to attack village if there are 5 or more same characters in hex";
            log += $"\n-Roll: {roll}";
#endif
            if (roll < 7) { //7
                int currentCapacity = 0;
                if (character.homeSettlement != null) {
                    currentCapacity = character.homeSettlement.GetNumOfResidentsThatHasRaceAndClassOf(character.race, character.characterClass.className, typeof(MonsterInvadeBehaviour));
                } else {
                    Area area = character.areaLocation;
                    currentCapacity = area.locationCharacterTracker.GetNumOfCharactersInsideHexThatHasRaceAndClassOf(character.race, character.characterClass.className, typeof(MonsterInvadeBehaviour));
                }
                if (currentCapacity >= 5) {
                    List<Area> targets = ObjectPoolManager.Instance.CreateNewAreaList();
                    character.behaviourComponent.PopulateVillageTargetsByPriority(targets);
                    if (targets != null && targets.Count > 0) {
                        Area targetArea = targets[0];
                        ObjectPoolManager.Instance.ReturnAreaListToPool(targets);
#if DEBUG_LOG
                        log += $"\n-Will attack";
#endif
                        if (targetArea.settlementOnArea != null && targetArea.settlementOnArea is NPCSettlement settlement) {
                            return character.jobComponent.TriggerMonsterInvadeJob(settlement.mainStorage, out producedJob);
                        } else {
                            return character.jobComponent.TriggerMonsterInvadeJob(targetArea, out producedJob);
                        }
                    }
                }
            }
        } else {
#if DEBUG_LOG
            log += $"\n-Will return to territory if he has one";
#endif
            if (character.homeStructure != null || character.HasTerritory()) {
#if DEBUG_LOG
                log += $"\n-Return to territory";
#endif
                character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                return true;
            }
        }
#if DEBUG_LOG
        log += $"\n-Will roam";
#endif
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
    }
}
