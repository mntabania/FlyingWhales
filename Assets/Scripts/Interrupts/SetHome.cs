using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Logs;
using UnityEngine.Assertions;
using Inner_Maps;
using UtilityScripts;

namespace Interrupts {
    public class SetHome : Interrupt {
        public SetHome() : base(INTERRUPT.Set_Home) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(interruptHolder.target != null) {
                //This means that the new home is predetermined
                if(interruptHolder.target is Character targetCharacter) {
                    interruptHolder.actor.MigrateHomeStructureTo(targetCharacter.homeStructure);
                } else if(interruptHolder.target is GenericTileObject genericTileObject) {
                    Assert.IsFalse(genericTileObject.gridTileLocation.structure is Wilderness, $"Set home interrupt of {interruptHolder.actor.name} will set home to wilderness! Provided tile object is {genericTileObject} at {genericTileObject.gridTileLocation}");
                    interruptHolder.actor.MigrateHomeStructureTo(genericTileObject.gridTileLocation.structure);
                }
            } else {
                SetNewHomeSettlement(interruptHolder.actor);
            }
            //if(actor.homeStructure != null && actor.homeStructure.settlementLocation != null) {
            //    if(actor is Summon) {
            //        actor.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
            //    } else {
            //        actor.SetIsWanderer(false);
            //    }
            //} else {
            //    if (actor is Summon) {
            //        actor.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
            //    } else {
            //        actor.SetIsWanderer(true);
            //    }
            //}
            return true;
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            Log log = default;
            if (actor.homeStructure != null && actor.homeStructure != actor.previousCharacterDataComponent.previousHomeStructure) {
                log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "set_new_home_structure", null, logTags);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, actor.homeStructure.name, LOG_IDENTIFIER.STRING_1);    
            } else if (actor.HasTerritory()) {
                //NO LOG YET IF NEW HOME IS TERRITORY

                //log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
                //log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //log.AddToFillers(null, actor.homeStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.STRING_1);
            }
            return log;
        }
        #endregion

        private void SetNewHomeSettlement(Character actor) {
            string log = "Setting new home for " + actor.name;
            Region currentRegion = actor.currentRegion;

            if (actor is Summon) {
                //Character is a monster
                log += "\n-Character is a monster";
                log += "\n-Find an unoccupied Special Structure within the region and randomly select one. Clear out Territory data if it has one.";
                LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatMeetCriteria(currStructure => !currStructure.IsOccupied() && currStructure.settlementLocation != null && currStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON && currStructure.passableTiles.Count > 0);
                if (chosenHomeStructure != null && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                    log += "\n-Special Structure found: " + chosenHomeStructure.ToString();
                    actor.logComponent.PrintLogIfActive(log);
                    return;
                }
                log += "\n-If none available and character does not have a Territory, 50% chance to set a random structure-less Area as its Territory.";
                if (!actor.HasTerritory()) {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        log += "\n-Getting structureless hex tile in current region: " + currentRegion.name;
                        Area area = currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
                        if (area != null) {
                            actor.SetTerritory(area);
                            log += "\n-Area found: " + area.locationName;
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
            } else {
                //Character is not a summon
                if (actor.isVagrantOrFactionless) {
                    log += "\n-Character is a vagrant";
                    if (actor.homeStructure == null || actor.homeStructure.hasBeenDestroyed) {
                        log += "\n-Character has no home structure";
                        //log += "\n-20% chance to join faction";
                        int roll = UnityEngine.Random.Range(0, 100);
                        //log += "\n-Roll: " + roll;
                        //if (roll < 20) {
                        //    Faction joinedFaction = actor.JoinFactionProcessing();
                        //    if (joinedFaction != null) {
                        //        log += $"\n-Chosen faction to join: {joinedFaction.name}";
                        //        SetNewHomeSettlementForNonVagrant(actor, ref log);
                        //        actor.logComponent.PrintLogIfActive(log);
                        //        return;
                        //    } else {
                        //        log += "\n-No available faction for character";
                        //    }
                        //}
                        log += "\n-40% chance: find an unoccupied but Habitable Special Structure within the region and randomly select one as its new Home Structure";
                        log += "\n-Roll: " + roll;
                        if (roll < 40) {
                            LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatMeetCriteria(s => !s.IsOccupied() && s.HasStructureTag(STRUCTURE_TAG.Shelter) && actor.previousCharacterDataComponent.previousHomeStructure != s);
                            if (chosenHomeStructure != null) {
                                log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
                                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }
                        }
                        roll = UnityEngine.Random.Range(0, 100);
                        log += "\n-20% chance: find an unoccupied Village or Village occupied only by Vagrants within the region and randomly select one of its Structures (prioritize Dwellings) as its new Home Structure.  Clear out Territory data if it has one.";
                        log += "\n-Roll: " + roll;
                        if (roll < 20) {
                            BaseSettlement chosenSettlement = currentRegion.GetFirstSettlementInRegion(x => x.locationType == LOCATION_TYPE.VILLAGE && actor.previousCharacterDataComponent.previousHomeSettlement != x && (x.residents.Count <= 0 || x.AreAllResidentsVagrantOrFactionless()));
                            if(chosenSettlement != null) {
                                log += "\n-Chosen Settlement: " + chosenSettlement.name;
                                LocationStructure chosenHomeStructure = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(chosenSettlement, actor);
                                if (chosenHomeStructure != null) {
                                    log += "\n-Chosen Home Structure: " + chosenHomeStructure.name;
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }
                            }
                        }
                        if (!actor.HasTerritory()) {
                            log += "\n-Character has no territory";
                            Area area = currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.gridTileComponent.HasCorruption());
                            if (area != null) {
                                actor.SetTerritory(area);
                                log += "\n-Territory found: " + area.locationName;
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }
                        } 
                        //else {
                        //    log += "\n-Character has territory, 50% chance to change territory to adjacent region";
                        //    roll = UnityEngine.Random.Range(0, 100);
                        //    log += "\n-Roll: " + roll;
                        //    if (roll < 50) {
                        //        Area territory = GetTerritoryInAdjacentRegions(currentRegion);
                        //        if (territory != null) {
                        //            actor.ClearTerritory();
                        //            actor.SetTerritory(territory);
                        //            log += "\n-Territory found: " + territory.tileName + " in region: " + territory.region.name;
                        //            actor.logComponent.PrintLogIfActive(log);
                        //            return;
                        //        }
                        //    }
                        //}
                    }
                } else {
                    SetNewHomeSettlementForNonVagrant(actor, ref log);
                }
            }
            //If all else fails, check if character has home structure and if it is already destroyed, set it to null
            if (actor.homeStructure != null && actor.homeStructure.hasBeenDestroyed) {
                actor.MigrateHomeStructureTo(null, affectSettlement: false);
            }
            actor.logComponent.PrintLogIfActive(log);
        }

        private void SetNewHomeSettlementForNonVagrant(Character actor, ref string log) {
            Region currentRegion = actor.currentRegion;
            log += "\nCharacter is not vagrant";
            if(actor.homeSettlement != null) {
                log += "\nCharacter is still part of a village";
                log += "\nFind unoccupied dwelling";
                LocationStructure chosenDwelling = actor.homeSettlement.GetFirstStructureThatMeetCriteria(s => !s.IsOccupied() && s is Dwelling && actor.previousCharacterDataComponent.previousHomeStructure != s);
                if(chosenDwelling != null && !IsSameAsCurrentHomeStructure(chosenDwelling, actor)) {
                    log += "\nFound dwelling: " + chosenDwelling.name;
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
                } else {
                    log += "\nFind dwelling that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friend";
                    chosenDwelling = GetDwellingWithCloseFriendOrNonRivalEnemyRelative(actor.homeSettlement, actor);
                    if (chosenDwelling != null && !IsSameAsCurrentHomeStructure(chosenDwelling, actor)) {
                        log += "\nFound dwelling: " + chosenDwelling.name;
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
                    } else {
                        log += "\n35% chance: if the character's Faction also owns other Villages or Special Structure";
                        int roll = UnityEngine.Random.Range(0, 100);
                        log += "\n-Roll: " + roll;
                        if (roll < 35) {
                            if (actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {
                                log += "\nFind an unoccupied House in one of those other Villages and set that as its Home Structure";
                                string identifier = string.Empty;
                                LocationStructure chosenHomeStructure = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(actor, ref identifier);
                                if (chosenHomeStructure != null && identifier == "unoccupied" && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                                    log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }

                                log += "\nFind a Habitable Special Structure or House that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friende";
                                if (chosenHomeStructure != null && identifier == "occupied" && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                                    log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }

                                if (chosenHomeStructure != null && identifier == "habitable" && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                                    log += "\nFound Habitable Structure: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }

                                log += "\nFind an unoccupied but Habitable Special Structure within the region";
                                chosenHomeStructure = currentRegion.GetRandomStructureThatMeetCriteria(s => !s.IsOccupied() && s.HasStructureTag(STRUCTURE_TAG.Shelter) && actor.previousCharacterDataComponent.previousHomeStructure != s);
                                if (chosenHomeStructure != null && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                                    log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }

                                //log += "\n-50% chance: set Territory to a random structure-less Area in one of the adjacent regions";
                                //roll = UnityEngine.Random.Range(0, 100);
                                //log += "\n-Roll: " + roll;
                                //if (roll < 50) {
                                //    HexTile territory = GetTerritoryInAdjacentRegions(currentRegion);
                                //    if (territory != null) {
                                //        actor.ClearTerritory();
                                //        actor.SetTerritory(territory);
                                //        log += "\n-Territory found: " + territory.tileName + " in region: " + territory.region.name;
                                //        actor.logComponent.PrintLogIfActive(log);
                                //        return;
                                //    }
                                //}

                                log += "\n-Set a random structure-less Area as its Territory and make character go there";
                                Area area = currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.gridTileComponent.HasCorruption());
                                if (area != null) {
                                    actor.ClearTerritory();
                                    actor.SetTerritory(area);
                                    log += "\n-Territory found: " + area.locationName;
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }
                            }
                        }
                    }
                }
            } else {
                log += "\nCharacter is not part of a village";
                log += "\n35% chance: if the character is a Faction Leader and they do not own any Villages";
                int roll = UnityEngine.Random.Range(0, 100);
                log += "\n-Roll: " + roll;
                if (roll < 35) {
                    if(actor.isFactionLeader && !actor.faction.HasOwnedSettlement() && actor.currentRegion != null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached()) {
                        log += $"\n-Find new village";
                        Area targetArea = actor.currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
                        if (targetArea != null) {
                            StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, actor.faction.factionType.mainResource, actor.faction.factionType.usesCorruptedStructures); //character.faction.factionType.mainResource
                            List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
                            GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                            actor.jobComponent.TriggerFindNewVillage(targetArea.gridTileComponent.centerGridTile, chosenStructurePrefab.name);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }

                log += "\n3% chance: Create a Found New Village Job if the current region's Village capacity is not yet full";
                log += "\n-Roll: " + roll;
                if (roll < 3) {
                    if (actor.isFactionLeader && !actor.faction.HasOwnedSettlement() && actor.currentRegion != null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached()) {
                        log += $"\n-Find new village";
                        Area targetArea = actor.currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
                        if (targetArea != null) {
                            StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, actor.faction.factionType.mainResource, actor.faction.factionType.usesCorruptedStructures); //character.faction.factionType.mainResource
                            List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
                            GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                            actor.jobComponent.TriggerFindNewVillage(targetArea.gridTileComponent.centerGridTile, chosenStructurePrefab.name);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }

                log += "\n80% chance: if the character's Faction also owns other Villages or Special Structure";
                roll = UnityEngine.Random.Range(0, 100);
                log += "\n-Roll: " + roll;
                LocationStructure chosenHomeStructure = null;
                if (roll < 80) {
                    if (actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {
                        log += "\nFind an unoccupied House in one of those other Villages and set that as its Home Structure";
                        string identifier = string.Empty;
                        chosenHomeStructure = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(actor, ref identifier);
                        if (chosenHomeStructure != null && identifier == "unoccupied" && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                            log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }

                        log += "\nFind a Habitable Special Structure or House that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friend";
                        if (chosenHomeStructure != null && identifier == "occupied" && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                            log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }

                        if (chosenHomeStructure != null && identifier == "habitable" && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                            log += "\nFound Habitable Structure: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }

                        log += "\nIf none available: Find the Village with least number of Villagers owned by the character's Faction and set its Town Center as its Home Structure. Make character go there.";
                        chosenHomeStructure = GetFirstStructureOfTypeFromOwnedSettlementsWithLeastVillagers(STRUCTURE_TYPE.CITY_CENTER, actor.faction, actor);
                        if (chosenHomeStructure != null && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                            log += "\nFound City Center: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }

                log += "\n-15% chance: find an unoccupied Village within the region and randomly select one of its Structures (prioritize Dwellings) as its new Home Structure.  Clear out Territory data if it has one.";
                roll = UnityEngine.Random.Range(0, 100);
                log += "\n-Roll: " + roll;
                if (roll < 15) {
                    BaseSettlement chosenSettlement = currentRegion.GetFirstSettlementInRegion(x => x.locationType == LOCATION_TYPE.VILLAGE && x.residents.Count <= 0 && x != actor.previousCharacterDataComponent.previousHomeSettlement);
                    if (chosenSettlement != null) {
                        log += "\n-Chosen Settlement: " + chosenSettlement.name;
                        chosenHomeStructure = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(chosenSettlement, actor);
                        if (chosenHomeStructure != null && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                            log += "\n-Chosen Home Structure: " + chosenHomeStructure.name;
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }

                log += "\n-15% chance: Find an unoccupied but Habitable Special Structure within the region";
                roll = UnityEngine.Random.Range(0, 100);
                log += "\n-Roll: " + roll;
                if (roll < 15) {
                    chosenHomeStructure = currentRegion.GetRandomStructureThatMeetCriteria(s => !s.IsOccupied() && s.HasStructureTag(STRUCTURE_TAG.Shelter) && actor.previousCharacterDataComponent.previousHomeStructure != s);
                    if (chosenHomeStructure != null && !IsSameAsCurrentHomeStructure(chosenHomeStructure, actor)) {
                        log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                }

                //log += "\n-50% chance: set Territory to a random structure-less Area in one of the adjacent regions";
                //roll = UnityEngine.Random.Range(0, 100);
                //log += "\n-Roll: " + roll;
                //if (roll < 50) {
                //    HexTile territory = GetTerritoryInAdjacentRegions(currentRegion);
                //    if (territory != null) {
                //        actor.ClearTerritory();
                //        actor.SetTerritory(territory);
                //        log += "\n-Territory found: " + territory.tileName + " in region: " + territory.region.name;
                //        actor.logComponent.PrintLogIfActive(log);
                //        return;
                //    }
                //}

                log += "\n-Set a random structure-less Area as its Territory and make character go there";
                Area area = currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.gridTileComponent.HasCorruption());
                if (area != null) {
                    actor.ClearTerritory();
                    actor.SetTerritory(area);
                    log += "\n-Territory found: " + area.locationName;
                    actor.logComponent.PrintLogIfActive(log);
                    return;
                }
            }
        }

        private LocationStructure FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(Character actor, ref string identifier) {
            LocationStructure chosenDwellingWithCloseFriendOrNonEnemyRivalRelative = null;
            LocationStructure chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative = null;
            LocationStructure chosenDwelling = null;
            for (int i = 0; i < actor.faction.ownedSettlements.Count; i++) {
                BaseSettlement baseSettlement = actor.faction.ownedSettlements[i];
                if (baseSettlement != actor.homeSettlement && baseSettlement != actor.previousCharacterDataComponent.previousHomeSettlement) {
                    if(baseSettlement.locationType == LOCATION_TYPE.VILLAGE) {
                        if (baseSettlement is NPCSettlement npcSettlement) {
                            chosenDwelling = npcSettlement.GetFirstStructureThatMeetCriteria(s => !s.IsOccupied() && s is Dwelling && actor.previousCharacterDataComponent.previousHomeStructure != s);
                            if (chosenDwelling != null) {
                                identifier = "unoccupied";
                                return chosenDwelling;
                            } else {
                                chosenDwellingWithCloseFriendOrNonEnemyRivalRelative = GetDwellingWithCloseFriendOrNonRivalEnemyRelative(npcSettlement, actor);
                            }
                        }
                    }
                    if (chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative == null) {
                        if (baseSettlement.locationType == LOCATION_TYPE.DUNGEON) {
                            for (int j = 0; j < baseSettlement.allStructures.Count; j++) {
                                LocationStructure structure = baseSettlement.allStructures[j];
                                if (structure != actor.previousCharacterDataComponent.previousHomeStructure && !structure.HasReachedMaxResidentCapacity()) {
                                    if (structure.HasCloseFriendOrNonEnemyRivalRelative(actor)) {
                                        chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative = structure;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (chosenDwellingWithCloseFriendOrNonEnemyRivalRelative != null) {
                identifier = "occupied";
                return chosenDwellingWithCloseFriendOrNonEnemyRivalRelative;
            } else if (chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative != null) {
                identifier = "habitable";
                return chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative;
            }
            return null;
        }
        //private Area GetTerritoryInAdjacentRegions(Region region) {
        //    List<Region> adjacentRegions = new List<Region>(region.neighbours);
        //    if(adjacentRegions != null) {
        //        while (adjacentRegions.Count > 0) {
        //            Region chosenAdjacentRegion = adjacentRegions[UnityEngine.Random.Range(0, adjacentRegions.Count)];
        //            HexTile hex = chosenAdjacentRegion.GetRandomHexThatMeetCriteria(currHex => currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN && currHex.landmarkOnTile == null && !currHex.isCorrupted);
        //    if (hex != null) {
        //                return hex;
        //            } else {
        //                adjacentRegions.Remove(chosenAdjacentRegion);
        //            }
        //        }
        //    }
        //    return null;
        //} 
        private NPCSettlement GetNewSettlementHomeFromFactionMember(Character actor) {
            NPCSettlement chosenSettlement = null;
            if (actor.faction.isMajorNonPlayer) {
                List<NPCSettlement> npcSettlements = null;
                for (int i = 0; i < actor.faction.characters.Count; i++) {
                    Character factionMember = actor.faction.characters[i];
                    if (factionMember.isSettlementRuler) {
                        if (npcSettlements == null) { npcSettlements = new List<NPCSettlement>(); }
                        npcSettlements.Add(factionMember.homeSettlement);
                    }
                }
                if (npcSettlements != null && npcSettlements.Count > 0) {
                    chosenSettlement = npcSettlements[UnityEngine.Random.Range(0, npcSettlements.Count)];
                }
            }
            return chosenSettlement;
        }
        private LocationStructure GetDwellingWithCloseFriendOrNonRivalEnemyRelative(NPCSettlement settlement, Character actor) {
            LocationStructure chosenDwelling = null;
            List<LocationStructure> dwellings = settlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if (dwellings != null) {
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure currDwelling = dwellings[i];
                    if (currDwelling != actor.previousCharacterDataComponent.previousHomeStructure && !currDwelling.HasReachedMaxResidentCapacity() && currDwelling.residents.Count > 0) {
                        Character resident = currDwelling.residents[0];
                        bool isCloseFriend = actor.relationshipContainer.IsFriendsWith(resident);
                        if (isCloseFriend) {
                            chosenDwelling = currDwelling;
                            break;
                        } else {
                            bool isNonRivalEnemyRelative = !actor.relationshipContainer.IsEnemiesWith(resident) && actor.relationshipContainer.IsFamilyMember(resident);
                            if (isNonRivalEnemyRelative) {
                                chosenDwelling = currDwelling;
                                break;
                            }
                        }
                    }
                }
            }
            return chosenDwelling;
        }
        private LocationStructure GetStructureInSettlementPrioritizeDwellingsExceptPrevious(BaseSettlement settlement, Character actor) {
            LocationStructure secondaryStructure = null;
            for (int i = 0; i < settlement.allStructures.Count; i++) {
                LocationStructure currStructure = settlement.allStructures[i];
                if (currStructure != actor.previousCharacterDataComponent.previousHomeStructure) {
                    if (currStructure is Dwelling) {
                        return currStructure;
                    } else {
                        if (secondaryStructure == null) {
                            secondaryStructure = currStructure;
                        }
                    }
                }
            }
            return secondaryStructure;
        }
        private LocationStructure GetFirstStructureOfTypeFromOwnedSettlementsWithLeastVillagers(STRUCTURE_TYPE structureType, Faction faction, Character actor) {
            BaseSettlement leastVillagersSettlement = null;
            LocationStructure structure = null;
            for (int i = 0; i < faction.ownedSettlements.Count; i++) {
                BaseSettlement settlement = faction.ownedSettlements[i];
                if (settlement != actor.previousCharacterDataComponent.previousHomeSettlement) {
                    LocationStructure structureOfType = settlement.GetFirstStructureThatMeetCriteria(s => s.structureType == structureType && s != actor.previousCharacterDataComponent.previousHomeStructure);
                    //if settlement has structure of type
                    if (structureOfType != null) {
                        if (leastVillagersSettlement == null || settlement.residents.Count < leastVillagersSettlement.residents.Count) {
                            leastVillagersSettlement = settlement;
                            structure = structureOfType;
                        }
                    }
                }
            }
            return structure;
        }
        private bool IsSameAsCurrentHomeStructure(LocationStructure p_structure, Character p_character) {
            return p_structure == p_character.homeStructure;
        }
    }
}