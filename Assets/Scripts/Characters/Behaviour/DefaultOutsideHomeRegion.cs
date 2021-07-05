using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;

public class DefaultOutsideHomeRegion : CharacterBehaviourComponent {
    public DefaultOutsideHomeRegion() {
        priority = 25;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (!character.isAtHomeRegion) {
#if DEBUG_LOG
            log += $"\n-{character.name} is not in home region";
#endif
            TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(null);
#if DEBUG_LOG
            log += $"\n  -Time of Day: {currentTimeOfDay}";
#endif
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += $"\n  -Morning/Lunch/Afternoon/Early Night: 35% to stroll";
                log += $"\n  -RNG roll: {chance}";
#endif
            if (chance < 35) {
#if DEBUG_LOG
                    log += $"\n  -Enter Stroll Outside State";
#endif
            return character.jobComponent.PlanIdleStrollOutside(out producedJob);
                } else {
#if DEBUG_LOG
                    log += $"\n  -Otherwise: Return home";
#endif
                    return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                }
            } else {
                if(character.currentStructure != null && character.currentStructure.structureType == STRUCTURE_TYPE.TAVERN) {
                    int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                    log += $"\n  -Already in a tavern, 35% to roam";
                    log += $"\n  -RNG roll: {chance}";
#endif
                    if (chance < 35) {
#if DEBUG_LOG
                        log += $"\n  -Roam";
#endif
                        return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    } else {
#if DEBUG_LOG
                        log += $"\n  -Otherwise: Drink";
#endif
                        Table table = character.currentStructure.GetRandomTileObjectOfTypeThatHasTileLocation<Table>();
                        if(table != null) {
                            return character.jobComponent.TriggerDrinkJob(JOB_TYPE.IDLE, table, out producedJob);
                        } else {
#if DEBUG_LOG
                            log += $"\n  -No table available in tavern, stroll";
#endif
                            return character.jobComponent.PlanIdleStrollOutside(out producedJob);
                        }
                    }

                } else {
                    List<LocationStructure> taverns = character.currentRegion.GetStructuresAtLocation(STRUCTURE_TYPE.TAVERN);
                    if (taverns != null && taverns.Count > 0) {
#if DEBUG_LOG
                        log += $"\n  -Has tavern in region";
#endif
                        LocationStructure chosenTavern = null;
                        for (int i = 0; i < taverns.Count; i++) {
                            LocationStructure potentialTavern = taverns[i];
                            if (potentialTavern.settlementLocation == null || potentialTavern.settlementLocation.owner == null || character.faction == null || !potentialTavern.settlementLocation.owner.IsHostileWith(character.faction)) {
                                chosenTavern = potentialTavern;
                                break;
                            }
                        }
                        if (chosenTavern != null) {
#if DEBUG_LOG
                            log += $"\n  -Chosen tavern: " + chosenTavern.name;
#endif
                            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(chosenTavern.passableTiles);
                            return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                        } else {
#if DEBUG_LOG
                            log += $"\n  -No tavern available for character, might be hostile with all the available taverns";
#endif
                        }
                    }
                }
                if (!character.currentStructure.isInterior) {
#if DEBUG_LOG
                    log += $"\n  -Character is in an exterior structure";
#endif
                    List<LocationStructure> structures = RuinarchListPool<LocationStructure>.Claim();
                    LocationStructure chosenStructure = null;
                    Area currentArea = character.gridTileLocation.area;
                    for (int i = 0; i < currentArea.neighbourComponent.neighbours.Count; i++) {
                        Area area = currentArea.neighbourComponent.neighbours[i];
                        LocationGridTile centerTile = area.gridTileComponent.centerGridTile;
                        //TODO: Enable digging
                        if (centerTile.structure.structureType.IsSpecialStructure() && centerTile.structure.isInterior && character.movementComponent.HasPathTo(centerTile)) {
                            structures.Add(centerTile.structure);
                        }
                    }
                    if (structures.Count > 0) {
                        chosenStructure = CollectionUtilities.GetRandomElement(structures);
                    }
                    RuinarchListPool<LocationStructure>.Release(structures);
                    if (chosenStructure != null) {
#if DEBUG_LOG
                        log += $"\n  -Has an adjacent special structure that has a path to, go there";
                        log += $"\n  -Chosen special structure: " + chosenStructure.name;
#endif
                        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(chosenStructure.passableTiles);
                        return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                    } else {
#if DEBUG_LOG
                        log += $"\n  -No adjacent special structure that has a path to";
#endif
                        if (character.currentSettlement != null) {
#if DEBUG_LOG
                            log += $"\n  -Character is inside settlement, go to adjacent plain hextile outside settlement";
#endif
                            Area chosenArea = character.currentSettlement.GetAPlainAdjacentArea();
                            if(chosenArea != null) {
                                LocationGridTile targetTile = chosenArea.gridTileComponent.GetRandomPassableTile();
                                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                            } else {
#if DEBUG_LOG
                                log += $"\n  -No adjacent plain hextile outside settlement, stroll";
#endif
                                return character.jobComponent.PlanIdleStrollOutside(out producedJob);
                            }
                        } else {
#if DEBUG_LOG
                            log += $"\n  -Outside settlement";
#endif
                            Campfire chosenCampfire = null;
                            List<TileObject> campfires = RuinarchListPool<TileObject>.Claim();
                            currentArea.tileObjectComponent.PopulateTileObjectsInArea<Campfire>(campfires);
                            if(campfires != null && campfires.Count > 0) {
                                for (int i = 0; i < campfires.Count; i++) {
                                    TileObject campfire = campfires[i];
                                    if(campfire.characterOwner == null || campfire.IsOwnedBy(character) || (!character.IsHostileWith(campfire.characterOwner) && !character.relationshipContainer.IsEnemiesWith(campfire.characterOwner))){
                                        chosenCampfire = campfire as Campfire;
                                        break;
                                    }
                                }
                            }
                            RuinarchListPool<TileObject>.Release(campfires);
                            if (chosenCampfire != null) {
                                int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                                log += $"\n  -Has available campfire within hex, warm up: " + chosenCampfire.nameWithID + ", 25% chance to roam around";
                                log += $"\n  -RNG roll: {chance}";
#endif
                                if (chance < 25) {
#if DEBUG_LOG
                                    log += $"\n  -Roam";
#endif
                                    return character.jobComponent.TriggerRoamAroundTile(out producedJob);
                                } else {
#if DEBUG_LOG
                                    log += $"\n  -Warm up";
#endif
                                    return character.jobComponent.TriggerWarmUp(chosenCampfire, out producedJob);
                                }
                            } else {
#if DEBUG_LOG
                                log += $"\n  -No available campfire within hex, create one";
#endif
                                return character.jobComponent.TriggerBuildCampfireJob(JOB_TYPE.IDLE, out producedJob);
                            }
                        }
                    }
                } else {
                    int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                    log += $"\n  -Character is in an interior structure: 35% to roam";
                    log += $"\n  -RNG roll: {chance}";
#endif
                    if (chance < 35) {
#if DEBUG_LOG
                        log += $"\n  -Roam";
#endif
                        return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    } else {
#if DEBUG_LOG
                        log += $"\n  -Otherwise: Stand";
#endif
                        return character.jobComponent.TriggerStand(out producedJob);
                    }
                }
            }
        }
        return false;
    }
}
