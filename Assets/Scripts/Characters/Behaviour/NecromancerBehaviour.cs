using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class NecromancerBehaviour : CharacterBehaviourComponent {
	public NecromancerBehaviour() {
		priority = 30;
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a necromancer";
        if (character.homeStructure != null && !character.homeStructure.hasBeenDestroyed && character.homeStructure.tiles.Count > 0 && character.homeStructure == character.necromancerTrait.lairStructure) {
            log += $"\n-Character has a home structure/territory";
            if (character.marker) {
                Character deadCharacter = null;
                Character deadSummon = null;
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    Character inVision = character.marker.inVisionCharacters[i];
                    if (inVision.isDead) {
                        if(!(inVision is Summon)) {
                            if (!inVision.hasRisen) {
                                deadCharacter = inVision;
                                break;
                            }
                        } else {
                            if (deadSummon == null) {
                                deadSummon = inVision;
                            }
                        }
                    }
                }
                if(deadCharacter != null) {
                    log += $"\n-Character saw a dead character, has a 80% chance to raise corpse";
                    if (UnityEngine.Random.Range(0, 100) < 80) {
                        log += $"\n-Character will raise corpse";
                        character.jobComponent.TriggerRaiseCorpse(deadCharacter, out producedJob);
                        return true;
                    }
                } else {
                    Tombstone tomb = null;
                    for (int i = 0; i < character.marker.inVisionTileObjects.Count; i++) {
                        if (character.marker.inVisionTileObjects[i] is Tombstone tombstone) {
                            Character dead = tombstone.character;
                            if (!(dead is Summon)) {
                                if (!dead.hasRisen) {
                                    tomb = tombstone;
                                    break;
                                }
                            }
                        }
                    }
                    if (tomb != null) {
                        log += $"\n-Character saw a tombstone, has a 80% chance to raise corpse";
                        if (UnityEngine.Random.Range(0, 100) < 80) {
                            log += $"\n-Character will raise corpse";
                            character.jobComponent.TriggerRaiseCorpse(tomb, out producedJob);
                            return true;
                        }
                    }
                }

                if(deadSummon != null) {
                    log += $"\n-Character saw a dead summon will try to absorb power";
                    if (deadSummon.characterClass.elementalType != ELEMENTAL_TYPE.Normal) {
                        if (!character.traitContainer.HasTrait(deadSummon.characterClass.elementalType.ToString() + " Attacker")) {
                            character.jobComponent.TriggerAbsorbPower(deadSummon, out producedJob);
                            return true;
                        }
                    }
                    character.jobComponent.TriggerAbsorbLife(deadSummon, out producedJob);
                    return true;
                }
            }

            TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
            if(currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT || currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                log += $"\n-It is Early Night, Late Night, or After Midnight";
                int skeletonFollowers = character.necromancerTrait.GetNumOfSkeletonFollowersThatAreNotAttackingAndIsAlive();
                if (skeletonFollowers > 5 && UnityEngine.Random.Range(0, 100) < 65) {
                    log += $"\n-Skeleton followers are more than 5, attack village";
                    //Attack
                    //character.faction.ClearAllDeadCharactersFromFaction();
                    NPCSettlement attackVillageTarget = LandmarkManager.Instance.GetFirstVillageSettlementInRegionWithAliveResident(character.currentRegion);
                    if(attackVillageTarget != null) {
                        character.necromancerTrait.SetAttackVillageTarget(attackVillageTarget);
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Order_Attack, character);
                    }
                } else {
                    log += $"\n-Not enough skeleton followers, will try to create more";
                    bool hasCreated = false;
                    if (character.necromancerTrait.energy > 0) {
                        hasCreated = character.jobComponent.TriggerSpawnSkeleton(out producedJob);
                    } else {
                        hasCreated = character.jobComponent.TriggerRegainEnergy(out producedJob);
                    }
                    if (!hasCreated) {
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    }
                    //if (character.necromancerTrait.lifeAbsorbed <= 0) {
                    //    log += $"\n-Life absorbed is none, will try to absorb life";
                    //    character.jobComponent.TriggerAbsorbLife();
                    //} else {
                    //    log += $"\n-There is life absorbed, 80% to create skeleton follower, 20% chance to absorb more life";
                    //    if (UnityEngine.Random.Range(0, 100) < 10) {
                    //        log += $"\n-Absorb life";
                    //        character.jobComponent.TriggerAbsorbLife();
                    //    } else {
                    //        log += $"\n-Spawn skeleton";
                    //        //Create Skeleton
                    //        character.jobComponent.TriggerSpawnSkeleton();
                    //    }
                    //}
                }
            } else {
                log += $"\n-It is not Early Night, Late Night, or After Midnight";
                if (character.currentStructure != character.homeStructure) {
                    log += $"\n-Character is not at home, return home";
                    character.PlanIdleReturnHome(out producedJob);
                } else {
                    for (int i = 0; i < character.faction.characters.Count; i++) {
                        if(character.faction.characters[i].race == RACE.SKELETON && !character.faction.characters[i].isDead && character.behaviourComponent.attackVillageTarget != null) {
                            log += $"\n-Character will recall his undead followers";
                            character.necromancerTrait.SetAttackVillageTarget(null);
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Recall_Attack, character);
                            break;
                        }
                    }
                    if (character.HasItem("Necronomicon")) {
                        if (UnityEngine.Random.Range(0, 100) < 30) {
                            log += $"\n-Character is at home, read necronomicon";
                            if (character.jobComponent.TriggerReadNecronomicon(out producedJob)) {
                                return true;
                            }
                        }
                    }
                    if (UnityEngine.Random.Range(0, 100) < 40) {
                        log += $"\n-Character is at home, meditate";
                        if (character.jobComponent.TriggerMeditate(out producedJob)) {
                            return true;
                        }
                    }
                    log += $"\n-Character is at home, roam";
                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                }
            }
        } else {
            log += $"\n-Character does not have a home structure/territory";
            log += $"\n-Character will set lair";
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Lair, character);
            if(character.necromancerTrait.lairStructure != null && character.homeStructure == character.necromancerTrait.lairStructure) {
                log += $"\n-Lair is set, character home structure is set as the lair";
                log += $"\n-Character will return home";
                character.PlanIdleReturnHome(out producedJob);
            } else {
                log += $"\n-Lair is not set, will spawn lair";

                HexTile chosenHex = null;
                if(character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                    HexTile targetHex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                    if(targetHex != null && targetHex.elevationType != ELEVATION.WATER && targetHex.elevationType != ELEVATION.MOUNTAIN && targetHex.landmarkOnTile == null && !targetHex.IsNextToOrPartOfVillage()) {
                        chosenHex = targetHex;
                    }
                }
                if (chosenHex == null) {
                    chosenHex = GetNoStructurePlainHexInRegion(character.currentRegion);
                }
                if (chosenHex == null) {
                    chosenHex = GetNoStructurePlainHexInAllRegions();
                }
                LocationGridTile centerTileOfHex = chosenHex.GetCenterLocationGridTile();
                character.jobComponent.TriggerSpawnLair(centerTileOfHex, out producedJob);
            }
        }
        return true;
	}

    private HexTile GetNoStructurePlainHexInAllRegions() {
        HexTile chosenHex = null;
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            chosenHex = GetNoStructurePlainHexInRegion(region);
            if (chosenHex != null) {
                return chosenHex;
            }
        }
        return chosenHex;
    }
    private HexTile GetNoStructurePlainHexInRegion(Region region) {
        return region.GetRandomNoStructureNotPartOrNextToVillagePlainHex();
    }
}
