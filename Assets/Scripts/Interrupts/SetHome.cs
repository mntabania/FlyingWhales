using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

namespace Interrupts {
    public class SetHome : Interrupt {
        public SetHome() : base(INTERRUPT.Set_Home) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(target != null) {
                //This means that the new home is predetermined
                if(target is Character targetCharacter) {
                    actor.MigrateHomeStructureTo(targetCharacter.homeStructure);
                }
            } else {
                SetNewHomeSettlement(actor);
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
            Log log = null;
            if (actor.homeStructure != null) {
                log = new Log(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, actor.homeStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.STRING_1);    
            } else if (actor.HasTerritory()) {
                //NO LOG YET IF NEW HOME IS TERRITORY

                //log = new Log(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
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
                LocationStructure chosenHomeStructure = currentRegion.GetRandomUnoccupiedSpecialStructure();
                if (chosenHomeStructure != null) {
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                    log += "\n-Special Structure found: " + chosenHomeStructure.ToString();
                    actor.logComponent.PrintLogIfActive(log);
                    return;
                }
                log += "\n-If none available and character does not have a Territory, 50% chance to set a random structure-less Area as its Territory.";
                if (!actor.HasTerritory()) {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        log += "\n-Getting structureless hex tile in current region: " + currentRegion.name;
                        HexTile hex = currentRegion.GetRandomNoStructureUncorruptedNotPartOrNextToVillagePlainHex();
                        if (hex != null) {
                            actor.AddTerritory(hex);
                            log += "\n-Hex tile found: " + hex.tileName;
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        }
                    }
                }
            } else {
                //Character is not a summon
                if (actor.isFriendlyFactionless) {
                    log += "\n-Character is a vagrant";
                    if (actor.homeStructure == null || actor.homeStructure.hasBeenDestroyed) {
                        log += "\n-Character has no home structure";
                        log += "\n-20% chance to join faction";
                        int roll = UnityEngine.Random.Range(0, 100);
                        log += "\n-Roll: " + roll;
                        if (roll < 20) {
                            Faction joinedFaction = actor.JoinFactionProcessing();
                            if (joinedFaction != null) {
                                log += $"\n-Chosen faction to join: {joinedFaction.name}";
                                SetNewHomeSettlementForNonVagrant(actor, ref log);
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            } else {
                                log += "\n-No available faction for character";
                            }
                        }
                        log += "\n-60% chance: find an unoccupied but Habitable Special Structure within the region and randomly select one as its new Home Structure";
                        roll = UnityEngine.Random.Range(0, 100);
                        log += "\n-Roll: " + roll;
                        if (roll < 60) {
                            LocationStructure chosenHomeStructure = currentRegion.GetRandomUnoccupiedStructureWithTag(STRUCTURE_TAG.Shelter);
                            if (chosenHomeStructure != null) {
                                log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
                                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }
                        }
                        if (!actor.HasTerritory()) {
                            log += "\n-Character has no territory";
                            HexTile hex = currentRegion.GetRandomNoStructureUncorruptedPlainHex();
                            if (hex != null) {
                                actor.AddTerritory(hex);
                                log += "\n-Territory found: " + hex.tileName;
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }
                        } else {
                            log += "\n-Character has territory, 15% chance to change territory to adjacent region";
                            roll = UnityEngine.Random.Range(0, 100);
                            log += "\n-Roll: " + roll;
                            if (roll < 15) {
                                HexTile territory = GetTerritoryInAdjacentRegions(currentRegion);
                                if (territory != null) {
                                    actor.ClearTerritory();
                                    actor.AddTerritory(territory);
                                    log += "\n-Territory found: " + territory.tileName + " in region: " + territory.region.name;
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }
                            }
                        }
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
                LocationStructure chosenDwelling = GetUnoccupiedDwelling(actor.homeSettlement);
                if(chosenDwelling != null) {
                    log += "\nFound dwelling: " + chosenDwelling.name;
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
                } else {
                    log += "\nFind dwelling that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friend";
                    chosenDwelling = GetDwellingWithCloseFriendOrNonRivalEnemyRelative(actor.homeSettlement, actor);
                    if (chosenDwelling != null) {
                        log += "\nFound dwelling: " + chosenDwelling.name;
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenDwelling, affectSettlement: false);
                    } else {
                        log += "\n20% chance: if the character's Faction also owns other Villages or Special Structure";
                        int roll = UnityEngine.Random.Range(0, 100);
                        log += "\n-Roll: " + roll;
                        if (roll < 20) {
                            log += "\nFind an unoccupied House in one of those other Villages and set that as its Home Structure";
                            string identifier = string.Empty;
                            LocationStructure chosenHomeStructure = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(actor, ref identifier);
                            if(chosenHomeStructure != null && identifier == "unoccupied") {
                                log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.location.name;
                                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }

                            log += "\nFind a Habitable Special Structure or House that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friende";
                            if (chosenHomeStructure != null && identifier == "occupied") {
                                log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.location.name;
                                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }

                            if (chosenHomeStructure != null && identifier == "habitable") {
                                log += "\nFound Habitable Structure: " + chosenHomeStructure.name + " in " + chosenHomeStructure.location.name;
                                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }

                            log += "\nFind an unoccupied but Habitable Special Structure within the region";
                            chosenHomeStructure = currentRegion.GetRandomUnoccupiedStructureWithTag(STRUCTURE_TAG.Shelter);
                            if (chosenHomeStructure != null) {
                                log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
                                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }

                            log += "\n-15% chance: set Territory to a random structure-less Area in one of the adjacent regions";
                            roll = UnityEngine.Random.Range(0, 100);
                            log += "\n-Roll: " + roll;
                            if (roll < 15) {
                                HexTile territory = GetTerritoryInAdjacentRegions(currentRegion);
                                if (territory != null) {
                                    actor.ClearTerritory();
                                    actor.AddTerritory(territory);
                                    log += "\n-Territory found: " + territory.tileName + " in region: " + territory.region.name;
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }
                            }

                            log += "\n-Set a random structure-less Area as its Territory and make character go there";
                            HexTile hex = currentRegion.GetRandomNoStructureUncorruptedPlainHex();
                            if (hex != null) {
                                actor.ClearTerritory();
                                actor.AddTerritory(hex);
                                log += "\n-Territory found: " + hex.tileName;
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }
                        }

                        //TODO: Build House
                    }
                }
            } else {
                log += "\nCharacter is not part of a village";
                log += "\n20% chance: if the character's Faction also owns other Villages or Special Structure";
                int roll = UnityEngine.Random.Range(0, 100);
                log += "\n-Roll: " + roll;
                LocationStructure chosenHomeStructure = null;
                if (roll < 20) {
                    log += "\nFind an unoccupied House in one of those other Villages and set that as its Home Structure";
                    string identifier = string.Empty;
                    chosenHomeStructure = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(actor, ref identifier);
                    if (chosenHomeStructure != null && identifier == "unoccupied") {
                        log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.location.name;
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }

                    log += "\nFind a Habitable Special Structure or House that is still not at full capacity and is home of a non-enemy and non-rival relative or a non-relative but close friend";
                    if (chosenHomeStructure != null && identifier == "occupied") {
                        log += "\nFound dwelling: " + chosenHomeStructure.name + " in " + chosenHomeStructure.location.name;
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }

                    if (chosenHomeStructure != null && identifier == "habitable") {
                        log += "\nFound Habitable Structure: " + chosenHomeStructure.name + " in " + chosenHomeStructure.location.name;
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                }

                log += "\nFind an unoccupied but Habitable Special Structure within the region";
                chosenHomeStructure = currentRegion.GetRandomUnoccupiedStructureWithTag(STRUCTURE_TAG.Shelter);
                if (chosenHomeStructure != null) {
                    log += "\n-Chosen Habitable Structure: " + chosenHomeStructure.name;
                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                    actor.logComponent.PrintLogIfActive(log);
                    return;
                }

                log += "\n-15% chance: set Territory to a random structure-less Area in one of the adjacent regions";
                roll = UnityEngine.Random.Range(0, 100);
                log += "\n-Roll: " + roll;
                if (roll < 15) {
                    HexTile territory = GetTerritoryInAdjacentRegions(currentRegion);
                    if (territory != null) {
                        actor.ClearTerritory();
                        actor.AddTerritory(territory);
                        log += "\n-Territory found: " + territory.tileName + " in region: " + territory.region.name;
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                }

                log += "\n-Set a random structure-less Area as its Territory and make character go there";
                HexTile hex = currentRegion.GetRandomNoStructureUncorruptedPlainHex();
                if (hex != null) {
                    actor.ClearTerritory();
                    actor.AddTerritory(hex);
                    log += "\n-Territory found: " + hex.tileName;
                    actor.logComponent.PrintLogIfActive(log);
                    return;
                }
            }
        }

        private LocationStructure FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(Character actor, ref string identifier) {
            LocationStructure chosenDwellingWithCloseFriendOrNonEnemyRivalRelative = null;
            LocationStructure chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative = null;
            LocationStructure chosenDwelling = null;
            if (actor.faction.HasOwnedSettlementExcept(actor.homeSettlement)) {
                for (int i = 0; i < actor.faction.ownedSettlements.Count; i++) {
                    BaseSettlement baseSettlement = actor.faction.ownedSettlements[i];
                    if (baseSettlement != actor.homeSettlement) {
                        if(baseSettlement.locationType == LOCATION_TYPE.SETTLEMENT) {
                            if (baseSettlement is NPCSettlement npcSettlement) {
                                chosenDwelling = GetUnoccupiedDwelling(npcSettlement);
                                if (chosenDwelling != null) {
                                    identifier = "unoccupied";
                                    return chosenDwelling;
                                } else {
                                    chosenDwellingWithCloseFriendOrNonEnemyRivalRelative = GetDwellingWithCloseFriendOrNonRivalEnemyRelative(npcSettlement, actor);
                                }
                            }
                        }
                    }
                    if(chosenHabitableSpecialWithCloseFriendOrNonEnemyRivalRelative == null) {
                        if (baseSettlement.locationType == LOCATION_TYPE.DUNGEON) {
                            for (int j = 0; j < baseSettlement.allStructures.Count; j++) {
                                LocationStructure structure = baseSettlement.allStructures[j];
                                if (structure.residents.Count < structure.maxResidentCapacity) {
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
        private HexTile GetTerritoryInAdjacentRegions(Region region) {
            List<Region> adjacentRegions = region.AdjacentRegions();
            if(adjacentRegions != null) {
                while (adjacentRegions.Count > 0) {
                    Region chosenAdjacentRegion = adjacentRegions[UnityEngine.Random.Range(0, adjacentRegions.Count)];
                    HexTile hex = chosenAdjacentRegion.GetRandomNoStructureUncorruptedPlainHex();
                    if (hex != null) {
                        return hex;
                    } else {
                        adjacentRegions.Remove(chosenAdjacentRegion);
                    }
                }
            }
            return null;
        } 
        private NPCSettlement GetNewSettlementHomeFromFactionMember(Character actor) {
            NPCSettlement chosenSettlement = null;
            if (actor.faction.isMajorNonPlayer) {
                List<NPCSettlement> npcSettlements = null;
                for (int i = 0; i < actor.faction.characters.Count; i++) {
                    Character factionMember = actor.faction.characters[i];
                    if (factionMember.isSettlementRuler) {
                        if (npcSettlements == null) { npcSettlements = new List<NPCSettlement>(); }
                        npcSettlements.Add(factionMember.ruledSettlement);
                    }
                }
                if (npcSettlements != null && npcSettlements.Count > 0) {
                    chosenSettlement = npcSettlements[UnityEngine.Random.Range(0, npcSettlements.Count)];
                }
            }
            return chosenSettlement;
        }
        private LocationStructure GetUnoccupiedDwelling(NPCSettlement settlement) {
            LocationStructure chosenDwelling = null;
            List<LocationStructure> dwellings = settlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if (dwellings != null) {
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure currDwelling = dwellings[i];
                    if (!currDwelling.IsOccupied()) {
                        chosenDwelling = currDwelling;
                        break;
                    }
                }
            }
            return chosenDwelling;
        }
        private LocationStructure GetDwellingWithCloseFriendOrNonRivalEnemyRelative(NPCSettlement settlement, Character actor) {
            LocationStructure chosenDwelling = null;
            List<LocationStructure> dwellings = settlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if (dwellings != null) {
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure currDwelling = dwellings[i];
                    if (currDwelling.residents.Count < currDwelling.maxResidentCapacity && currDwelling.residents.Count > 0) {
                        Character resident = currDwelling.residents[0];
                        bool isCloseFriend = actor.relationshipContainer.IsFriendsWith(resident);
                        if (isCloseFriend) {
                            chosenDwelling = currDwelling;
                            break;
                        } else {
                            bool isNonRivalEnemyRelative = !actor.relationshipContainer.IsEnemiesWith(resident) && actor.relationshipContainer.HasRelationshipWith(resident, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.SIBLING, RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.PARENT);
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
    }
}