using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Logs;
using UnityEngine.Assertions;
using Inner_Maps;
using UtilityScripts;
using Object_Pools;

namespace Interrupts {
    public class SetHome : Interrupt {
        public SetHome() : base(INTERRUPT.Set_Home) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            LocationStructure currentHomeStructure = actor.homeStructure;
            if(interruptHolder.target != null) {
                //This means that the new home is predetermined
                if(interruptHolder.target is Character targetCharacter) {
                    actor.MigrateHomeStructureTo(targetCharacter.homeStructure);
                } else if(interruptHolder.target is GenericTileObject genericTileObject) {
                    Assert.IsFalse(genericTileObject.gridTileLocation.structure is Wilderness, $"Set home interrupt of {actor.name} will set home to wilderness! Provided tile object is {genericTileObject} at {genericTileObject.gridTileLocation}");
                    actor.MigrateHomeStructureTo(genericTileObject.gridTileLocation.structure);
                }
            } else {
                SetNewHomeSettlement(actor);
            }
            //Do not log if the new home is same as previous/current home so that it will not spam in the log tab
            //This is also the fix for this: https://trello.com/c/Ecjx7j55/3762-live-v03502-cultist-found-new-home-loop
            if (actor.homeStructure != null && actor.homeStructure != actor.previousCharacterDataComponent.previousHomeStructure && actor.homeStructure != currentHomeStructure) {
                if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "set_new_home_structure", null, logTags);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(actor.homeStructure, actor.homeStructure.name, LOG_IDENTIFIER.LANDMARK_1);
            }
            return true;
        }
        //public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
        //    Log log = default;
        //    //Do not log if the new home is same as previous home so that it will not spam in the log tab
        //    //This is also the fix for this: https://trello.com/c/Ecjx7j55/3762-live-v03502-cultist-found-new-home-loop
        //    if (actor.homeStructure != null && actor.homeStructure != actor.previousCharacterDataComponent.previousHomeStructure) {
        //        log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "set_new_home_structure", null, logTags);
        //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(null, actor.homeStructure.name, LOG_IDENTIFIER.STRING_1);    
        //    } else if (actor.HasTerritory()) {
        //        //NO LOG YET IF NEW HOME IS TERRITORY

        //        //log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
        //        //log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        //log.AddToFillers(null, actor.homeStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.STRING_1);
        //    }
        //    return log;
        //}
        #endregion

        private void SetNewHomeSettlement(Character actor) {
            string log = string.Empty;
#if DEBUG_LOG
            log = "Setting new home for " + actor.name;
#endif
            Region currentRegion = actor.currentRegion;

            if (actor is Summon) {
                //Character is a monster
#if DEBUG_LOG
                log += "\n-Character is a monster";
                log += "\n-Find an unoccupied Special Structure within the region and randomly select one. Clear out Territory data if it has one.";
#endif
                LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles();
                if (chosenHomeStructure != null) {
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
#if DEBUG_LOG
                    log += "\n-Special Structure found: " + chosenHomeStructure.ToString();
                    actor.logComponent.PrintLogIfActive(log);
#endif
                    return;
                }
#if DEBUG_LOG
                log += "\n-If none available and character does not have a Territory, 50% chance to set a random structure-less Area as its Territory.";
#endif
                if (!actor.HasTerritory()) {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
#if DEBUG_LOG
                        log += "\n-Getting structureless hex tile in current region: " + currentRegion.name;
#endif
                        Area area = currentRegion.GetRandomAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage();
                        if (area != null) {
                            actor.SetTerritory(area);
#if DEBUG_LOG
                            log += "\n-Area found: " + area.locationName;
                            actor.logComponent.PrintLogIfActive(log);
#endif
                            return;
                        }
                    }
                } else {
#if DEBUG_LOG
                    log += "\n-Character already has territory";
#endif
                }
            } else {
                //Character is not a summon
                if (actor.isVagrantOrFactionless) {
                    SetNewHomeSettlementForVagrant(actor, ref log);
                } else {
                    SetNewHomeSettlementForNonVagrant(actor, ref log);
                }
            }
            //If all else fails, check if character has home structure and if it is already destroyed, set it to null
            if (actor.homeStructure != null && actor.homeStructure.hasBeenDestroyed) {
                actor.MigrateHomeStructureTo(null, affectSettlement: false);
            }
#if DEBUG_LOG
            actor.logComponent.PrintLogIfActive(log);
#endif
        }

        #region Set Home Parts
        private void SetNewHomeSettlementForVagrant(Character actor, ref string log) {
            Region currentRegion = actor.currentRegion;
#if DEBUG_LOG
            log += "\n-Character is a vagrant";
#endif
            if (actor.homeStructure == null || actor.homeStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                log += "\n-Character has no home structure";
#endif
                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-40% chance: find an unoccupied but Habitable Special Structure within the region and randomly select one as its new Home Structure";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 40) {
                    LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
                    if (chosenHomeStructure != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
#endif
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-20% chance: find an unoccupied Village or Village occupied only by Vagrants within the region and randomly select one of its Structures (prioritize Dwellings) as its new Home Structure.  Clear out Territory data if it has one.";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 20) {
                    BaseSettlement chosenSettlement = currentRegion.GetFirstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf(actor);
                    if (chosenSettlement != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Settlement: " + chosenSettlement.name;
#endif
                        LocationStructure chosenHomeStructure = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(chosenSettlement, actor);
                        if (chosenHomeStructure != null) {
#if DEBUG_LOG
                            log += "\n-Chosen Home Structure: " + chosenHomeStructure.name;
#endif
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
#if DEBUG_LOG
                log += "\n-Otherwise, set a random structure-less Area as its Territory and make character go there.";
#endif
                if (!actor.HasTerritory()) {
#if DEBUG_LOG
                    log += "\n-Character has no territory";
#endif
                    Area area = currentRegion.GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption();
                    if (area != null) {
                        actor.SetTerritory(area);
#if DEBUG_LOG
                        log += "\n-Territory found: " + area.locationName;
                        actor.logComponent.PrintLogIfActive(log);
#endif
                        return;
                    }
                } else {
#if DEBUG_LOG
                    log += "\n-Character already has territory";
#endif
                }
            }
        }
        private void SetNewHomeSettlementForNonVagrant(Character actor, ref string log) {
            Region currentRegion = actor.currentRegion;
#if DEBUG_LOG
            log += "\nCharacter is not vagrant";
#endif
            if (actor.homeSettlement != null && actor.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
#if DEBUG_LOG
                log += "\nCharacter is still part of a village";
                log += "\nFind an unoccupied House and set that as its Home Structure - exclude previous Home.";
#endif
//                 LocationStructure chosenDwelling = actor.homeSettlement.GetFirstStructureThatIsUnoccupiedDwelling(actor.previousCharacterDataComponent.previousHomeStructure);
//                 if (chosenDwelling != null) {
// #if DEBUG_LOG
//                     log += "\nFound dwelling: " + chosenDwelling.name;
// #endif
//                     actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
//                 } else {
// #if DEBUG_LOG
//                     log += "\nFind dwelling that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friend";
// #endif
//                     chosenDwelling = GetDwellingWithCloseFriendOrNonRivalEnemyRelativeInSameFaction(actor.homeSettlement, actor);
//                     if (chosenDwelling != null) {
// #if DEBUG_LOG
//                         log += "\nFound dwelling: " + chosenDwelling.name;
// #endif
//                         actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
//                     } else {
                        int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                        log += "\n35% chance: if the character's Faction also owns other Villages or Special Structure";
                        log += "\n-Roll: " + roll;
#endif
                        if (roll < 35) {
                            if (actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {

//                                 bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlements(actor, ref log);
//                                 if (hasFound) {
// #if DEBUG_LOG
//                                     actor.logComponent.PrintLogIfActive(log);
// #endif
//                                     return;
//                                 }

#if DEBUG_LOG
                                log += "\nFind an unoccupied but Habitable Special Structure within the region";
#endif
                                LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
                                if (chosenHomeStructure != null) {
#if DEBUG_LOG
                                    log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
#endif
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
#if DEBUG_LOG
                                    actor.logComponent.PrintLogIfActive(log);
#endif
                                    return;
                                }

#if DEBUG_LOG
                                log += "\n-Set a random structure-less Area as its Territory and make character go there";
#endif
                                Area area = currentRegion.GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption();
                                if (area != null) {
                                    actor.ClearTerritory();
                                    actor.SetTerritory(area);
#if DEBUG_LOG
                                    log += "\n-Territory found: " + area.locationName;
                                    actor.logComponent.PrintLogIfActive(log);
#endif
                                    return;
                                }
                            }
                        }
                // }
                // }
            } else if (actor.homeStructure != null && !actor.homeStructure.hasBeenDestroyed && actor.homeStructure.HasStructureTag(STRUCTURE_TAG.Shelter)) {
#if DEBUG_LOG
                log += "\nCharacter is part of a habitable structure";
#endif
                int chance = 20;
                if (actor.homeStructure.HasAliveResident(actor)) {
#if DEBUG_LOG
                    log += "\n3% chance: because there is a resident other than the actor";
#endif
                    chance = 3;
                } else {
#if DEBUG_LOG
                    log += "\n20% chance: because actor lives alone";
#endif
                }
                if (GameUtilities.RollChance(chance)) {
#if DEBUG_LOG
                    log += "\n30% chance: if the character's Faction also owns other Villages or Special Structure (excluding current home Special Structure)";
#endif
                    if (GameUtilities.RollChance(30) && actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {
                        bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(actor, ref log);
                        if (hasFound) {
#if DEBUG_LOG
                            actor.logComponent.PrintLogIfActive(log);
#endif
                            return;
                        }
                    } else {
                        bool hasFoundNewVillage = FindNewVillageProcessing(actor, true, ref log);
                        if (hasFoundNewVillage) {
#if DEBUG_LOG
                            actor.logComponent.PrintLogIfActive(log);
#endif
                            return;
                        }
                    }
                }
            } else {
                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\nCharacter is not part of a village or habitable structure";
                log += "\n35% chance: if the character is a Faction Leader and they do not own any Villages";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 35) {
#if DEBUG_LOG
                    log += "\nCreate a Found New Village Job if the current region's Village capacity is not yet full.";
#endif
                    if (actor.isFactionLeader && !actor.faction.HasOwnedVillages() && actor.currentRegion != null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached()) {
                        bool hasFoundNewVillage = FindNewVillageProcessing(actor, false, ref log);
                        if (hasFoundNewVillage) {
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }

                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n3% chance: Create a Found New Village Job if the current region's Village capacity is not yet full and there are no other active Found New Village Jobs amongst members of the character's Faction.";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 3) {
                    if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached()) {
                        bool hasFoundNewVillage = FindNewVillageProcessing(actor, true, ref log);
                        if (hasFoundNewVillage) {
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n80% chance: if the character's Faction also owns other Villages or Special Structure";
                log += "\n-Roll: " + roll;
#endif
                LocationStructure chosenHomeStructure = null;
                if (roll < 80) {
                    if (actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {
                        bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(actor, ref log);
                        if (hasFound) {
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-15% chance: find an unoccupied Village within the region and randomly select one of its Structures (prioritize Dwellings) as its new Home Structure. Clear out Territory data if it has one.";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 15) {
                    BaseSettlement chosenSettlement = currentRegion.GetFirstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf(actor);
                    if (chosenSettlement != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Settlement: " + chosenSettlement.name;
#endif
                        chosenHomeStructure = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(chosenSettlement, actor);
                        if (chosenHomeStructure != null) {
#if DEBUG_LOG
                            log += "\n-Chosen Home Structure: " + chosenHomeStructure.name;
#endif
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-15% chance: Find an unoccupied but Habitable Special Structure within the region";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 15) {
                    chosenHomeStructure = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
                    if (chosenHomeStructure != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
#endif
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                }

#if DEBUG_LOG
                log += "\n-Set a random structure-less Area as its Territory and make character go there";
#endif
                Area area = currentRegion.GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption();
                if (area != null) {
                    actor.ClearTerritory();
                    actor.SetTerritory(area);
#if DEBUG_LOG
                    log += "\n-Territory found: " + area.locationName;
#endif
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
                            chosenDwelling = npcSettlement.GetFirstStructureThatIsUnoccupiedDwelling(actor.previousCharacterDataComponent.previousHomeStructure);
                            if (chosenDwelling != null) {
                                identifier = "unoccupied";
                                return chosenDwelling;
                            } else {
                                chosenDwellingWithCloseFriendOrNonEnemyRivalRelative = GetDwellingWithCloseFriendOrNonRivalEnemyRelativeInSameFaction(npcSettlement, actor);
                            }
                        }
                    }
                    if (chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative == null) {
                        if (baseSettlement.locationType == LOCATION_TYPE.DUNGEON) {
                            for (int j = 0; j < baseSettlement.allStructures.Count; j++) {
                                LocationStructure structure = baseSettlement.allStructures[j];
                                if (structure != actor.previousCharacterDataComponent.previousHomeStructure && !structure.HasReachedMaxResidentCapacity() && !IsSameAsCurrentHomeStructure(structure, actor)) {
                                    if (structure.HasCloseFriendOrNonEnemyRivalRelativeInSameFaction(actor)) {
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
        private bool FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlements(Character actor, ref string log) {
#if DEBUG_LOG
            log += "\nFind an unoccupied House in one of those other Villages and set that as its Home Structure";
#endif
            string identifier = string.Empty;
            LocationStructure chosenHomeStructure = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(actor, ref identifier);
            if (chosenHomeStructure != null && identifier == "unoccupied") {
#if DEBUG_LOG
                log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }

#if DEBUG_LOG
            log += "\nFind a Habitable Special Structure or House that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friende";
#endif
            if (chosenHomeStructure != null && identifier == "occupied") {
#if DEBUG_LOG
                log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }

            if (chosenHomeStructure != null && identifier == "habitable") {
#if DEBUG_LOG
                log += "\nFound Habitable Structure: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }
            return false;
        }
        private bool FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(Character actor, ref string log) {
            // bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlements(actor, ref log);
            // if (hasFound) {
            //     return true;
            // }

#if DEBUG_LOG
            log += "\nIf none available: Find the Village with least number of Villagers owned by the character's Faction and set its Town Center as its Home Structure. Make character go there.";
#endif
            LocationStructure chosenHomeStructure = GetFirstStructureOfTypeFromOwnedSettlementsWithLeastVillagers(STRUCTURE_TYPE.CITY_CENTER, actor.faction, actor);
            if (chosenHomeStructure != null) {
#if DEBUG_LOG
                log += "\nFound City Center: " + chosenHomeStructure.name + " in " + chosenHomeStructure.settlementLocation.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }
            return false;
        }
        private bool FindNewVillageProcessing(Character actor, bool checkIfThereAreOtherFindVillageJob, ref string log) {
#if DEBUG_LOG
            log += "\nCreate a Found New Village Job";
#endif
            if (actor.traitContainer.HasTrait("Enslaved")) {
#if DEBUG_LOG
                log += "\nCharacter is Enslaved, do not find village";
#endif
                return false;
            }
            // Area targetArea = actor.currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
            VillageSpot villageSpot = actor.currentRegion.GetRandomUnoccupiedVillageSpot();
            if (villageSpot != null) {
                Area targetArea = villageSpot.mainSpot;
                if (!checkIfThereAreOtherFindVillageJob || !FactionMemberAlreadyHasFindVillageJob(actor.faction)) {
#if DEBUG_LOG
                    log += "\nTriggered found new village";
#endif
                    StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, actor.faction.factionType.mainResource, actor.faction.factionType.usesCorruptedStructures); //character.faction.factionType.mainResource
                    List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
                    GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                    actor.jobComponent.TriggerFindNewVillage(targetArea.gridTileComponent.centerGridTile, chosenStructurePrefab.name);
                    return true;
                } else {
#if DEBUG_LOG
                    log += $"\n-Another faction member already has a find new village job";
#endif
                }
            } else {
#if DEBUG_LOG
                log += $"\n-No target area to find new village";
#endif
            }
            return false;
        }
#endregion

        #region Utilities
        private LocationStructure GetDwellingWithCloseFriendOrNonRivalEnemyRelativeInSameFaction(NPCSettlement settlement, Character actor) {
            LocationStructure chosenDwelling = null;
            List<LocationStructure> dwellings = settlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if (dwellings != null) {
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure currDwelling = dwellings[i];
                    if (currDwelling != actor.previousCharacterDataComponent.previousHomeStructure && !currDwelling.HasReachedMaxResidentCapacity() && currDwelling.residents.Count > 0 && !IsSameAsCurrentHomeStructure(currDwelling, actor)) {
                        if (currDwelling.HasCloseFriendOrNonEnemyRivalRelativeInSameFaction(actor)) {
                            chosenDwelling = currDwelling;
                            break;
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
                if (currStructure != actor.previousCharacterDataComponent.previousHomeStructure && currStructure != actor.homeStructure) {
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
                if (settlement != actor.previousCharacterDataComponent.previousHomeSettlement && settlement != actor.homeSettlement) {
                    LocationStructure structureOfType = settlement.GetFirstStructureWithStructureType(structureType, actor.previousCharacterDataComponent.previousHomeStructure, actor.homeStructure);
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
        private bool FactionMemberAlreadyHasFindVillageJob(Faction faction) {
            for (int i = 0; i < faction.characters.Count; i++) {
                Character factionMember = faction.characters[i];
                if (!factionMember.isDead && factionMember.jobQueue.HasJob(JOB_TYPE.FIND_NEW_VILLAGE)) {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}

/*
 *  SET HOME COPY BEFORE VILLAGER WANTS
 * 
 * private void SetNewHomeSettlement(Character actor) {
            string log = string.Empty;
#if DEBUG_LOG
            log = "Setting new home for " + actor.name;
#endif
            Region currentRegion = actor.currentRegion;

            if (actor is Summon) {
                //Character is a monster
#if DEBUG_LOG
                log += "\n-Character is a monster";
                log += "\n-Find an unoccupied Special Structure within the region and randomly select one. Clear out Territory data if it has one.";
#endif
                LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles();
                if (chosenHomeStructure != null) {
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
#if DEBUG_LOG
                    log += "\n-Special Structure found: " + chosenHomeStructure.ToString();
                    actor.logComponent.PrintLogIfActive(log);
#endif
                    return;
                }
#if DEBUG_LOG
                log += "\n-If none available and character does not have a Territory, 50% chance to set a random structure-less Area as its Territory.";
#endif
                if (!actor.HasTerritory()) {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
#if DEBUG_LOG
                        log += "\n-Getting structureless hex tile in current region: " + currentRegion.name;
#endif
                        Area area = currentRegion.GetRandomAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage();
                        if (area != null) {
                            actor.SetTerritory(area);
#if DEBUG_LOG
                            log += "\n-Area found: " + area.locationName;
                            actor.logComponent.PrintLogIfActive(log);
#endif
                            return;
                        }
                    }
                } else {
#if DEBUG_LOG
                    log += "\n-Character already has territory";
#endif
                }
            } else {
                //Character is not a summon
                if (actor.isVagrantOrFactionless) {
                    SetNewHomeSettlementForVagrant(actor, ref log);
                } else {
                    SetNewHomeSettlementForNonVagrant(actor, ref log);
                }
            }
            //If all else fails, check if character has home structure and if it is already destroyed, set it to null
            if (actor.homeStructure != null && actor.homeStructure.hasBeenDestroyed) {
                actor.MigrateHomeStructureTo(null, affectSettlement: false);
            }
#if DEBUG_LOG
            actor.logComponent.PrintLogIfActive(log);
#endif
        }

        #region Set Home Parts
        private void SetNewHomeSettlementForVagrant(Character actor, ref string log) {
            Region currentRegion = actor.currentRegion;
#if DEBUG_LOG
            log += "\n-Character is a vagrant";
#endif
            if (actor.homeStructure == null || actor.homeStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                log += "\n-Character has no home structure";
#endif
                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-40% chance: find an unoccupied but Habitable Special Structure within the region and randomly select one as its new Home Structure";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 40) {
                    LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
                    if (chosenHomeStructure != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
#endif
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-20% chance: find an unoccupied Village or Village occupied only by Vagrants within the region and randomly select one of its Structures (prioritize Dwellings) as its new Home Structure.  Clear out Territory data if it has one.";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 20) {
                    BaseSettlement chosenSettlement = currentRegion.GetFirstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf(actor);
                    if (chosenSettlement != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Settlement: " + chosenSettlement.name;
#endif
                        LocationStructure chosenHomeStructure = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(chosenSettlement, actor);
                        if (chosenHomeStructure != null) {
#if DEBUG_LOG
                            log += "\n-Chosen Home Structure: " + chosenHomeStructure.name;
#endif
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
#if DEBUG_LOG
                log += "\n-Otherwise, set a random structure-less Area as its Territory and make character go there.";
#endif
                if (!actor.HasTerritory()) {
#if DEBUG_LOG
                    log += "\n-Character has no territory";
#endif
                    Area area = currentRegion.GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption();
                    if (area != null) {
                        actor.SetTerritory(area);
#if DEBUG_LOG
                        log += "\n-Territory found: " + area.locationName;
                        actor.logComponent.PrintLogIfActive(log);
#endif
                        return;
                    }
                } else {
#if DEBUG_LOG
                    log += "\n-Character already has territory";
#endif
                }
            }
        }
        private void SetNewHomeSettlementForNonVagrant(Character actor, ref string log) {
            Region currentRegion = actor.currentRegion;
#if DEBUG_LOG
            log += "\nCharacter is not vagrant";
#endif
            if (actor.homeSettlement != null && actor.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
#if DEBUG_LOG
                log += "\nCharacter is still part of a village";
                log += "\nFind an unoccupied House and set that as its Home Structure - exclude previous Home.";
#endif
                LocationStructure chosenDwelling = actor.homeSettlement.GetFirstStructureThatIsUnoccupiedDwelling(actor.previousCharacterDataComponent.previousHomeStructure);
                if (chosenDwelling != null) {
#if DEBUG_LOG
                    log += "\nFound dwelling: " + chosenDwelling.name;
#endif
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
                } else {
#if DEBUG_LOG
                    log += "\nFind dwelling that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friend";
#endif
                    chosenDwelling = GetDwellingWithCloseFriendOrNonRivalEnemyRelativeInSameFaction(actor.homeSettlement, actor);
                    if (chosenDwelling != null) {
#if DEBUG_LOG
                        log += "\nFound dwelling: " + chosenDwelling.name;
#endif
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
                    } else {
                        int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                        log += "\n35% chance: if the character's Faction also owns other Villages or Special Structure";
                        log += "\n-Roll: " + roll;
#endif
                        if (roll < 35) {
                            if (actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {

                                bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlements(actor, ref log);
                                if (hasFound) {
#if DEBUG_LOG
                                    actor.logComponent.PrintLogIfActive(log);
#endif
                                    return;
                                }

#if DEBUG_LOG
                                log += "\nFind an unoccupied but Habitable Special Structure within the region";
#endif
                                LocationStructure chosenHomeStructure = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
                                if (chosenHomeStructure != null) {
#if DEBUG_LOG
                                    log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
#endif
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
#if DEBUG_LOG
                                    actor.logComponent.PrintLogIfActive(log);
#endif
                                    return;
                                }

#if DEBUG_LOG
                                log += "\n-Set a random structure-less Area as its Territory and make character go there";
#endif
                                Area area = currentRegion.GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption();
                                if (area != null) {
                                    actor.ClearTerritory();
                                    actor.SetTerritory(area);
#if DEBUG_LOG
                                    log += "\n-Territory found: " + area.locationName;
                                    actor.logComponent.PrintLogIfActive(log);
#endif
                                    return;
                                }
                            }
                        }
                    }
                }
            } else if (actor.homeStructure != null && !actor.homeStructure.hasBeenDestroyed && actor.homeStructure.HasStructureTag(STRUCTURE_TAG.Shelter)) {
#if DEBUG_LOG
                log += "\nCharacter is part of a habitable structure";
#endif
                int chance = 20;
                if (actor.homeStructure.HasAliveResidentOtherThan(actor)) {
#if DEBUG_LOG
                    log += "\n3% chance: because there is a resident other than the actor";
#endif
                    chance = 3;
                } else {
#if DEBUG_LOG
                    log += "\n20% chance: because actor lives alone";
#endif
                }
                if (GameUtilities.RollChance(chance)) {
#if DEBUG_LOG
                    log += "\n30% chance: if the character's Faction also owns other Villages or Special Structure (excluding current home Special Structure)";
#endif
                    if (GameUtilities.RollChance(30) && actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {
                        bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(actor, ref log);
                        if (hasFound) {
#if DEBUG_LOG
                            actor.logComponent.PrintLogIfActive(log);
#endif
                            return;
                        }
                    } else {
                        bool hasFoundNewVillage = FindNewVillageProcessing(actor, true, ref log);
                        if (hasFoundNewVillage) {
#if DEBUG_LOG
                            actor.logComponent.PrintLogIfActive(log);
#endif
                            return;
                        }
                    }
                }
            } else {
                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\nCharacter is not part of a village or habitable structure";
                log += "\n35% chance: if the character is a Faction Leader and they do not own any Villages";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 35) {
#if DEBUG_LOG
                    log += "\nCreate a Found New Village Job if the current region's Village capacity is not yet full.";
#endif
                    if (actor.isFactionLeader && !actor.faction.HasOwnedVillages() && actor.currentRegion != null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached()) {
                        bool hasFoundNewVillage = FindNewVillageProcessing(actor, false, ref log);
                        if (hasFoundNewVillage) {
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }

                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n3% chance: Create a Found New Village Job if the current region's Village capacity is not yet full and there are no other active Found New Village Jobs amongst members of the character's Faction.";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 3) {
                    if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached()) {
                        bool hasFoundNewVillage = FindNewVillageProcessing(actor, true, ref log);
                        if (hasFoundNewVillage) {
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n80% chance: if the character's Faction also owns other Villages or Special Structure";
                log += "\n-Roll: " + roll;
#endif
                LocationStructure chosenHomeStructure = null;
                if (roll < 80) {
                    if (actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement)) {
                        bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(actor, ref log);
                        if (hasFound) {
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-15% chance: find an unoccupied Village within the region and randomly select one of its Structures (prioritize Dwellings) as its new Home Structure. Clear out Territory data if it has one.";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 15) {
                    BaseSettlement chosenSettlement = currentRegion.GetFirstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf(actor);
                    if (chosenSettlement != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Settlement: " + chosenSettlement.name;
#endif
                        chosenHomeStructure = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(chosenSettlement, actor);
                        if (chosenHomeStructure != null) {
#if DEBUG_LOG
                            log += "\n-Chosen Home Structure: " + chosenHomeStructure.name;
#endif
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
                roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += "\n-15% chance: Find an unoccupied but Habitable Special Structure within the region";
                log += "\n-Roll: " + roll;
#endif
                if (roll < 15) {
                    chosenHomeStructure = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
                    if (chosenHomeStructure != null) {
#if DEBUG_LOG
                        log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
#endif
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                }

#if DEBUG_LOG
                log += "\n-Set a random structure-less Area as its Territory and make character go there";
#endif
                Area area = currentRegion.GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption();
                if (area != null) {
                    actor.ClearTerritory();
                    actor.SetTerritory(area);
#if DEBUG_LOG
                    log += "\n-Territory found: " + area.locationName;
#endif
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
                            chosenDwelling = npcSettlement.GetFirstStructureThatIsUnoccupiedDwelling(actor.previousCharacterDataComponent.previousHomeStructure);
                            if (chosenDwelling != null) {
                                identifier = "unoccupied";
                                return chosenDwelling;
                            } else {
                                chosenDwellingWithCloseFriendOrNonEnemyRivalRelative = GetDwellingWithCloseFriendOrNonRivalEnemyRelativeInSameFaction(npcSettlement, actor);
                            }
                        }
                    }
                    if (chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative == null) {
                        if (baseSettlement.locationType == LOCATION_TYPE.DUNGEON) {
                            for (int j = 0; j < baseSettlement.allStructures.Count; j++) {
                                LocationStructure structure = baseSettlement.allStructures[j];
                                if (structure != actor.previousCharacterDataComponent.previousHomeStructure && !structure.HasReachedMaxResidentCapacity() && !IsSameAsCurrentHomeStructure(structure, actor)) {
                                    if (structure.HasCloseFriendOrNonEnemyRivalRelativeInSameFaction(actor)) {
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
        private bool FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlements(Character actor, ref string log) {
#if DEBUG_LOG
            log += "\nFind an unoccupied House in one of those other Villages and set that as its Home Structure";
#endif
            string identifier = string.Empty;
            LocationStructure chosenHomeStructure = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(actor, ref identifier);
            if (chosenHomeStructure != null && identifier == "unoccupied") {
#if DEBUG_LOG
                log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }

#if DEBUG_LOG
            log += "\nFind a Habitable Special Structure or House that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friende";
#endif
            if (chosenHomeStructure != null && identifier == "occupied") {
#if DEBUG_LOG
                log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }

            if (chosenHomeStructure != null && identifier == "habitable") {
#if DEBUG_LOG
                log += "\nFound Habitable Structure: " + chosenHomeStructure.name + " in " + chosenHomeStructure.region.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }
            return false;
        }
        private bool FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(Character actor, ref string log) {
            bool hasFound = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlements(actor, ref log);
            if (hasFound) {
                return true;
            }

#if DEBUG_LOG
            log += "\nIf none available: Find the Village with least number of Villagers owned by the character's Faction and set its Town Center as its Home Structure. Make character go there.";
#endif
            LocationStructure chosenHomeStructure = GetFirstStructureOfTypeFromOwnedSettlementsWithLeastVillagers(STRUCTURE_TYPE.CITY_CENTER, actor.faction, actor);
            if (chosenHomeStructure != null) {
#if DEBUG_LOG
                log += "\nFound City Center: " + chosenHomeStructure.name + " in " + chosenHomeStructure.settlementLocation.name;
#endif
                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                return true;
            }
            return false;
        }
        private bool FindNewVillageProcessing(Character actor, bool checkIfThereAreOtherFindVillageJob, ref string log) {
#if DEBUG_LOG
            log += "\nCreate a Found New Village Job";
#endif
            if (actor.traitContainer.HasTrait("Enslaved")) {
#if DEBUG_LOG
                log += "\nCharacter is Enslaved, do not find village";
#endif
                return false;
            }
            // Area targetArea = actor.currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
            VillageSpot villageSpot = actor.currentRegion.GetRandomUnoccupiedVillageSpot();
            if (villageSpot != null) {
                Area targetArea = villageSpot.mainSpot;
                if (!checkIfThereAreOtherFindVillageJob || !FactionMemberAlreadyHasFindVillageJob(actor.faction)) {
#if DEBUG_LOG
                    log += "\nTriggered found new village";
#endif
                    StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, actor.faction.factionType.mainResource, actor.faction.factionType.usesCorruptedStructures); //character.faction.factionType.mainResource
                    List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
                    GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                    actor.jobComponent.TriggerFindNewVillage(targetArea.gridTileComponent.centerGridTile, chosenStructurePrefab.name);
                    return true;
                } else {
#if DEBUG_LOG
                    log += $"\n-Another faction member already has a find new village job";
#endif
                }
            } else {
#if DEBUG_LOG
                log += $"\n-No target area to find new village";
#endif
            }
            return false;
        }
#endregion
 *
 * 
 */
