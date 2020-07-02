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
        if (!character.behaviourComponent.hasLayedAnEgg) {
            log += $"\n-10% chance to lay an egg if current tile has no object, and if there are less than 8 same characters in hex";
            if(character.gridTileLocation != null && character.gridTileLocation.objHere == null) {
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: {roll}";
                if(roll < 10) {
                    if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        HexTile hex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                        if (hex != null && hex.GetNumOfCharactersInsideHexWithSameRaceAndClass(character.race, character.characterClass.className) < 8) {
                            character.jobComponent.TriggerLayEgg(out producedJob);
                            return true;
                        }
                    }
                }
            }
        }
        if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            HexTile hex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
            if(hex != null) {
                if (!character.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour))) {
                    if (character.IsInTerritory() || character.isAtHomeStructure) {
                        log += $"\n-7% chance to attack village if there are 5 or more same characters in hex";
                        int roll = UnityEngine.Random.Range(0, 100);
                        log += $"\n-Roll: {roll}";
                        if (roll < 7) {
                            if (hex.GetNumOfCharactersInsideHexWithSameRaceAndClass(character.race, character.characterClass.className) >= 5) {
                                List<HexTile> targets = character.behaviourComponent.GetVillageTargetsByPriority();
                                if (targets != null && targets.Count > 0) {
                                    HexTile targetHex = targets[0];
                                    if (targetHex.settlementOnTile != null && targetHex.settlementOnTile is NPCSettlement settlement) {
                                        character.behaviourComponent.SetAttackVillageTarget(settlement);
                                    } else {
                                        character.behaviourComponent.SetAttackHexTarget(targetHex);
                                    }
                                    character.behaviourComponent.AddBehaviourComponent(typeof(AttackVillageBehaviour));
                                    log += $"\n-Will attack";
                                    GameDate dueDate = GameManager.Instance.Today();
                                    dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
                                    SchedulingManager.Instance.AddEntry(dueDate, character.behaviourComponent.ClearAttackVillageData, character);
                                    return true;
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
                }
            }
        }
        log += $"\n-Will roam";
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
    }
}
