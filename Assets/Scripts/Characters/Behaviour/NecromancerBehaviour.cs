﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class NecromancerBehaviour : CharacterBehaviourComponent {
	public NecromancerBehaviour() {
		priority = 30;
	}
	public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} is a necromancer";
        if (character.homeStructure != null && !character.homeStructure.hasBeenDestroyed && character.homeStructure.tiles.Count > 0 && character.homeStructure == character.necromancerTrait.lairStructure) {
            log += $"\n-Character has a home structure/territory";
            if (character.marker) {
                Character deadCharacter = null;
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    Character inVision = character.marker.inVisionCharacters[i];
                    if (inVision.isDead && !(inVision is Summon)) {
                        deadCharacter = character.marker.inVisionCharacters[i];
                        break;
                    }
                }
                if(deadCharacter != null) {
                    log += $"\n-Character saw a dead character, has a 80% chance to raise corpse";
                    if (UnityEngine.Random.Range(0, 100) < 80) {
                        log += $"\n-Character will raise corpse";
                        character.jobComponent.TriggerRaiseCorpse(deadCharacter);
                        return true;
                    }
                } else {
                    Tombstone tomb = null;
                    for (int i = 0; i < character.marker.inVisionTileObjects.Count; i++) {
                        if (character.marker.inVisionTileObjects[i] is Tombstone tombstone) {
                            Character dead = tombstone.character;
                            if (!(dead is Summon)) {
                                tomb = tombstone;
                                break;
                            }
                        }
                    }
                    if (tomb != null) {
                        log += $"\n-Character saw a tombstone, has a 80% chance to raise corpse";
                        if (UnityEngine.Random.Range(0, 100) < 80) {
                            log += $"\n-Character will raise corpse";
                            character.jobComponent.TriggerRaiseCorpse(tomb);
                            return true;
                        }
                    }
                }
            }

            TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
            if(currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT || currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                log += $"\n-It is Early Night, Late Night, or After Midnight";
                int skeletonFollowers = character.necromancerTrait.numOfSkeletonFollowers;
                if (skeletonFollowers > 5) {
                    log += $"\n-Skeleton followers are more than 5, attack village";
                    //Attack
                    NPCSettlement attackVillageTarget = LandmarkManager.Instance.GetFirstVillageSettlementInRegion(character.currentRegion);
                    if(attackVillageTarget != null) {
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Order_Attack, character);
                    }
                } else {
                    log += $"\n-Not enough skeleton followers, will try to create more";
                    if (character.necromancerTrait.lifeAbsorbed <= 0) {
                        log += $"\n-Life absorbed is none, will try to absorb life";
                        character.jobComponent.TriggerAbsorbLife();
                    } else {
                        log += $"\n-There is life absorbed, 80% to create skeleton follower, 20% chance to absorb more life";
                        if (UnityEngine.Random.Range(0, 100) < 20) {
                            log += $"\n-Absorb life";
                            character.jobComponent.TriggerAbsorbLife();
                        } else {
                            log += $"\n-Spawn skeleton";
                            //Create Skeleton
                            character.jobComponent.TriggerSpawnSkeleton();
                        }
                    }
                }
            } else {
                log += $"\n-It is not Early Night, Late Night, or After Midnight";
                if (character.currentStructure != character.homeStructure) {
                    log += $"\n-Character is not at home, return home";
                    character.PlanIdleReturnHome();
                } else {
                    log += $"\n-Character is at home, roam";
                    character.jobComponent.TriggerRoamAroundTile();
                }
            }
        } else {
            log += $"\n-Character does not have a home structure/territory";
            log += $"\n-Character will set lair";
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Lair, character);
            if(character.necromancerTrait.lairStructure != null && character.homeStructure == character.necromancerTrait.lairStructure) {
                log += $"\n-Lair is set, character home structure is set as the lair";
                log += $"\n-Character will return home";
                character.PlanIdleReturnHome();
            } else {
                log += $"\n-Lair is not set, will spawn lair";

                HexTile chosenHex = null;
                if(character.gridTileLocation.collectionOwner.partOfHextile != null) {
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
                character.jobComponent.TriggerSpawnLair(centerTileOfHex);
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
