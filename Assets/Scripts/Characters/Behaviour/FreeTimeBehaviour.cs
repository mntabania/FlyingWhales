using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class FreeTimeBehaviour : CharacterBehaviourComponent {
    private readonly string[] highTierClasses = new string[] {"Knight", "Hunter", "Mage", "Barbarian", "Stalker", "Shaman"};
    public FreeTimeBehaviour() {
        priority = 9;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log = $"{log}\n-{character.name} Free Time Behaviour";
#endif
        //needs recovery
        if (character.needsComponent.isStarving) {
#if DEBUG_LOG
            
            log = $"{log}\n-{character.name} is starving will try to do fullness recovery.";
#endif
            //add fullness recovery
            if (character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob)) {
                return true;
            }
        } else if (character.needsComponent.isHungry) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is hungry 20% chance to do fullness recovery.";
#endif
            if (GameUtilities.RollChance(20, ref log)) {
                //add fullness recovery
                if (character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob)) {
                    return true;
                }
            }
        }
        if (character.needsComponent.isSulking) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is sulking will try to do happiness recovery.";
#endif
            //add happiness recovery
            if (CreateHappinessRecoveryJob(character, out producedJob)) {
                return true;
            }
        } else if (character.needsComponent.isBored) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is bored 20% chance do happiness recovery.";
#endif
            if (GameUtilities.RollChance(20, ref log)) {
                //add happiness recovery
                if (CreateHappinessRecoveryJob(character, out producedJob)) {
                    return true;
                }
            }
        }

        //join or create party
        FreeTimePartyLogic(character, ref log);
        
#if DEBUG_LOG
        log = $"{log}\n-{character.name} Will try to obtain want.";
#endif
        //obtain want
        //only villagers part of major faction can process wants since all of it requires a character that is part of a faction that lives in a village with the needed facilities.
        if (!character.trapStructure.IsTrapped() && ChanceData.RollChance(CHANCE_TYPE.Free_Time_Obtain_Want, ref log) && character.faction != null && character.faction.isMajorFaction) { //20
#if DEBUG_LOG
            GameManager.stopwatch.Stop();
            GameManager.stopwatch.Reset();
            GameManager.stopwatch.Start();
#endif
            VillagerWant want = character.villagerWantsComponent.GetTopPriorityWant(character, out LocationStructure foundStructure);
#if DEBUG_LOG
            GameManager.stopwatch.Stop();
            UnityEngine.Debug.Log($"{character.name} wants processing took {GameManager.stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            log = $"{log}\n-Top priority want is {want?.name}.";
#endif
            if (want is DwellingWant) {
#if DEBUG_LOG
                log = $"{log}\n-Will purchase dwelling {foundStructure.name}.";
#endif
                //Purchase Dwelling. If Lover is in the same Faction, they will also automatically relocate to the same Dwelling.
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Buy_Home, foundStructure.tiles.First().tileObjectComponent.genericTileObject);
                producedJob = null;
                return true;
            } else if (want is FoodWant) {
#if DEBUG_LOG
                log = $"{log}\n-Will purchase food at {foundStructure.name} and bring home.";
#endif
                //Purchase or Take Food from favored Food Producing Structure and then bring it back home.
                if (character.jobComponent.TryCreateStockpileFood(character, foundStructure, out producedJob)) {
                    return true;
                }
            } else if (want is FurnitureWant furnitureWant) {
#if DEBUG_LOG
                log = $"{log}\n-Will purchase supplies at {foundStructure.name} and build {furnitureWant.furnitureWanted.ToString()}.";
#endif
                //Purchase or Take Wood or Stone from favored Basic Resource Producing Structure and then build the Furniture.
                if (character.jobComponent.CreateCraftFurniture(furnitureWant.furnitureWanted, character.homeStructure, foundStructure, out producedJob)) {
                    return true;
                }
            } else if (want is EquipmentWant equipmentWant) {
                Workshop workshop = foundStructure as Workshop;
                Assert.IsNotNull(workshop);
                CharacterClassData characterClassData = CharacterManager.Instance.GetOrCreateCharacterClassData(character.characterClass.className);
                List<TILE_OBJECT_TYPE> wantedEquipment = null;
                if (equipmentWant is WeaponWant) {
                    wantedEquipment = characterClassData.craftableWeapons;
                } else if (equipmentWant is ArmorWant) {
                    wantedEquipment = characterClassData.craftableArmors;
                } else if (equipmentWant is AccessoryWant) {
                    wantedEquipment = characterClassData.craftableAccessories;
                }
                if (wantedEquipment != null) {
                    //If wanted Equipment is available at any favored Workshop, purchase and then equip it
                    List<TileObject> availableEquipment = RuinarchListPool<TileObject>.Claim();
                    for (int i = 0; i < wantedEquipment.Count; i++) {
                        TILE_OBJECT_TYPE equipmentType = wantedEquipment[i];
                        List<TileObject> foundObjects = foundStructure.GetTileObjectsOfType(equipmentType);
                        if (foundObjects != null) {
                            for (int j = 0; j < foundObjects.Count; j++) {
                                TileObject equipment = foundObjects[j];
                                if (equipment.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                                    availableEquipment.Add(equipment);
                                }
                            }    
                        }
                    }
                    if (availableEquipment.Count > 0) {
                        TileObject targetEquipment = CollectionUtilities.GetRandomElement(availableEquipment);
                        //Purchase equipment
                        if (character.jobComponent.TryCreateBuyItem(character, targetEquipment, out producedJob)) {
                            return true;
                        }    
                    } else {
                        if (!workshop.IsCharacterAlreadyHasRequest(character)) {
                            //request equipment
                            WorkShopRequestForm workShopRequestForm = new WorkShopRequestForm();
                            workShopRequestForm.requestingCharacter = character;
                            if (equipmentWant is WeaponWant) {
                                workShopRequestForm.equipmentType = EQUIPMENT_TYPE.WEAPON;
                            } else if (equipmentWant is ArmorWant) {
                                workShopRequestForm.equipmentType = EQUIPMENT_TYPE.ARMOR;
                            } else if (equipmentWant is AccessoryWant) {
                                workShopRequestForm.equipmentType = EQUIPMENT_TYPE.ACCESSORY;
                            }
                            workshop.PostRequest(workShopRequestForm);    
                        }
                    }
                }
            } else if (want is HealingPotionWant) {
                //If wanted Item is available at any favored Hospice, purchase and then equip it.
                TileObject targetPotion = null;
                List<TileObject> potions = RuinarchListPool<TileObject>.Claim();
                foundStructure.PopulateTileObjectsOfType(potions, TILE_OBJECT_TYPE.HEALING_POTION);
                List<TileObject> validPotions = RuinarchListPool<TileObject>.Claim();
                for (int i = 0; i < potions.Count; i++) {
                    TileObject potion = potions[i];
                    if (potion.gridTileLocation != null && !potion.HasJobTargetingThis(JOB_TYPE.BUY_ITEM)) {
                        validPotions.Add(potion);
                    }
                }
                if (validPotions.Count > 0) { targetPotion = CollectionUtilities.GetRandomElement(validPotions); }
                RuinarchListPool<TileObject>.Release(potions);
                RuinarchListPool<TileObject>.Release(validPotions);
                if (targetPotion != null) {
#if DEBUG_LOG
                    log = $"{log}\n-Will purchase healing potion: {targetPotion.nameWithID} at {foundStructure.name}.";
#endif
                    if (character.jobComponent.TryCreateBuyItem(character, targetPotion, out producedJob)) {
                        return true;
                    }    
                } else {
#if DEBUG_LOG
                    log = $"{log}\n-Could not find valid healing potion at {foundStructure.name}.";
#endif
                }
            }
        }
        
#if DEBUG_LOG
        log = $"{log}\n-{character.name} will run through idle behaviour.";
#endif
        //Idle Behaviour
        if (!HasHomeStructureOrTerritory(character)) {
            return HomelessBehaviour(character, ref log, out producedJob);
        } else if (character.isAtHomeStructure || character.IsInTerritory()) {
            return AtHomeBehaviour(character, ref log, out producedJob);
        } else if (character.currentStructure is Tavern) {
            return TavernBehaviour(character, ref log, out producedJob);
        } else if (character.currentStructure is Hospice) {
            return HospiceBehaviour(character, ref log, out producedJob);
        } else {
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        }
    }
    private bool AtHomeBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.previousCharacterDataComponent.IsPreviousJobOrActionReturnHome()) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is in home structure and just returned home";
#endif
            if ((character.characterClass.IsCombatant() || character.characterClass.className == "Noble") && !character.traitContainer.HasTrait("Enslaved")) {
#if DEBUG_LOG
                log = $"{log}\n-{character.name} is a Combatant that is not Enslaved";
#endif
                if (ChanceData.RollChance(CHANCE_TYPE.Personal_Combatant_Change_Class, ref log) && CanCharacterChangeToAHigherTierCombatClass(character)) {
#if DEBUG_LOG
                    log = $"{log}\n-{character.name} can upgrade to a higher tier.";
#endif
                    List<string> classChoices = RuinarchListPool<string>.Claim();
                    if (character.characterClass.attackType == ATTACK_TYPE.MAGICAL && character.structureComponent.HasWorkPlaceStructure()) {
                        //added this case for magic users assigned to the Hospice, so that they can still be assigned to the hospice after upgrading their class
                        TryAddClassToChangeClassChoices(character, "Mage", classChoices);
                        TryAddClassToChangeClassChoices(character, "Shaman", classChoices);
                    } else {
                        TryAddClassToChangeClassChoices(character, "Knight", classChoices);
                        TryAddClassToChangeClassChoices(character, "Hunter", classChoices);
                        TryAddClassToChangeClassChoices(character, "Mage", classChoices);
                        if (classChoices.Count <= 0) {
                            TryAddClassToChangeClassChoices(character, "Barbarian", classChoices);
                            TryAddClassToChangeClassChoices(character, "Stalker", classChoices);
                            TryAddClassToChangeClassChoices(character, "Shaman", classChoices);    
                        }    
                    }

                    if (classChoices.Count > 0) {
#if DEBUG_LOG
                        log = $"{log}\n-{character.name} change class choices are {classChoices.ComafyList()}";
#endif
                        string classToChangeTo = CollectionUtilities.GetRandomElement(classChoices);
#if DEBUG_LOG
                        log = $"{log}\n-{character.name} chosen Change Class target is {classToChangeTo}";
#endif
                        if (character.jobComponent.TriggerPersonalChangeClassJob(classToChangeTo, out producedJob)) {
#if DEBUG_LOG
                            log = $"{log}\n-Personal Change Class was created.";
#endif
                            return true;
                        }
                    } else {
#if DEBUG_LOG
                        log = $"{log}\n-{character.name} has no higher tier classes that it can change to";
#endif
                    }
                }
            }

            TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
#if DEBUG_LOG
            log = $"{log}\n-Sit if there is still an unoccupied Table or Desk in the current location";
#endif
            if (deskOrTable != null) {
#if DEBUG_LOG
                log = $"{log}\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
            }
            else {
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, stand idle";
                log = $"{log}\n  -{character.name} will do action Stand";
#endif
                character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
            }
            return true;
        } else {
            
#if DEBUG_LOG
            log = $"{log}\n-{character.name} is in home structure and previous action is not returned home";
#endif
            if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time &&
                character.dailyScheduleComponent.schedule.IsInFirstHourOfCurrentScheduleType(GameManager.Instance.currentTick) && !character.traitContainer.HasTrait("Agoraphobic")) {
            
                if (ChanceData.RollChance(CHANCE_TYPE.Socialize_Chance, ref log) && !character.behaviourComponent.HasBehaviour(typeof(SocializingBehaviour)) && 
                    character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
                    LocationStructure targetStructure;
                    if (character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) {
                        targetStructure = GameUtilities.RollChance(50) ? character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : character.homeSettlement.cityCenter;
                    } else {
                        targetStructure = character.homeSettlement.cityCenter;
                    }
                    character.behaviourComponent.GoSocializing(character, targetStructure);
                    producedJob = null;
                    return true;
                }
                
                if (ChanceData.RollChance(CHANCE_TYPE.Visit_Village_Chance, ref log) && !character.behaviourComponent.HasBehaviour(typeof(VisitVillageBehaviour))) {
                    List<NPCSettlement> villageChoices = RuinarchListPool<NPCSettlement>.Claim();
                    character.currentRegion.PopulateVillagesInRegionThatAreOwnedByFactionOrNotHostileToIt(villageChoices, character.faction);
                    if (character.homeSettlement != null) {
                        villageChoices.Remove(character.homeSettlement);    
                    }
                    if (villageChoices.Count > 0) {
                        NPCSettlement targetVillage = CollectionUtilities.GetRandomElement(villageChoices);
                        character.behaviourComponent.VisitVillage(character, targetVillage);
                        RuinarchListPool<NPCSettlement>.Release(villageChoices);
                        producedJob = null;
                        return true;    
                    }
                    RuinarchListPool<NPCSettlement>.Release(villageChoices);
                }
            }
            
            TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
            string strCurrentTimeOfDay = currentTimeOfDay.ToString();

// #if DEBUG_LOG
//             log = $"{log}\n-If it is Early Night, 5% chance to Host Social Party at Inn";
// #endif
//             if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea() &&
//                 character.currentSettlement != null && character.currentSettlement.HasStructure(STRUCTURE_TYPE.TAVERN) && !character.traitContainer.HasTrait("Agoraphobic")) {
// #if DEBUG_LOG
//                 log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
// #endif
//                 if (ChanceData.RollChance(CHANCE_TYPE.Host_Social_Party, ref log)) {
//                     LocationStructure structure = character.homeSettlement.GetFirstStructureOfTypeWithNoActiveSocialParty(STRUCTURE_TYPE.TAVERN);
//                     if (structure != null) {
// #if DEBUG_LOG
//                         log = $"{log}\n  -Early Night: {character.name} host a social party at Inn";
// #endif
//                         if (character.jobComponent.TriggerHostSocialPartyJob(out producedJob)) {
//                             return true;
//                         }
//                     }
// #if DEBUG_LOG
//                     log = $"{log}\n  -No Inn Structure in the npcSettlement";
// #endif
//                 }
// #if DEBUG_LOG
//                 log = $"{log}\n  -Did not host social party. Will roll to go to tavern";
// #endif
//                 if (GameUtilities.RollChance(10, ref log)) {
//                     LocationStructure tavern = character.currentSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
//                     //Go to tavern
//                     character.PlanFixedJob(JOB_TYPE.IDLE_GO_TO_INN, INTERACTION_TYPE.VISIT, character, out producedJob,
//                         new OtherData[] {new LocationStructureOtherData(tavern)});
//                     return true;
//                 }
//             }

            
            if (GameUtilities.RollChance(4, ref log)) {
                TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                if (bed != null) {
                    if (character.traitContainer.HasTrait("Vampire")) {
#if DEBUG_LOG
                        log = $"{log}\n  -Character is vampiric, cannot do nap action";
#endif
                    }
                    else {
#if DEBUG_LOG
                        log = $"{log}\n  -Afternoon: {character.name} will do action Nap on {bed}";
#endif
                        character.PlanFixedJob(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, bed, out producedJob);
                        return true;
                    }
                }
                else {
#if DEBUG_LOG
                    log = $"{log}\n  -No unoccupied bed in the current structure";
#endif
                }
            }


            if (GameUtilities.RollChance(30, ref log) && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea()) {
                Character chosenCharacter = GetDisabledCharacterToVisit(character);
                if (chosenCharacter != null) {
                    if (chosenCharacter.homeStructure != null) {
#if DEBUG_LOG
                        log = $"{log}\n  -Will visit house of Disabled Character {chosenCharacter.name}";
#endif
                        character.PlanFixedJob(JOB_TYPE.CHECK_PARALYZED_FRIEND, INTERACTION_TYPE.VISIT, character, out producedJob,
                            new OtherData[] {new LocationStructureOtherData(chosenCharacter.homeStructure), new CharacterOtherData(chosenCharacter),});
                        return true;
                    }
                }
#if DEBUG_LOG
                log = $"{log}\n  -No available character to visit ";
#endif
            }

            if (character.currentSettlement != null && character.currentSettlement.HasStructureClaimedByNonEnemyOrSelf(STRUCTURE_TYPE.HOSPICE, character, out LocationStructure foundStructure)) {
#if DEBUG_LOG
                log = $"{log}\n  -There is a Hospice in the Village claimed by a non-Enemy or by self: ";
#endif
                Hospice hospice = foundStructure as Hospice;
                Assert.IsNotNull(hospice);
                if ((character.traitContainer.HasTrait("Injured") || character.traitContainer.HasTrait("Plagued")) && 
                    !character.traitContainer.HasTrait("Plague Reservoir") && ChanceData.RollChance(CHANCE_TYPE.Plauged_Injured_Visit_Hospice)) {
                    //recuperate
#if DEBUG_LOG
                    log = $"{log}\n  -Actor has Injured or Plagued and there is still an available Bed in the Hospice: Create Recuperate Job";
#endif
                    
                    BedClinic bedClinic = hospice.GetFirstBedToRecuperate();
                    if (bedClinic != null) {
                        if (character.jobComponent.TryRecuperate(bedClinic, out producedJob)) {
                            return true;
                        }
                    }
                }
                if (ChanceData.RollChance(CHANCE_TYPE.Vampire_Lycan_Visit_Hospice, ref log) && character.currentStructure != foundStructure && hospice.HasWorkerWithLevel5HealingMagic()) {
#if DEBUG_LOG
                    log = $"{log}\n  -Hospice is claimed by a Villager with Level 5 Healing Magic:";
#endif
                    Traits.Vampire vampire = character.traitContainer.GetTraitOrStatus<Traits.Vampire>("Vampire");
                    if (vampire != null && vampire.dislikedBeingVampire) {
                        //Go to hospice and wait there for 2 hours
#if DEBUG_LOG
                        log = $"{log}\n  -Actor has Vampirism and disliked being a Vampire";
                        character.PlanFixedJob(JOB_TYPE.VISIT_HOSPICE, INTERACTION_TYPE.VISIT, character, out producedJob,
                            new OtherData[] {new LocationStructureOtherData(hospice)});
                        return true;
#endif
                    }
                    if (character.lycanData != null && character.lycanData.dislikesBeingLycan) {
                        //Go to hospice and wait there for 2 hours
#if DEBUG_LOG
                        log = $"{log}\n  -If Actor has Lycanthropy and disliked being a Werewolf";
                        character.PlanFixedJob(JOB_TYPE.VISIT_HOSPICE, INTERACTION_TYPE.VISIT, character, out producedJob,
                            new OtherData[] {new LocationStructureOtherData(hospice)});
                        return true;
#endif
                    }
                }
            }

#if DEBUG_LOG
            log = $"{log}\n-Otherwise, if character has at least one item in his inventory (exclude healing potions)";
#endif
            if (character.HasItemOtherThan(TILE_OBJECT_TYPE.HEALING_POTION) && character.homeStructure != null) {
                if (GameUtilities.RollChance(10)) {
#if DEBUG_LOG
                    log = $"{log}\n  -Will create Drop Item job";
#endif
                    if (character.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM, character.GetRandomItemThatIsNotOfType(TILE_OBJECT_TYPE.HEALING_POTION), character.homeStructure, out producedJob)) {
                        return true;
                    }
                }
            }
            else {
#if DEBUG_LOG
                log = $"{log}\n  -Has no non-healing potion item in inventory";
#endif
            }

#if DEBUG_LOG
            log = $"{log}\n-Otherwise, if character is Lazy";
#endif
            if (character.traitContainer.HasTrait("Lazy") && GameUtilities.RollChance(4)) {
                CreateCleanJob(character, ref log, out producedJob);
            } else {
                CreateCleanJob(character, ref log, out producedJob);
            }
            if(producedJob != null) {
                return true;
			}
            if (character.HasItemOtherThan(TILE_OBJECT_TYPE.HEALING_POTION) && character.homeStructure != null) {
                if (GameUtilities.RollChance(10)) {
#if DEBUG_LOG
                    log = $"{log}\n  -Will create Drop Item job";
#endif
                    if (character.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM, character.GetRandomItemThatIsNotOfType(TILE_OBJECT_TYPE.HEALING_POTION), character.homeStructure, out producedJob)) {
                        return true;
                    }
                }
            } else {
#if DEBUG_LOG
                log = $"{log}\n  -Has no non-healing potion item in inventory";
#endif
            }

#if DEBUG_LOG
            log = $"{log}\n-Otherwise, sit if there is still an unoccupied Table or Desk";
#endif
            TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
            if (deskOrTable != null) {
#if DEBUG_LOG
                log = $"{log}\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                return true;
            }
#if DEBUG_LOG
            log = $"{log}\n  -No unoccupied Table or Desk";

            log = $"{log}\n-Otherwise, stand idle";
            log = $"{log}\n  -{character.name} will do action Stand";
#endif
            character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
            return true;
        }
    }
    private bool HomelessBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log = $"{log}\n-{character.name} is homeless";
#endif
        if (character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
#if DEBUG_LOG
            log = $"{log}\n-{character.name} lives in a village: {character.homeSettlement.name}";
#endif
            if (character.currentStructure != null && character.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && character.currentStructure.structureType != STRUCTURE_TYPE.TAVERN) {
#if DEBUG_LOG
                log = $"{log}\n-{character.name} is not in city center or tavern. Rolling to go to city center or tavern...";
#endif
                if (GameUtilities.RollChance(25, ref log)) {
                    if (character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN) && !character.traitContainer.HasTrait("Agoraphobic")) {
#if DEBUG_LOG
                        log = $"{log}\n-Home village has tavern and character is not Agoraphobic";
#endif
                        LocationStructure targetStructure = GameUtilities.RollChance(50) ? character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : character.homeSettlement.cityCenter;
#if DEBUG_LOG
                        log = $"{log}\n-Target Structure is {targetStructure.name}";
#endif
                        if (targetStructure.passableTiles.Count > 0) {
                            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                            return character.jobComponent.CreateGoToJob(JOB_TYPE.IDLE, targetTile, out producedJob);  
                        }
                    } else {
#if DEBUG_LOG
                        log = $"{log}\n-Home village doesn't have a tavern or character is Agoraphobic. Will Go to City Center";
#endif
                        LocationStructure targetStructure = character.homeSettlement.cityCenter;
                        if (targetStructure.passableTiles.Count > 0) {
                            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                            return character.jobComponent.CreateGoToJob(JOB_TYPE.IDLE, targetTile, out producedJob);  
                        }
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n-Chance not met. Will do action Stand";
#endif
                    character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);                
                }
            } else {
#if DEBUG_LOG
                log = $"{log}\n-Character is at city center or tavern";
#endif
                //socializing and visit village
                 if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time &&
                    character.dailyScheduleComponent.schedule.IsInFirstHourOfCurrentScheduleType(GameManager.Instance.currentTick) && !character.traitContainer.HasTrait("Agoraphobic")) {
#if DEBUG_LOG
                     log = $"{log}\n-Is in first hour of free time. Will roll chance to socialize";
#endif
                    if (ChanceData.RollChance(CHANCE_TYPE.Socialize_Chance, ref log) && !character.behaviourComponent.HasBehaviour(typeof(SocializingBehaviour)) && 
                        character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) {
                        LocationStructure targetStructure;
                        if (character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) {
                            targetStructure = GameUtilities.RollChance(50) ? character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : character.homeSettlement.cityCenter;
                        } else {
                            targetStructure = character.homeSettlement.cityCenter;
                        }
#if DEBUG_LOG
                        log = $"{log}\n-Will socialize at {targetStructure.name}";
#endif
                        character.behaviourComponent.GoSocializing(character, targetStructure);
                        producedJob = null;
                        return true;
                    }
#if DEBUG_LOG
                    log = $"{log}\n-Will roll for chance to Visit Village...";
#endif
                    if (ChanceData.RollChance(CHANCE_TYPE.Visit_Village_Chance, ref log) && !character.behaviourComponent.HasBehaviour(typeof(VisitVillageBehaviour))) {
                        List<NPCSettlement> villageChoices = RuinarchListPool<NPCSettlement>.Claim();
                        character.currentRegion.PopulateVillagesInRegionThatAreOwnedByFactionOrNotHostileToIt(villageChoices, character.faction);
                        if (character.homeSettlement != null) {
                            villageChoices.Remove(character.homeSettlement);    
                        }
                        if (villageChoices.Count > 0) {
                            NPCSettlement targetVillage = CollectionUtilities.GetRandomElement(villageChoices);
#if DEBUG_LOG
                            log = $"{log}\n-Will visit {targetVillage.name}";
#endif
                            character.behaviourComponent.VisitVillage(character, targetVillage);
                            RuinarchListPool<NPCSettlement>.Release(villageChoices);
                            producedJob = null;
                            return true;    
                        }
                        RuinarchListPool<NPCSettlement>.Release(villageChoices);
                    } 
                 }
                 
                 //combatant change class
                 if ((character.characterClass.IsCombatant() || character.characterClass.className == "Noble") && !character.traitContainer.HasTrait("Enslaved")) {
#if DEBUG_LOG
                    log = $"{log}\n-{character.name} is a Combatant that is not Enslaved";
#endif
                    if (ChanceData.RollChance(CHANCE_TYPE.Personal_Combatant_Change_Class, ref log) && CanCharacterChangeToAHigherTierCombatClass(character)) {
#if DEBUG_LOG
                        log = $"{log}\n-{character.name} can upgrade to a higher tier.";
#endif
                        List<string> classChoices = RuinarchListPool<string>.Claim();
                        if (character.characterClass.attackType == ATTACK_TYPE.MAGICAL && character.structureComponent.HasWorkPlaceStructure()) {
                            //added this case for magic users assigned to the Hospice, so that they can still be assigned to the hospice after upgrading their class
                            TryAddClassToChangeClassChoices(character, "Mage", classChoices);
                            TryAddClassToChangeClassChoices(character, "Shaman", classChoices);
                        } else {
                            TryAddClassToChangeClassChoices(character, "Knight", classChoices);
                            TryAddClassToChangeClassChoices(character, "Hunter", classChoices);
                            TryAddClassToChangeClassChoices(character, "Mage", classChoices);
                            if (classChoices.Count <= 0) {
                                TryAddClassToChangeClassChoices(character, "Barbarian", classChoices);
                                TryAddClassToChangeClassChoices(character, "Stalker", classChoices);
                                TryAddClassToChangeClassChoices(character, "Shaman", classChoices);    
                            }    
                        }

                        if (classChoices.Count > 0) {
#if DEBUG_LOG
                            log = $"{log}\n-{character.name} change class choices are {classChoices.ComafyList()}";
#endif
                            string classToChangeTo = CollectionUtilities.GetRandomElement(classChoices);
#if DEBUG_LOG
                            log = $"{log}\n-{character.name} chosen Change Class target is {classToChangeTo}";
#endif
                            if (character.jobComponent.TriggerPersonalChangeClassJob(classToChangeTo, out producedJob)) {
#if DEBUG_LOG
                                log = $"{log}\n-Personal Change Class was created.";
#endif
                                return true;
                            }
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n-{character.name} has no higher tier classes that it can change to";
#endif
                        }
                    }
                }
                 
                 //visit disabled friend
                 if (GameUtilities.RollChance(30, ref log) && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea()) {
                     Character chosenCharacter = GetDisabledCharacterToVisit(character);
                     if (chosenCharacter != null) {
                         if (chosenCharacter.homeStructure != null) {
#if DEBUG_LOG
                             log = $"{log}\n  -Will visit house of Disabled Character {chosenCharacter.name}";
#endif
                             character.PlanFixedJob(JOB_TYPE.CHECK_PARALYZED_FRIEND, INTERACTION_TYPE.VISIT, character, out producedJob,
                                 new OtherData[] {new LocationStructureOtherData(chosenCharacter.homeStructure), new CharacterOtherData(chosenCharacter),});
                             return true;
                         }
                     }
#if DEBUG_LOG
                     log = $"{log}\n  -No available character to visit ";
#endif
                 }
                 
                 //visit Hospice
                 if (character.currentSettlement != null && character.currentSettlement.HasStructureClaimedByNonEnemyOrSelf(STRUCTURE_TYPE.HOSPICE, character, out LocationStructure foundStructure)) {
#if DEBUG_LOG
                    log = $"{log}\n  -There is a Hospice in the Village claimed by a non-Enemy or by self: ";
#endif
                    Hospice hospice = foundStructure as Hospice;
                    Assert.IsNotNull(hospice);
                    if ((character.traitContainer.HasTrait("Injured") || character.traitContainer.HasTrait("Plagued")) && 
                        !character.traitContainer.HasTrait("Plague Reservoir") && ChanceData.RollChance(CHANCE_TYPE.Plauged_Injured_Visit_Hospice)) {
                        //recuperate
#if DEBUG_LOG
                        log = $"{log}\n  -Actor has Injured or Plagued and there is still an available Bed in the Hospice: Create Recuperate Job";
#endif
                        
                        BedClinic bedClinic = hospice.GetFirstBedToRecuperate();
                        if (bedClinic != null) {
                            if (character.jobComponent.TryRecuperate(bedClinic, out producedJob)) {
                                return true;
                            }
                        }
                    }
                    if (ChanceData.RollChance(CHANCE_TYPE.Vampire_Lycan_Visit_Hospice, ref log) && character.currentStructure != foundStructure && hospice.HasWorkerWithLevel5HealingMagic()) {
#if DEBUG_LOG
                        log = $"{log}\n  -Hospice is claimed by a Villager with Level 5 Healing Magic:";
#endif
                        Traits.Vampire vampire = character.traitContainer.GetTraitOrStatus<Traits.Vampire>("Vampire");
                        if (vampire != null && vampire.dislikedBeingVampire) {
                            //Go to hospice and wait there for 2 hours
#if DEBUG_LOG
                            log = $"{log}\n  -Actor has Vampirism and disliked being a Vampire";
                            character.PlanFixedJob(JOB_TYPE.VISIT_HOSPICE, INTERACTION_TYPE.VISIT, character, out producedJob,
                                new OtherData[] {new LocationStructureOtherData(hospice)});
                            return true;
#endif
                        }
                        if (character.lycanData != null && character.lycanData.dislikesBeingLycan) {
                            //Go to hospice and wait there for 2 hours
#if DEBUG_LOG
                            log = $"{log}\n  -If Actor has Lycanthropy and disliked being a Werewolf";
                            character.PlanFixedJob(JOB_TYPE.VISIT_HOSPICE, INTERACTION_TYPE.VISIT, character, out producedJob,
                                new OtherData[] {new LocationStructureOtherData(hospice)});
                            return true;
#endif
                        }
                    }
                }
            } 
        }
        
#if DEBUG_LOG
        log = $"{log}\n-No home structure";
        log = $"{log}\n-Will do action Stand";
#endif
        character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
        return true;
    }
    private bool TavernBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
        string strCurrentTimeOfDay = currentTimeOfDay.ToString();

#if DEBUG_LOG
        log = $"{log}\n-Current time of day is {strCurrentTimeOfDay}";
#endif
        if (currentTimeOfDay != TIME_IN_WORDS.EARLY_NIGHT) {
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        } else {
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        }
    }
    private bool HospiceBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log = $"{log}\n-{character.name} is at Hospice";
#endif
        if ((character.traitContainer.HasTrait("Injured") || character.traitContainer.HasTrait("Poisoned") || character.traitContainer.HasTrait("Plagued")) && 
            !character.traitContainer.HasTrait("Plague Reservoir")) {
            Hospice hospice = character.currentStructure as Hospice;
            BedClinic bedClinic = hospice.GetFirstBedToRecuperate();
            if (bedClinic != null) {
                if (character.jobComponent.TryRecuperate(bedClinic, out producedJob)) {
                    return true;
                }
            }
        }
        if (!character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea()) {
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);    
        } else {
            //character is trapped in hospice
            return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        }
    }

    #region Utilities
    private bool HasHomeStructureOrTerritory(Character character) {
        return (character.homeStructure != null && !character.homeStructure.hasBeenDestroyed) || character.HasTerritory();
    }
    private Character GetDisabledCharacterToVisit(Character p_character) {
        //List<Character> charactersWithRel = relationshipContainer.relationships.Keys.Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList();
        Character chosenCharacter = null;
        List<Character> charactersWithRel = p_character.relationshipContainer.charactersWithOpinion;
        if (charactersWithRel.Count > 0) {
            List<Character> positiveCharacters = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < charactersWithRel.Count; i++) {
                Character character = charactersWithRel[i];
                if (p_character.homeSettlement != character.homeSettlement) {
                    continue;
                }
                if (p_character.homeStructure == character.homeStructure) {
                    continue;
                }
                if (!character.traitContainer.HasTrait("Paralyzed", "Catatonic")) {
                    continue;
                }
                if(character.isDead || p_character.relationshipContainer.GetAwarenessState(character) == AWARENESS_STATE.Missing || p_character.homeStructure == character.homeStructure) {
                    continue;
                }
                if (p_character.relationshipContainer.IsFriendsWith(character)) {
                    positiveCharacters.Add(character);
                }
            }
            if (positiveCharacters.Count > 0) {
                chosenCharacter = CollectionUtilities.GetRandomElement(positiveCharacters);
            }
            RuinarchListPool<Character>.Release(positiveCharacters);
        }
        return chosenCharacter;
    }
    private bool CreateHappinessRecoveryJob(Character p_character, out JobQueueItem producedJob) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, 
            new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), p_character, p_character);
        JobUtilities.PopulatePriorityLocationsForHappinessRecovery(p_character, job);
        job.SetDoNotRecalculate(true);
        producedJob = job;
        return true;
    }
    private void CreateCleanJob(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log = $"{log}\n-Otherwise, Not Lazy look for items to clean(wet and Dry)";
#endif
        List<TileObject> allObjectsInsideCurrentStructure = RuinarchListPool<TileObject>.Claim();
        character.currentStructure.PopulateTileObjectsListWithAllTileObjects(allObjectsInsideCurrentStructure);
        for (int x = 0; x < allObjectsInsideCurrentStructure.Count; ++x) {
            if (allObjectsInsideCurrentStructure[x].traitContainer.HasTrait("Dirty") || allObjectsInsideCurrentStructure[x].traitContainer.HasTrait("Wet")) {
                if (!allObjectsInsideCurrentStructure[x].HasJobTargetingThis(JOB_TYPE.IDLE_CLEAN)) {
                    character.jobComponent.TryCreateCleanItemJob(allObjectsInsideCurrentStructure[x], out producedJob);
                    if(producedJob != null) {
#if DEBUG_LOG
                        log = $"{log}\n  -Will create Clean Up Item";
#endif
                        break;
                    }
                }
            }
        }
    }
    private bool CanCharacterChangeToAHigherTierCombatClass(Character p_character) {
        //characters can upgrade its current class if its current class is not part of the high tier list
        return !highTierClasses.Contains(p_character.characterClass.className);
    }
    private void TryAddClassToChangeClassChoices(Character p_character, string p_className, List<string> p_classChoices) {
        if (p_character.classComponent.ableClasses.Contains(p_className)) {
            p_classChoices.Add(p_className);
        }
    }
    private void FreeTimePartyLogic(Character character, ref string log) {
        if ((character.characterClass.IsCombatant() || character.characterClass.className == "Noble") && !character.traitContainer.HasTrait("Enslaved") && character.faction != null) {
            if (!character.partyComponent.hasParty && character.homeSettlement != null && !character.structureComponent.HasWorkPlaceStructure() && !character.crimeComponent.IsWantedBy(character.faction)) {
#if DEBUG_LOG
                log = $"{log}\n-{character.name} is not yet part of a party. Will try to join or create one.";
#endif
                bool shouldCreateOrJoinParty = true;
                if (character.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA)) {
                    shouldCreateOrJoinParty = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA).currentLevel >= 3;
                }
                Party unFullParty = character.homeSettlement.GetFirstUnfullParty();
                if (unFullParty == null) {
#if DEBUG_LOG
                    log = $"{log}\n-No un-full party. Will try to create party, rolling chance...";
#endif
                    if (GameUtilities.RollChance(10, ref log) && shouldCreateOrJoinParty) {
#if DEBUG_LOG
                        log = $"{log}\n-Chance met, will create party.";
#endif
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Party, character);
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n-Found an un-full party: {unFullParty.name}. Rolling chance to join...";
#endif
                    if (GameUtilities.RollChance(15, ref log) && shouldCreateOrJoinParty) {
#if DEBUG_LOG
                        log = $"{log}\n-Will join party";
#endif
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Party, unFullParty.members[0]);
                    }
                }    
            }
        }
    }
    #endregion

    
}
