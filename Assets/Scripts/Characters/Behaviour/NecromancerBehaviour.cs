using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class NecromancerBehaviour : CharacterBehaviourComponent {
	public NecromancerBehaviour() {
		priority = 30;
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} is a necromancer";
#endif
        if (character.HasHome()) {
#if DEBUG_LOG
            log += $"\n-Character has a home structure/territory";
#endif
            if (character.marker) {
                Character deadCharacter = null;
                Character deadSummon = null;
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    Character inVision = character.marker.inVisionCharacters[i];
                    if (inVision.isDead) {
                        if(!(inVision is Summon)) {
                            if (!inVision.hasBeenRaisedFromDead) {
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
#if DEBUG_LOG
                    log += $"\n-Character saw a dead character, has a 80% chance to raise corpse";
#endif
                    if (UnityEngine.Random.Range(0, 100) < 80) {
                        int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                        if(followersInRegion > Necromancer.MaxSkeletonFollowers) {
#if DEBUG_LOG
                            log += $"\n-Character will no longer raise corpse because the number of followers in region is above {Necromancer.MaxSkeletonFollowers.ToString()}";
#endif
                        } else {
#if DEBUG_LOG
                            log += $"\n-Character will raise corpse";
#endif
                            character.jobComponent.TriggerRaiseCorpse(deadCharacter, out producedJob);
                            return true;
                        }
                    }
                } else {
                    Tombstone tomb = null;
                    for (int i = 0; i < character.marker.inVisionTileObjects.Count; i++) {
                        if (character.marker.inVisionTileObjects[i] is Tombstone tombstone) {
                            Character dead = tombstone.character;
                            if (!(dead is Summon)) {
                                if (!dead.hasBeenRaisedFromDead) {
                                    tomb = tombstone;
                                    break;
                                }
                            }
                        }
                    }
                    if (tomb != null) {
#if DEBUG_LOG
                        log += $"\n-Character saw a tombstone, has a 80% chance to raise corpse";
#endif
                        if (UnityEngine.Random.Range(0, 100) < 80) {
                            int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                            if (followersInRegion > Necromancer.MaxSkeletonFollowers) {
#if DEBUG_LOG
                                log += $"\n-Character will no longer raise corpse because the number of followers in region is above {Necromancer.MaxSkeletonFollowers.ToString()}";
#endif
                            } else {
#if DEBUG_LOG
                                log += $"\n-Character will raise corpse";
#endif
                                character.jobComponent.TriggerRaiseCorpse(tomb, out producedJob);
                                return true;
                            }
                        }
                    }
                }

                if(deadSummon != null) {
#if DEBUG_LOG
                    log += $"\n-Character saw a dead summon will try to absorb power";
#endif
                    //Removed absorbing element since characters can now equip weapons that can change elements also
                    //This will conflict the new system of changing elements
                    //ELEMENTAL_TYPE elementalType = deadSummon.characterClass.elementalType;
                    //if (elementalType != ELEMENTAL_TYPE.Normal) {
                    //    if (!character.traitContainer.HasTrait(elementalType.ToString() + " Attacker")) {
                    //        //NOTE: Rename to absorb element
                    //        if(character.jobComponent.TriggerAbsorbPower(deadSummon, out producedJob)) {
                    //            return true;
                    //        }
                    //    }
                    //}
                    if (!character.traitContainer.HasTrait("Enhanced Power")) {
                        if (character.jobComponent.TriggerAbsorbPower(deadSummon, out producedJob)) {
                            //Will absorb power that will enhance attack damage temporarily (12 hours)
                            return true;
                        }
                    }
                    character.jobComponent.TriggerAbsorbLife(deadSummon, out producedJob);
                    return true;
                }
            }

            TIME_IN_WORDS currentTime = GameManager.Instance.GetCurrentTimeInWordsOfTick();
            if(currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT || currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
#if DEBUG_LOG
                log += $"\n-It is Early Night, Late Night, or After Midnight";
#endif
                int skeletonFollowers = character.necromancerTrait.GetNumOfSkeletonFollowersThatAreNotAttackingAndIsAlive();
                if (skeletonFollowers > 5 && UnityEngine.Random.Range(0, 100) < 4) {
#if DEBUG_LOG
                    log += $"\n-Skeleton followers are more than 5, attack village";
#endif
                    //Attack
                    //character.faction.ClearAllDeadCharactersFromFaction();
                    NPCSettlement attackVillageTarget = LandmarkManager.Instance.GetFirstVillageSettlementInRegionWithAliveResident(character.currentRegion, character.faction);
                    if(attackVillageTarget != null) {
                        character.necromancerTrait.SetAttackVillageTarget(attackVillageTarget);
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Order_Attack, character);
                    }
                } else {
                    if(character.currentStructure != null) {
                        if(character.currentStructure.structureType == STRUCTURE_TYPE.ANCIENT_GRAVEYARD || character.currentStructure.structureType == STRUCTURE_TYPE.CEMETERY) {
                            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                            log += $"\n-30% chance to Roam";
                            log += $"\n-Roll: " + roll;
#endif
                            if (roll < 30) {
                                character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                                return true;
                            }
                        } else {
                            int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                            if (followersInRegion > Necromancer.MaxSkeletonFollowers) {
#if DEBUG_LOG
                                log += $"\n-Character will no longer visit Graveyards/Cemeteries because the number of followers in region is above {Necromancer.MaxSkeletonFollowers.ToString()}";
#endif
                            } else {
                                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                                log += $"\n-50% chance to visit Ancient Graveyard/Cemetery if there's any, Otherwise, Create skeletons";
                                log += $"\n-Roll: " + roll;
#endif
                                if (roll < 50) {
                                    roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                                    log += $"\n-90% chance to visit an Ancient Graveyard, Otherwise visit Cemetery";
                                    log += $"\n-Roll: " + roll;
#endif
                                    STRUCTURE_TYPE structureTypeToVisit = STRUCTURE_TYPE.CEMETERY;
                                    if (roll < 90) {
                                        structureTypeToVisit = STRUCTURE_TYPE.ANCIENT_GRAVEYARD;
                                    }
                                    LocationStructure chosenStructure = character.currentRegion.GetRandomStructureOfTypeThatHasTombstone(structureTypeToVisit);
                                    if (chosenStructure != null) {
#if DEBUG_LOG
                                        log += $"\n-Will visit " + chosenStructure.name;
#endif
                                        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(chosenStructure.passableTiles);
                                        character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                                        return true;
                                    } else {
#if DEBUG_LOG
                                        log += $"\n-No structure " + structureTypeToVisit.ToString() + ", will try to visit the other one";
#endif
                                        if (structureTypeToVisit == STRUCTURE_TYPE.CEMETERY) {
                                            structureTypeToVisit = STRUCTURE_TYPE.ANCIENT_GRAVEYARD;
                                        } else {
                                            structureTypeToVisit = STRUCTURE_TYPE.CEMETERY;
                                        }
                                        chosenStructure = character.currentRegion.GetRandomStructureOfTypeThatHasTombstone(structureTypeToVisit);
                                        if (chosenStructure != null) {
#if DEBUG_LOG
                                            log += $"\n-Will visit " + chosenStructure.name;
#endif
                                            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(chosenStructure.passableTiles);
                                            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
#if DEBUG_LOG
                    log += $"\n-Will try to create more skeletons";
#endif
                    bool hasCreated = false;
                    if (character.necromancerTrait.energy >= 5) {
                        //hasCreated = character.jobComponent.TriggerSpawnSkeleton(out producedJob);
                        int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                        if (followersInRegion > 15) {
#if DEBUG_LOG
                            log += $"\n-Character will no longer visit create skeletons because the number of followers in region is above 15";
#endif
                        } else {
                            if (character.necromancerTrait.lifeAbsorbed <= 0) {
#if DEBUG_LOG
                                log += $"\n-Life absorbed is none, will try to absorb life";
#endif
                                if (GameUtilities.RollChance(10)) {
                                    hasCreated = character.jobComponent.TriggerAbsorbLife(out producedJob);
                                } else {
#if DEBUG_LOG
                                    log += $"\n-Will roam";
#endif
                                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                                }
                            } else {
#if DEBUG_LOG
                                log += $"\n-There is life absorbed, 80% to create skeleton follower, 20% chance to absorb more life";
#endif
                                if (GameUtilities.RollChance(10)) {
#if DEBUG_LOG
                                    log += $"\n-Absorb life";
#endif
                                    hasCreated = character.jobComponent.TriggerAbsorbLife(out producedJob);
                                } else {
#if DEBUG_LOG
                                    log += $"\n-Spawn skeleton";
#endif
                                    //Create Skeleton
                                    hasCreated = character.jobComponent.TriggerSpawnSkeleton(out producedJob);
                                }
                            }
                        }
                    } else {
#if DEBUG_LOG
                        log += $"\n-70% chance to regain energy";
#endif
                        if (GameUtilities.RollChance(70, ref log)) {
                            hasCreated = character.jobComponent.TriggerRegainEnergy(out producedJob);
                        } else {
#if DEBUG_LOG
                            log += $"\n-Will roam instead of regain energy";
#endif
                            character.jobComponent.TriggerRoamAroundTile(out producedJob);
                        }
                    }
                    if (!hasCreated) {
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    }
                }
            } else {
#if DEBUG_LOG
                log += $"\n-It is not Early Night, Late Night, or After Midnight";
#endif
                if (!character.IsAtHome()) {
#if DEBUG_LOG
                    log += $"\n-Character is not at home, return home";
#endif
                    character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                } else {
                    for (int i = 0; i < character.faction.characters.Count; i++) {
                        if(character.faction.characters[i].race == RACE.SKELETON && !character.faction.characters[i].isDead && character.behaviourComponent.attackVillageTarget != null) {
#if DEBUG_LOG
                            log += $"\n-Character will recall his undead followers";
#endif
                            character.necromancerTrait.SetAttackVillageTarget(null);
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Recall_Attack, character);
                            break;
                        }
                    }
                    if (character.HasItem("Necronomicon")) {
                        if (UnityEngine.Random.Range(0, 100) < 30) {
#if DEBUG_LOG
                            log += $"\n-Character is at home, read necronomicon";
#endif
                            if (character.jobComponent.TriggerReadNecronomicon(out producedJob)) {
                                return true;
                            }
                        }
                    }
                    if (UnityEngine.Random.Range(0, 100) < 40) {
#if DEBUG_LOG
                        log += $"\n-Character is at home, meditate";
#endif
                        if (character.jobComponent.TriggerMeditate(out producedJob)) {
                            return true;
                        }
                    }
#if DEBUG_LOG
                    log += $"\n-Character is at home, roam";
#endif
                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                }
            }
        } else {
#if DEBUG_LOG
            log += $"\n-Character does not have a home structure/territory";
            log += $"\n-Character will set lair";
#endif
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Lair, character);
            if(character.necromancerTrait.lairStructure != null && character.homeStructure == character.necromancerTrait.lairStructure) {
#if DEBUG_LOG
                log += $"\n-Lair is set, character home structure is set as the lair";
                log += $"\n-Character will return home";
#endif
                character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
            } else {
                if (character.necromancerTrait.doNotSpawnLair) {
#if DEBUG_LOG
                    log += $"\n-Necromancer cannot spawn lair, will roam instead";
#endif
                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                } else {
#if DEBUG_LOG
                    log += $"\n-Necromancer can spawn lair";
#endif
                    if (character.necromancerTrait.lairStructure == null || character.necromancerTrait.lairStructure.hasBeenDestroyed) {
                        character.necromancerTrait.SetLairStructure(null);
#if DEBUG_LOG
                        log += $"\n-Lair is not set, will try to spawn lair";
#endif
                        Area chosenArea = character.gridTileLocation.GetNearestHexTileForNecromancerSpawnLair(character);
                        //Removed this because we only have 1 region only
                        //if (chosenArea == null) {
                        //    chosenArea = GetNoStructurePlainAreaInAllRegions();
                        //}
                        if (chosenArea != null) {
#if DEBUG_LOG
                            log += $"\n-Has chosen an area to spawn lair";
#endif
                            LocationGridTile centerTileOfHex = chosenArea.gridTileComponent.centerGridTile;
                            character.jobComponent.TriggerSpawnLair(centerTileOfHex, out producedJob);
                        } else {
#if DEBUG_LOG
                            log += $"\n-Cannot spawn lair, will find a special structure and set is as lair";
#endif
                            LocationStructure possibleLairStructure = GetStructureForNecromancerLair(character);
                            if (possibleLairStructure != null) {
#if DEBUG_LOG
                                log += $"\n-Found special structure for lair: " + possibleLairStructure.name;
#endif
                                character.necromancerTrait.SetLairStructure(possibleLairStructure);
                                character.MigrateHomeStructureTo(possibleLairStructure);
                            } else {
#if DEBUG_LOG
                                log += $"\n-Could not find special structure, will roam instead";
#endif
                                character.necromancerTrait.SetDoNotSpawnLair(true);
                                character.jobComponent.TriggerRoamAroundTile(out producedJob);
                            }
                        }
                    } else {
                        if (character.homeStructure != character.necromancerTrait.lairStructure) {
#if DEBUG_LOG
                            log += $"\n-Lair is not his home, will migrate home first";
#endif
                            character.MigrateHomeStructureTo(character.necromancerTrait.lairStructure);
                        }
#if DEBUG_LOG
                        log += $"\n-Character will return home";
#endif
                        character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                    }
                }
            }
        }
        return true;
	}

    private LocationStructure GetStructureForNecromancerLair(Character p_necromancer) {
        Region region = p_necromancer.currentRegion;
        if (region != null) {
            for (int i = 0; i < region.allSpecialStructures.Count; i++) {
                LocationStructure structure = region.allSpecialStructures[i];
                if (!structure.hasBeenDestroyed) {
                    if (!structure.IsOccupied() || IsStructureOccupiedByUndead(structure)) {
                        return structure;
                    }
                }
            }
        }
        return null;
    }
    private bool IsStructureOccupiedByUndead(LocationStructure p_structure) {
        for (int i = 0; i < p_structure.residents.Count; i++) {
            Character resident = p_structure.residents[i];
            if (resident.faction != null && resident.faction.factionType.type == FACTION_TYPE.Undead) {
                return true;
            }
        }
        return false;
    }
    //private Area GetNoStructurePlainAreaInAllRegions() {
    //    Area chosenArea = GetNoStructurePlainAreaInRegion(GridMap.Instance.mainRegion);
    //    //for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
    //    //    Region region = GridMap.Instance.allRegions[i];
    //    //    chosenArea = GetNoStructurePlainAreaInRegion(region);
    //    //    if (chosenArea != null) {
    //    //        return chosenArea;
    //    //    }
    //    //}
    //    return chosenArea;
    //}
    private Area GetNoStructurePlainAreaInRegion(Region region) {
        return region.GetRandomAreaThatIsUncorruptedFullyPlainNoStructureAndNotNextToOrPartOfVillage();
    }
}
