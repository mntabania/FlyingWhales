﻿using System.Collections;
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
        log += $"\n-{character.name} is a necromancer";
        if (character.HasHome()) {
            log += $"\n-Character has a home structure/territory";
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
                    log += $"\n-Character saw a dead character, has a 80% chance to raise corpse";
                    if (UnityEngine.Random.Range(0, 100) < 80) {
                        int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                        if(followersInRegion > Necromancer.MaxSkeletonFollowers) {
                            log += $"\n-Character will no longer raise corpse because the number of followers in region is above {Necromancer.MaxSkeletonFollowers.ToString()}";
                        } else {
                            log += $"\n-Character will raise corpse";
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
                        log += $"\n-Character saw a tombstone, has a 80% chance to raise corpse";
                        if (UnityEngine.Random.Range(0, 100) < 80) {
                            int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                            if (followersInRegion > Necromancer.MaxSkeletonFollowers) {
                                log += $"\n-Character will no longer raise corpse because the number of followers in region is above {Necromancer.MaxSkeletonFollowers.ToString()}";
                            } else {
                                log += $"\n-Character will raise corpse";
                                character.jobComponent.TriggerRaiseCorpse(tomb, out producedJob);
                                return true;
                            }
                        }
                    }
                }

                if(deadSummon != null) {
                    log += $"\n-Character saw a dead summon will try to absorb power";
                    ELEMENTAL_TYPE elementalType = deadSummon.characterClass.elementalType;
                    if (elementalType != ELEMENTAL_TYPE.Normal) {
                        if (!character.traitContainer.HasTrait(elementalType.ToString() + " Attacker")) {
                            //NOTE: Rename to absorb element
                            character.jobComponent.TriggerAbsorbPower(deadSummon, out producedJob);
                            return true;
                        }
                    }
                    character.jobComponent.TriggerAbsorbLife(deadSummon, out producedJob);
                    return true;
                }
            }

            TIME_IN_WORDS currentTime = GameManager.Instance.GetCurrentTimeInWordsOfTick();
            if(currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT || currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                log += $"\n-It is Early Night, Late Night, or After Midnight";
                int skeletonFollowers = character.necromancerTrait.GetNumOfSkeletonFollowersThatAreNotAttackingAndIsAlive();
                if (skeletonFollowers > 5 && UnityEngine.Random.Range(0, 100) < 4) {
                    log += $"\n-Skeleton followers are more than 5, attack village";
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
                            log += $"\n-30% chance to Roam";
                            int roll = UnityEngine.Random.Range(0, 100);
                            log += $"\n-Roll: " + roll;
                            if (roll < 30) {
                                character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                                return true;
                            }
                        } else {
                            int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                            if (followersInRegion > Necromancer.MaxSkeletonFollowers) {
                                log += $"\n-Character will no longer visit Graveyards/Cemeteries because the number of followers in region is above {Necromancer.MaxSkeletonFollowers.ToString()}";
                            } else {
                                log += $"\n-50% chance to visit Ancient Graveyard/Cemetery if there's any, Otherwise, Create skeletons";
                                int roll = UnityEngine.Random.Range(0, 100);
                                log += $"\n-Roll: " + roll;
                                if (roll < 50) {
                                    log += $"\n-90% chance to visit an Ancient Graveyard, Otherwise visit Cemetery";
                                    roll = UnityEngine.Random.Range(0, 100);
                                    log += $"\n-Roll: " + roll;
                                    STRUCTURE_TYPE structureTypeToVisit = STRUCTURE_TYPE.CEMETERY;
                                    if (roll < 90) {
                                        structureTypeToVisit = STRUCTURE_TYPE.ANCIENT_GRAVEYARD;
                                    }
                                    LocationStructure chosenStructure = character.currentRegion.GetRandomStructureOfTypeThatMeetCriteria(s => s.HasTileObjectOfType(TILE_OBJECT_TYPE.TOMBSTONE), structureTypeToVisit);
                                    if (chosenStructure != null) {
                                        log += $"\n-Will visit " + chosenStructure.name;
                                        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(chosenStructure.passableTiles);
                                        character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                                        return true;
                                    } else {
                                        log += $"\n-No structure " + structureTypeToVisit.ToString() + ", will try to visit the other one";
                                        if (structureTypeToVisit == STRUCTURE_TYPE.CEMETERY) {
                                            structureTypeToVisit = STRUCTURE_TYPE.ANCIENT_GRAVEYARD;
                                        } else {
                                            structureTypeToVisit = STRUCTURE_TYPE.CEMETERY;
                                        }
                                        chosenStructure = character.currentRegion.GetRandomStructureOfTypeThatMeetCriteria(s => s.HasTileObjectOfType(TILE_OBJECT_TYPE.TOMBSTONE), structureTypeToVisit);
                                        if (chosenStructure != null) {
                                            log += $"\n-Will visit " + chosenStructure.name;
                                            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(chosenStructure.passableTiles);
                                            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                   
                    log += $"\n-Will try to create more skeletons";
                    bool hasCreated = false;
                    if (character.necromancerTrait.energy > 0) {
                        //hasCreated = character.jobComponent.TriggerSpawnSkeleton(out producedJob);
                        int followersInRegion = character.necromancerTrait.numOfSkeletonFollowers;
                        if (followersInRegion > 15) {
                            log += $"\n-Character will no longer visit create skeletons because the number of followers in region is above 15";
                        } else {
                            if (character.necromancerTrait.lifeAbsorbed <= 0) {
                                log += $"\n-Life absorbed is none, will try to absorb life";
                                if (GameUtilities.RollChance(10)) {
                                    hasCreated = character.jobComponent.TriggerAbsorbLife(out producedJob);
                                } else {
                                    log += $"\n-Will roam";
                                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                                }
                            } else {
                                log += $"\n-There is life absorbed, 80% to create skeleton follower, 20% chance to absorb more life";
                                if (GameUtilities.RollChance(10)) {
                                    log += $"\n-Absorb life";
                                    hasCreated = character.jobComponent.TriggerAbsorbLife(out producedJob);
                                } else {
                                    log += $"\n-Spawn skeleton";
                                    //Create Skeleton
                                    hasCreated = character.jobComponent.TriggerSpawnSkeleton(out producedJob);
                                }
                            }
                        }
                    } else {
                        hasCreated = character.jobComponent.TriggerRegainEnergy(out producedJob);
                    }
                    if (!hasCreated) {
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    }
                }
            } else {
                log += $"\n-It is not Early Night, Late Night, or After Midnight";
                if (!character.IsAtHome()) {
                    log += $"\n-Character is not at home, return home";
                    character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
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
                character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
            } else {
                if (character.necromancerTrait.doNotSpawnLair) {
                    log += $"\n-Necromancer cannot spawn lair, will roam instead";
                    character.jobComponent.TriggerRoamAroundTile(out producedJob);
                } else {
                    log += $"\n-Necromancer can spawn lair";
                    if (character.necromancerTrait.lairStructure == null || character.necromancerTrait.lairStructure.hasBeenDestroyed) {
                        character.necromancerTrait.SetLairStructure(null);
                        log += $"\n-Lair is not set, will try to spawn lair";
                        Area chosenArea = character.gridTileLocation.GetNearestHexTileForNecromancerSpawnLair(character);
                        //Removed this because we only have 1 region only
                        //if (chosenArea == null) {
                        //    chosenArea = GetNoStructurePlainAreaInAllRegions();
                        //}
                        if (chosenArea != null) {
                            log += $"\n-Has chosen an area to spawn lair";
                            LocationGridTile centerTileOfHex = chosenArea.gridTileComponent.centerGridTile;
                            character.jobComponent.TriggerSpawnLair(centerTileOfHex, out producedJob);
                        } else {
                            log += $"\n-Cannot spawn lair, will find a special structure and set is as lair";
                            LocationStructure possibleLairStructure = GetStructureForNecromancerLair(character);
                            if (possibleLairStructure != null) {
                                log += $"\n-Found special structure for lair: " + possibleLairStructure.name;
                                character.necromancerTrait.SetLairStructure(possibleLairStructure);
                                character.MigrateHomeStructureTo(possibleLairStructure);
                            } else {
                                log += $"\n-Could not find special structure, will roam instead";
                                character.necromancerTrait.SetDoNotSpawnLair(true);
                                character.jobComponent.TriggerRoamAroundTile(out producedJob);
                            }
                        }
                    } else {
                        if (character.homeStructure != character.necromancerTrait.lairStructure) {
                            log += $"\n-Lair is not his home, will migrate home first";
                            character.MigrateHomeStructureTo(character.necromancerTrait.lairStructure);
                        }
                        log += $"\n-Character will return home";
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
            for (int i = 0; i < region.allStructures.Count; i++) {
                LocationStructure structure = region.allStructures[i];
                if (!structure.hasBeenDestroyed && structure.structureType.IsSpecialStructure()) {
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
    private Area GetNoStructurePlainAreaInAllRegions() {
        Area chosenArea = null;
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            chosenArea = GetNoStructurePlainAreaInRegion(region);
            if (chosenArea != null) {
                return chosenArea;
            }
        }
        return chosenArea;
    }
    private Area GetNoStructurePlainAreaInRegion(Region region) {
        return region.GetRandomHexThatMeetCriteria(a => a.elevationComponent.IsFully(ELEVATION.PLAIN) && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
    }
}
