using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

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
            if(actor.homeStructure != null && actor.homeStructure.settlementLocation != null) {
                if(actor is Summon) {
                    actor.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
                } else {
                    actor.SetIsWanderer(false);
                }
            } else {
                if (actor is Summon) {
                    actor.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
                } else {
                    actor.SetIsWanderer(true);
                }
            }
            //if(actor.homeSettlement != null) {
            //    NPCSettlement npcSettlement = actor.homeSettlement;
            //    LocationStructure chosenHomeStructure = null;
            //    if(target != actor) {
            //        chosenHomeStructure = (target as Character).homeStructure;
            //    }
            //    actor.MigrateHomeStructureTo(null);
            //    npcSettlement.AssignCharacterToDwellingInArea(actor, chosenHomeStructure);
            //    //if(actor.homeStructure != null) {
            //    //    Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
            //    //    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //    //    log.AddToFillers(null, actor.homeStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.STRING_1);
            //    //    actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            //    //}
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
            if (actor.isFactionless || actor.isFriendlyFactionless) {
                log += "\n-Character is factionless";
                if (actor is Summon) {
                    //Character is a monster
                    log += "\n-Character is a monster";
                    log += "\n-Find an unoccupied Special Structure within the region and randomly select one. Clear out Territory data if it has one.";
                    Region currentRegion = actor.currentRegion;
                    LocationStructure chosenHomeStructure = currentRegion.GetRandomUnoccupiedStructureWithTags();
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
                            HexTile hex = currentRegion.GetRandomNoStructureNotPartOrNextToVillagePlainHex();
                            if (hex != null) {
                                actor.AddTerritory(hex);
                                log += "\n-Hex tile found: " + hex.tileName;
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            } 
                            //else if (GridMap.Instance.allRegions.Length > 1) {
                            //    Region randomRegion = GridMap.Instance.allRegions[UnityEngine.Random.Range(0, GridMap.Instance.allRegions.Length)];
                            //    log += "\n-Getting structureless hex tile in a random region: " + randomRegion.name;
                            //    hex = randomRegion.GetRandomNoStructurePlainHex();
                            //    if (hex != null) {
                            //        actor.AddTerritory(hex);
                            //        log += "\n-Hex tile found: " + hex.tileName;
                            //        actor.logComponent.PrintLogIfActive(log);
                            //        return;
                            //    }
                            //}
                        }
                    }
                } else {
                    log += "\n-Character is a resident";
                    log += "\n-Find an unoccupied but Habitable Special Structure within the region and randomly select one as its new Home Structure. Clear out Territory data if it has one.";
                    Region currentRegion = actor.currentRegion;
                    LocationStructure chosenHomeStructure = currentRegion.GetRandomUnoccupiedStructureWithTag(STRUCTURE_TAG.Shelter);
                    if (chosenHomeStructure != null) {
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        log += "\n-Habitable Special Structure found: " + chosenHomeStructure.ToString();
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                    log += "\n-If none available and character does not have a Territory, 50% chance to set a random structure-less Area as its Territory";
                    if (!actor.HasTerritory()) {
                        if (UnityEngine.Random.Range(0, 2) == 0) {
                            log += "\n-Getting structureless hex tile in current region: " + currentRegion.name;
                            HexTile hex = currentRegion.GetRandomNoStructureNotPartOrNextToVillagePlainHex();
                            if (hex != null) {
                                actor.AddTerritory(hex);
                                log += "\n-Hex tile found: " + hex.tileName;
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            }
                            //else if (GridMap.Instance.allRegions.Length > 1) {
                            //    Region randomRegion = GridMap.Instance.allRegions[UnityEngine.Random.Range(0, GridMap.Instance.allRegions.Length)];
                            //    log += "\n-Getting structureless hex tile in a random region: " + randomRegion.name;
                            //    hex = randomRegion.GetRandomNoStructurePlainHex();
                            //    if (hex != null) {
                            //        actor.AddTerritory(hex);
                            //        log += "\n-Hex tile found: " + hex.tileName;
                            //        actor.logComponent.PrintLogIfActive(log);
                            //        return;
                            //    }
                            //}
                        }
                    }
                }
            } else {
                //Character belongs to a non-neutral faction
                log += "\n-Character belongs to a non-neutral faction";
                if (actor is Summon) {
                    //Character is a monster
                    log += "\n-Character is a monster";
                    log += "\n-If possible, choose a random settlement whose Settlement Ruler is a member of the Faction";
                    NPCSettlement chosenSettlement = GetNewSettlementHomeFromFactionMember(actor);
                    if (chosenSettlement != null) {
                        for (int i = 0; i < chosenSettlement.tiles.Count; i++) {
                            actor.AddTerritory(chosenSettlement.tiles[i]);
                        }
                        log += "\n-Settlement found: " + chosenSettlement.name;
                        log += "\n-Setting all hex tiles as territories";
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                    log += "\n-If Home Structure is still not set";
                    log += "\n-Find an unoccupied Special Structure within the region and randomly select one. Clear out Territory data if it has one.";
                    Region currentRegion = actor.currentRegion;
                    LocationStructure chosenHomeStructure = currentRegion.GetRandomUnoccupiedStructureWithTags();
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
                            HexTile hex = currentRegion.GetRandomNoStructureNotPartOrNextToVillagePlainHex();
                            if (hex != null) {
                                actor.AddTerritory(hex);
                                log += "\n-Hex tile found: " + hex.tileName;
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            } 
                            //else if (GridMap.Instance.allRegions.Length > 1) {
                            //    Region randomRegion = GridMap.Instance.allRegions[UnityEngine.Random.Range(0, GridMap.Instance.allRegions.Length)];
                            //    log += "\n-Getting structureless hex tile in a random region: " + randomRegion.name;
                            //    hex = randomRegion.GetRandomNoStructurePlainHex();
                            //    if (hex != null) {
                            //        actor.AddTerritory(hex);
                            //        log += "\n-Hex tile found: " + hex.tileName;
                            //        actor.logComponent.PrintLogIfActive(log);
                            //        return;
                            //    }
                            //}
                        }
                    }
                } else {
                    log += "\n-Character is a resident";
                    log += "\n-If possible, choose a random settlement whose Settlement Ruler is a member of the Faction";
                    LocationStructure chosenHomeStructure = null;
                    NPCSettlement chosenSettlement = GetNewSettlementHomeFromFactionMember(actor);
                    if (chosenSettlement != null) {
                        log += "\n-If that settlement has at least one unoccupied dwelling, choose one at random and set that as its new Home Structure. Clear out Territory data if it has one.";
                        chosenHomeStructure = GetUnoccupiedDwelling(chosenSettlement);
                        if (chosenHomeStructure != null) {
                            actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                            log += "\n-Structure Found: " + chosenHomeStructure.ToString();
                            actor.logComponent.PrintLogIfActive(log);
                            return;
                        } else {
                            log += "\n-Otherwise, if settlement has a dwelling occupied by just one character who is a non-Enemy and non-Rival relative of the character, choose one at random and set that as its new Home Structure. Clear out Territory data if it has one.";
                            chosenHomeStructure = GetDwellingWithNonRivalEnemyRelative(chosenSettlement, actor);
                            if (chosenHomeStructure != null) {
                                actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                log += "\n-Structure Found: " + chosenHomeStructure.ToString();
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            } else {
                                log += "\n-Otherwise, if village center is only occupied by 2 or less residents, set that as its new Home Structure. Clear out Territory data if it has one..";
                                if (chosenSettlement.cityCenter.residents.Count <= 2) {
                                    chosenHomeStructure = chosenSettlement.cityCenter;
                                    actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                                    log += "\n-Structure Found: " + chosenHomeStructure.ToString();
                                    actor.logComponent.PrintLogIfActive(log);
                                    return;
                                }
                            }
                        }
                    }
                    log += "\n-If Home Structure is still not set";
                    log += "\n-Find an unoccupied but Habitable Special Structure within the region and randomly select one as its new Home Structure. Clear out Territory data if it has one.";
                    Region currentRegion = actor.currentRegion;
                    chosenHomeStructure = currentRegion.GetRandomUnoccupiedStructureWithTag(STRUCTURE_TAG.Shelter);
                    if(chosenHomeStructure != null) {
                        actor.ClearTerritoryAndMigrateHomeStructureTo(chosenHomeStructure);
                        log += "\n-Habitable Special Structure found: " + chosenHomeStructure.ToString();
                        actor.logComponent.PrintLogIfActive(log);
                        return;
                    }
                    log += "\n-If none available and character does not have a Territory, 50% chance to set a random structure-less Area as its Territory";
                    if (!actor.HasTerritory()) {
                        if(UnityEngine.Random.Range(0, 2) == 0) {
                            log += "\n-Getting structureless hex tile in current region: " + currentRegion.name;
                            HexTile hex = currentRegion.GetRandomNoStructureNotPartOrNextToVillagePlainHex();
                            if(hex != null) {
                                actor.AddTerritory(hex);
                                log += "\n-Hex tile found: " + hex.tileName;
                                actor.logComponent.PrintLogIfActive(log);
                                return;
                            } 
                            //else if (GridMap.Instance.allRegions.Length > 1){
                            //    Region randomRegion = GridMap.Instance.allRegions[UnityEngine.Random.Range(0, GridMap.Instance.allRegions.Length)];
                            //    log += "\n-Getting structureless hex tile in a random region: " + randomRegion.name;
                            //    hex = randomRegion.GetRandomNoStructurePlainHex();
                            //    if (hex != null) {
                            //        actor.AddTerritory(hex);
                            //        log += "\n-Hex tile found: " + hex.tileName;
                            //        actor.logComponent.PrintLogIfActive(log);
                            //        return;
                            //    }
                            //}
                        }
                    }
                }
            }
            //If all else fails, check if character has home structure and if it is already destroyed, set it to null
            if (actor.homeStructure != null && actor.homeStructure.hasBeenDestroyed) {
                actor.MigrateHomeStructureTo(null);
            }
            actor.logComponent.PrintLogIfActive(log);
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
        private LocationStructure GetDwellingWithNonRivalEnemyRelative(NPCSettlement settlement, Character actor) {
            LocationStructure chosenDwelling = null;
            List<LocationStructure> dwellings = settlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if (dwellings != null) {
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure currDwelling = dwellings[i];
                    if (currDwelling.residents.Count == 1) {
                        Character resident = currDwelling.residents[0];
                        if(actor.relationshipContainer.HasRelationshipWith(resident, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.PARENT)) {
                            string opinionLabel = actor.relationshipContainer.GetOpinionLabel(resident);
                            if(opinionLabel != RelationshipManager.Enemy && opinionLabel != RelationshipManager.Rival) {
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