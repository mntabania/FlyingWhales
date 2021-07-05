using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using JetBrains.Annotations;
using System.Linq;

public class SettlementClassComponent : NPCSettlementComponent {
    private static readonly string[] _characterClassOrder = new[] { "Civilian", "Combatant", "Civilian", "Combatant", "Combatant", "Noble", "Combatant" };

    private readonly List<string> _currentResidentClasses;

    private int _currentClassOrderIndex;
    private bool m_bypass;

    public GameDate morningScheduleDateForProcessingOfNeededClasses { get; private set; }
    public GameDate afternoonScheduleDateForProcessingOfNeededClasses { get; private set; }

    #region getters
    public int currentClassOrderIndex => _currentClassOrderIndex;
    public List<string> currentResidentClasses => _currentResidentClasses;
    #endregion

    public SettlementClassComponent() {
        _currentClassOrderIndex = 0;
        _currentResidentClasses = new List<string>();
    }
    public SettlementClassComponent(SaveDataSettlementClassComponent data) {
        _currentClassOrderIndex = data.currentClassOrderIndex;
        _currentResidentClasses = data.currentResidentClasses;
        morningScheduleDateForProcessingOfNeededClasses = data.morningScheduleDateForProcessingOfNeededClasses;
        afternoonScheduleDateForProcessingOfNeededClasses = data.afternoonScheduleDateForProcessingOfNeededClasses;
    }

    #region Character Class Order
    public string GetNextClassToCreateAndIncrementOrder([NotNull] Faction p_faction) {
        string classToCreate = GetCurrentClassInClassOrder(p_faction);
        _currentClassOrderIndex = currentClassOrderIndex + 1;
        if (currentClassOrderIndex == _characterClassOrder.Length) {
            _currentClassOrderIndex = 0;
        }
        return classToCreate;
    }
    private string GetCurrentClassInClassOrder([NotNull] Faction p_faction) {
        string currentClass = _characterClassOrder[currentClassOrderIndex];
        if (currentClass == "Combatant") {
            currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(p_faction.factionType.combatantClasses);
        } else if (currentClass == "Civilian") {
            currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(p_faction.factionType.civilianClasses);
        }
        return currentClass;
    }
    #endregion

    #region Current Classes
    public void OnResidentAdded(Character p_newResident) {
        currentResidentClasses.Add(p_newResident.characterClass.className);
    }
    public void OnResidentRemoved(Character p_newResident) {
        currentResidentClasses.Remove(p_newResident.characterClass.className);
    }
    public void OnResidentChangedClass(string p_previousClass, Character p_character) {
        currentResidentClasses.Remove(p_previousClass);
        currentResidentClasses.Add(p_character.characterClass.className);
    }
    public int GetCurrentResidentClassAmount(string p_className) {
        int classCount = 0;
        for (int i = 0; i < _currentResidentClasses.Count; i++) {
            if (currentResidentClasses[i] == p_className) {
                classCount++;
            }
        }
        return classCount;
    }
    #endregion

    #region Needed Classes
    public void InitialMorningScheduleProcessingOfNeededClasses() {
        if (owner.locationType == LOCATION_TYPE.VILLAGE) {
            int minimumTick = GameManager.Instance.GetTicksBasedOnHour(2); //2 AM in ticks
            int maximumTick = GameManager.Instance.GetTicksBasedOnHour(5); //5 AM in ticks

            int scheduledTick = GameUtilities.RandomBetweenTwoNumbers(minimumTick, maximumTick);
            GameDate schedule = GameManager.Instance.Today().AddDays(1);
            schedule.SetTicks(scheduledTick);
            morningScheduleDateForProcessingOfNeededClasses = schedule;
            SchedulingManager.Instance.AddEntry(morningScheduleDateForProcessingOfNeededClasses, MorningProcessingOfNeededClasses, null);
        }
    }
    public void InitialAfternoonScheduleProcessingOfNeededClasses() {
        if (owner.locationType == LOCATION_TYPE.VILLAGE) {
            int minimumTick = GameManager.Instance.GetTicksBasedOnHour(12); //12 PM in ticks
            int maximumTick = GameManager.Instance.GetTicksBasedOnHour(15); //3 PM in ticks

            int scheduledTick = GameUtilities.RandomBetweenTwoNumbers(minimumTick, maximumTick);
            GameDate schedule = GameManager.Instance.Today();
            schedule.SetTicks(scheduledTick);
            afternoonScheduleDateForProcessingOfNeededClasses = schedule;
            SchedulingManager.Instance.AddEntry(afternoonScheduleDateForProcessingOfNeededClasses, AfternoonProcessingOfNeededClasses, null);
        }
    }
    private void MorningProcessingOfNeededClasses() {
        if (owner.HasResidentThatIsNotDead()) {
            ProcessNeededClasses();
        }
        morningScheduleDateForProcessingOfNeededClasses = GameManager.Instance.Today().AddDays(1);
        SchedulingManager.Instance.AddEntry(morningScheduleDateForProcessingOfNeededClasses, MorningProcessingOfNeededClasses, null);
    }
    private void AfternoonProcessingOfNeededClasses() {
        if (owner.HasResidentThatIsNotDead()) {
            ProcessNeededClasses();
        }
        afternoonScheduleDateForProcessingOfNeededClasses = GameManager.Instance.Today().AddDays(1);
        SchedulingManager.Instance.AddEntry(afternoonScheduleDateForProcessingOfNeededClasses, AfternoonProcessingOfNeededClasses, null);
    }
    private void ProcessNeededClasses() {
        string log = string.Empty;
        m_bypass = false;
#if DEBUG_LOG
        log = GameManager.Instance.TodayLogString() + owner.name + " will process needed classes";
        log += "\nINFO:";
#endif
        owner.ForceCancelJobTypesImmediately(JOB_TYPE.CHANGE_CLASS);
        int numOfActiveResidents = owner.GetNumberOfResidentsThatIsAliveVillager();
        int foodSupplyCapacity = owner.resourcesComponent.GetFoodSupplyCapacity();
        int resourceSupplyCapacity = owner.resourcesComponent.GetResourceSupplyCapacity();
        int numOfCombatants = owner.GetNumOfResidentsThatIsAliveCombatant();
        int neededCombatants = GetNumberOfNeededCombatants(numOfActiveResidents);
        bool villagerCanBecomeFisher = false;
        bool villagerCanBecomeButcher = false;
        bool villagerCanBecomeSkinner = false;
        bool villagerCanBecomeMerchant = false;

        //Determine who should change classes and who should not change class
        //Put this here so that looping through all residents is only done once
        int reservedCombatantCount = 0;
        int numberOfAvailableVillagers = 0;
        List<Character> sortedFoodProducers = RuinarchListPool<Character>.Claim();
        List<Character> foodProducersNoValue = RuinarchListPool<Character>.Claim();
        List<int> sortedFoodProducersSupplyCapacity = RuinarchListPool<int>.Claim();
        List<Character> sortedResourceProducers = RuinarchListPool<Character>.Claim();
        List<Character> resourceProducersNoValue = RuinarchListPool<Character>.Claim();
        List<int> sortedResourceProducersSupplyCapacity = RuinarchListPool<int>.Claim();
        List<Character> sortedCombatants = RuinarchListPool<Character>.Claim();
        List<int> sortedCombatantValues = RuinarchListPool<int>.Claim();

        for (int i = 0; i < owner.residents.Count; i++) {
            Character c = owner.residents[i];
            if (!c.isDead) {
                if (!villagerCanBecomeFisher || !villagerCanBecomeButcher || !villagerCanBecomeSkinner || !villagerCanBecomeMerchant) {
                    bool isAvailable = !c.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && c.HasTalents();
                    if (isAvailable) {
                        if (!villagerCanBecomeFisher) {
                            if (c.classComponent.HasAbleClass("Fisher")) {
                                villagerCanBecomeFisher = true;
                            }
                        }
                        if (!villagerCanBecomeButcher) {
                            if (c.classComponent.HasAbleClass("Butcher")) {
                                villagerCanBecomeButcher = true;
                            }
                        }
                        if (!villagerCanBecomeSkinner) {
                            if (c.classComponent.HasAbleClass("Skinner")) {
                                villagerCanBecomeSkinner = true;
                            }
                        }
                        if (!villagerCanBecomeMerchant) {
                            if (c.classComponent.HasAbleClass("Merchant")) {
                                villagerCanBecomeMerchant = true;
                            }
                        }
                    }
                }
                
                if (c.characterClass.IsCombatant()) {
                    int combatValue = c.classComponent.GetCombatSupplyValue();
                    if (combatValue == 0) {
                        sortedCombatants.Add(c);
                        sortedCombatantValues.Add(combatValue);
                    } else {
                        bool hasInserted = false;
                        for (int j = 0; j < sortedCombatantValues.Count; j++) {
                            int cv = sortedCombatantValues[j];
                            if (combatValue > cv) {
                                sortedCombatants.Insert(j, c);
                                sortedCombatantValues.Insert(j, combatValue);
                                hasInserted = true;
                                break;
                            }
                        }
                        if (!hasInserted) {
                            sortedCombatants.Add(c);
                            sortedCombatantValues.Add(combatValue);
                        }
                    }
                } else {
                    if (c.characterClass.className.IsFoodProducerClassName()) {
                        //Add all food producer characters in order of their supply value from highest to lowest
                        int supply = c.classComponent.GetFoodSupplyCapacityValue();
                        if (supply == 0) {
                            //If supply is 0, it is automatically added at the bottom of the list
                            //sortedFoodProducers.Add(c);
                            //sortedFoodProducersSupplyCapacity.Add(supply);
                            foodProducersNoValue.Add(c);
                        } else {
                            bool hasInserted = false;
                            for (int j = 0; j < sortedFoodProducersSupplyCapacity.Count; j++) {
                                int fsp = sortedFoodProducersSupplyCapacity[j];
                                if (supply > fsp) {
                                    sortedFoodProducers.Insert(j, c);
                                    sortedFoodProducersSupplyCapacity.Insert(j, supply);
                                    hasInserted = true;
                                    break;
                                }
                            }
                            if (!hasInserted) {
                                sortedFoodProducers.Add(c);
                                sortedFoodProducersSupplyCapacity.Add(supply);
                            }
                        }
                    } else if (c.characterClass.className.IsResourceProducerClassName()) {
                        //Add all resource producer characters in order of their supply value from highest to lowest
                        int supply = c.classComponent.GetResourceSupplyCapacityValue(c.characterClass.className);
                        if (supply == 0) {
                            //If supply is 0, it is automatically added at the bottom of the list
                            //sortedResourceProducers.Add(c);
                            //sortedResourceProducersSupplyCapacity.Add(supply);
                            resourceProducersNoValue.Add(c);
                        } else {
                            bool hasInserted = false;
                            for (int j = 0; j < sortedResourceProducersSupplyCapacity.Count; j++) {
                                int rsp = sortedResourceProducersSupplyCapacity[j];
                                if (supply > rsp) {
                                    sortedResourceProducers.Insert(j, c);
                                    sortedResourceProducersSupplyCapacity.Insert(j, supply);
                                    hasInserted = true;
                                    break;
                                }
                            }
                            if (!hasInserted) {
                                sortedResourceProducers.Add(c);
                                sortedResourceProducersSupplyCapacity.Add(supply);
                            }
                        }
                    } else {
                        c.classComponent.SetShouldChangeClass(true);
                        numberOfAvailableVillagers++;
                    }
                    //if (c.structureComponent.HasWorkPlaceStructure()) {
                    //    if (c.characterClass.className.IsFoodProducerClassName() && numOfActiveResidents > foodSupplyCapacity) {
                    //        c.classComponent.SetShouldChangeClass(false);
                    //    } else if (c.characterClass.className.IsResourceProducerClassName() && numOfActiveResidents > resourceSupplyCapacity) {
                    //        c.classComponent.SetShouldChangeClass(false);
                    //    } else {
                    //        c.classComponent.SetShouldChangeClass(true);
                    //        numberOfAvailableVillagers++;
                    //    }
                    //} else {
                    //    c.classComponent.SetShouldChangeClass(true);
                    //    numberOfAvailableVillagers++;
                    //}
                }
            }
        }

        //Reserve Food/Resource Producers
#if DEBUG_LOG
        if (sortedFoodProducers.Count != sortedFoodProducersSupplyCapacity.Count) {
            Debug.LogError("Food producer list and food supply capacity list not the same length: " + sortedFoodProducers.Count + "," + sortedFoodProducersSupplyCapacity.Count);
            string moreLog = "Food Producers";
            for (int i = 0; i < sortedFoodProducers.Count; i++) {
                Character c = sortedFoodProducers[i];
                moreLog += "\n" + c.name;
            }
            Debug.LogError(moreLog);
        }
        if (sortedResourceProducers.Count != sortedResourceProducersSupplyCapacity.Count) {
            Debug.LogError("Resource producer list and resource supply capacity list not the same length: " + sortedResourceProducers.Count + "," + sortedResourceProducersSupplyCapacity.Count);
            string moreLog = "Resource Producers";
            for (int i = 0; i < sortedResourceProducers.Count; i++) {
                Character c = sortedResourceProducers[i];
                moreLog += "\n" + c.name;
            }
            Debug.LogError(moreLog);
        }
        if (sortedCombatants.Count != sortedCombatantValues.Count) {
            Debug.LogError("Combatants list and combatant values list not the same length: " + sortedCombatants.Count + "," + sortedCombatantValues.Count);
            string moreLog = "Combatants";
            for (int i = 0; i < sortedCombatants.Count; i++) {
                Character c = sortedCombatants[i];
                moreLog += "\n" + c.name;
            }
            Debug.LogError(moreLog);
        }
#endif

#if DEBUG_LOG
        log += "\nReserved Food Producers With Work: ";
#endif
        int totalFSP = 0;
        for (int i = 0; i < sortedFoodProducers.Count; i++) {
            Character fp = sortedFoodProducers[i];
            if (totalFSP >= numOfActiveResidents) {
                fp.classComponent.SetShouldChangeClass(true);
                numberOfAvailableVillagers++;
            } else {
                totalFSP += sortedFoodProducersSupplyCapacity[i];
                fp.classComponent.SetShouldChangeClass(false);
#if DEBUG_LOG
                log += $"{fp.name},";
#endif
            }
        }

#if DEBUG_LOG
        log += "\nReserved Food Producers Without Work: ";
#endif
        if (totalFSP < numOfActiveResidents) {
            for (int i = 0; i < foodProducersNoValue.Count; i++) {
                Character fp = foodProducersNoValue[i];
                int newValue = fp.classComponent.GetFoodSupplyCapacityValueBase();
                if (totalFSP >= numOfActiveResidents || newValue == 0) {
                    fp.classComponent.SetShouldChangeClass(true);
                    numberOfAvailableVillagers++;
                } else {
                    CharacterClassData characterClassData = CharacterManager.Instance.GetOrCreateCharacterClassData(fp.characterClass.className);
                    if (owner.HasStructure(characterClassData.workStructureType) || owner.HasBlueprintOnTileForStructure(characterClassData.workStructureType)) {
                        totalFSP += newValue;
                        fp.classComponent.SetShouldChangeClass(false);
#if DEBUG_LOG
                        log += $"{fp.name},";
#endif
                    } else {
                        fp.classComponent.SetShouldChangeClass(true);
                        numberOfAvailableVillagers++;
                    }
                }
            }
        }

#if DEBUG_LOG
        log += "\nReserved Resource Producers With Work: ";
#endif
        int totalRSP = 0;
        for (int i = 0; i < sortedResourceProducers.Count; i++) {
            Character rp = sortedResourceProducers[i];
            if (totalRSP >= numOfActiveResidents) {
                rp.classComponent.SetShouldChangeClass(true);
                numberOfAvailableVillagers++;
            } else {
                totalRSP += sortedResourceProducersSupplyCapacity[i];
                rp.classComponent.SetShouldChangeClass(false);
#if DEBUG_LOG
                log += $"{rp.name},";
#endif
            }
        }

#if DEBUG_LOG
        log += "\nReserved Resource Producers Without Work: ";
#endif
        if (totalRSP < numOfActiveResidents) {
            for (int i = 0; i < resourceProducersNoValue.Count; i++) {
                Character rp = resourceProducersNoValue[i];
                int newValue = rp.classComponent.GetResourceSupplyCapacityValueBase(rp.characterClass.className);
                if (totalFSP >= numOfActiveResidents || newValue == 0) {
                    rp.classComponent.SetShouldChangeClass(true);
                    numberOfAvailableVillagers++;
                } else {
                    CharacterClassData characterClassData = CharacterManager.Instance.GetOrCreateCharacterClassData(rp.characterClass.className);
                    if (owner.HasStructure(characterClassData.workStructureType) || owner.HasBlueprintOnTileForStructure(characterClassData.workStructureType)) {
                        totalFSP += newValue;
                        rp.classComponent.SetShouldChangeClass(false);
#if DEBUG_LOG
                        log += $"{rp.name},";
#endif
                    } else {
                        rp.classComponent.SetShouldChangeClass(true);
                        numberOfAvailableVillagers++;
                    }
                }
            }
        }

        RuinarchListPool<Character>.Release(foodProducersNoValue);
        RuinarchListPool<Character>.Release(sortedFoodProducers);
        RuinarchListPool<int>.Release(sortedFoodProducersSupplyCapacity);
        RuinarchListPool<Character>.Release(sortedResourceProducers);
        RuinarchListPool<Character>.Release(resourceProducersNoValue);
        RuinarchListPool<int>.Release(sortedResourceProducersSupplyCapacity);

#if DEBUG_LOG
        log += "\nVillagers = " + numOfActiveResidents + ", FSP = " + foodSupplyCapacity + ", RSP = " + resourceSupplyCapacity + ", Combatants = " + numOfCombatants + ", Needed Combatants = " + neededCombatants + ", Non-Reserved = " + numberOfAvailableVillagers;
#endif

        ProcessNeededFoodProducerClasses(numOfActiveResidents, foodSupplyCapacity, villagerCanBecomeButcher, villagerCanBecomeFisher, ref log);
        ProcessNeededResourceClasses(numOfActiveResidents, resourceSupplyCapacity, ref log);

        if (m_bypass) {
#if DEBUG_LOG
            log += "\nBypass: Will NOT process Crafter, Combatants, and Special Classes because food and resource producers are already available";
#endif
            for (int i = 0; i < sortedCombatants.Count; i++) {
                Character c = sortedCombatants[i];
                c.classComponent.SetShouldChangeClass(true);
                numberOfAvailableVillagers++;
            }
        } else {
#if DEBUG_LOG
            log += "\nNot Bypass: Will process Crafter, Combatants, and Special Classes";
            log += "\nReserved Combatants: ";
#endif
            for (int i = 0; i < sortedCombatants.Count; i++) {
                Character c = sortedCombatants[i];
                if (reservedCombatantCount < neededCombatants) {
                    c.classComponent.SetShouldChangeClass(false);
                    reservedCombatantCount++;
#if DEBUG_LOG
                    log += $"{c.name},";
#endif
                } else {
                    c.classComponent.SetShouldChangeClass(true);
                    numberOfAvailableVillagers++;
                }
            }
            ProcessNeededCrafter(ref log);
            ProcessNeededCombatantClasses(numOfCombatants, neededCombatants, ref log);
            ProcessNeededSpecialClasses(numberOfAvailableVillagers, villagerCanBecomeSkinner, villagerCanBecomeMerchant, ref log);

        }
        RuinarchListPool<Character>.Release(sortedCombatants);
        RuinarchListPool<int>.Release(sortedCombatantValues);



#if DEBUG_LOG
        Debug.Log(log);
#endif
    }
    public static int GetNumberOfNeededCombatants(int numOfActiveResidents) {
        return numOfActiveResidents - (Mathf.CeilToInt((numOfActiveResidents / 8f) * 3f));
    }
    private void ProcessNeededFoodProducerClasses(int numOfActiveResidents, int foodSupplyCapacity, bool villagerCanBecomeButcher, bool villagerCanBecomeFisher, ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Food Producers";
#endif
        if (numOfActiveResidents > foodSupplyCapacity) {
#if DEBUG_LOG
            log += "\nVillagers exceeds Food Supply Capacity, will try to create Food Producers";
            log += "\nChecking Butcher's Shop...";
#endif
            //If Villagers exceeds Food Supply Capacity, check if there is no existing Change To A Needed Class job for Food Producers
            //If no settlement job to change class to a food producer, proceed here
            //Identify which food producer is needed
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.BUTCHERS_SHOP);
            if (noWorkerStructure != null && villagerCanBecomeButcher) {
#if DEBUG_LOG
                log += "\nThere is unclaimed/non-reserved Butcher's Shop and there is a villager that can become a Butcher: " + noWorkerStructure.name;
                log += "\nCreate Change Class Job to BUTCHER";
#endif
                m_bypass = true;
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Butcher", noWorkerStructure);
            } else {
                log += "\nChecking Fishery...";
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.FISHERY);
                if (noWorkerStructure != null && villagerCanBecomeFisher) {
#if DEBUG_LOG
                    log += "\nThere is unclaimed/non-reserved Fishery and there is a villager that can become a Fisher: " + noWorkerStructure.name;
                    log += "\nCreate Change Class Job to FISHER";
#endif
                    m_bypass = true;
                    owner.settlementJobTriggerComponent.TriggerChangeClassJob("Fisher", noWorkerStructure);
                } else {
                    log += "\nChecking Farm...";
                    noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.FARM);
                    if (noWorkerStructure != null) {
#if DEBUG_LOG
                        log += "\nThere is unclaimed/non-reserved Farm: " + noWorkerStructure.name;
                        log += "\nCreate Change Class Job to FARMER";
#endif
                        m_bypass = true;
                        owner.settlementJobTriggerComponent.TriggerChangeClassJob("Farmer", noWorkerStructure);
                    } else {
                        log += "\nChecking Butcher's Shop no limit...";
                        noWorkerStructure = owner.GetFirstStructureOfType(STRUCTURE_TYPE.BUTCHERS_SHOP);
                        if (noWorkerStructure != null && villagerCanBecomeButcher) {
#if DEBUG_LOG
                            log += "\nThere is a Butcher's Shop and there is a villager that can become a Butcher: " + noWorkerStructure.name;
                            log += "\nCreate Change Class Job to BUTCHER";
#endif
                            m_bypass = true;
                            owner.settlementJobTriggerComponent.TriggerChangeClassJob("Butcher", null);
                        } else {
                            log += "\nChecking Fishery no limit...";
                            noWorkerStructure = owner.GetFirstStructureOfType(STRUCTURE_TYPE.FISHERY);
                            if (noWorkerStructure != null && villagerCanBecomeFisher) {
#if DEBUG_LOG
                                log += "\nThere is a Fishery and there is a villager that can become a Fisher: " + noWorkerStructure.name;
                                log += "\nCreate Change Class Job to FISHER";
#endif
                                m_bypass = true;
                                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Fisher", null);
                            } else {
                                log += "\nChecking Farm no limit...";
                                noWorkerStructure = owner.GetFirstStructureOfType(STRUCTURE_TYPE.FARM);
                                if (noWorkerStructure != null) {
#if DEBUG_LOG
                                    log += "\nThere is a Farm: " + noWorkerStructure.name;
                                    log += "\nCreate Change Class Job to FARMER";
#endif
                                    m_bypass = true;
                                    owner.settlementJobTriggerComponent.TriggerChangeClassJob("Farmer", null);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void ProcessNeededResourceClasses(int numOfActiveResidents, int resourceSupplyCapacity, ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Resource Producers";
#endif
        if (numOfActiveResidents > resourceSupplyCapacity) {
#if DEBUG_LOG
            log += "\nVillagers exceeds Resource Supply Capacity, will try to create Resource Producers";
#endif
            if (owner.owner?.factionType.type == FACTION_TYPE.Elven_Kingdom) {
#if DEBUG_LOG
                log += "\nVillage faction is Elven Kingdom";
#endif
                ResourceProducersProcessing(STRUCTURE_TYPE.LUMBERYARD, "Logger", STRUCTURE_TYPE.MINE, "Miner", ref log);
            } else {
#if DEBUG_LOG
                log += "\nVillage faction is NOT Elven Kingdom";
#endif
                ResourceProducersProcessing(STRUCTURE_TYPE.MINE, "Miner", STRUCTURE_TYPE.LUMBERYARD, "Logger", ref log);
            }
        }
    }
    private void ProcessNeededCrafter(ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Crafter";
        log += "\nChecking Workshop...";
#endif
        LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.WORKSHOP);
        if (noWorkerStructure != null) {
#if DEBUG_LOG
            log += "\nHas unclaimed Workshop: " + noWorkerStructure.name;
#endif
            LocationStructure structureWithWorker = owner.GetFirstStructureOfTypeThatHasWorkerOrIsReserved(STRUCTURE_TYPE.WORKSHOP);
            if (structureWithWorker == null) {
#if DEBUG_LOG
                log += "\nHas no claimed Workshop in village, create change class job";
                log += "\nCreate Change Class Job to CRAFTER";
#endif
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Crafter", noWorkerStructure);
            } else {
#if DEBUG_LOG
                log += "\nHas claimed Workshop in village: " + structureWithWorker.name;
#endif
            }
        } else {
#if DEBUG_LOG
            log += "\nHas no unclaimed Workshop";
#endif
        }
    }
    private void ProcessNeededCombatantClasses(int numOfCombatants, int neededCombatants, ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Combatants";
#endif
        if (numOfCombatants < neededCombatants) {
            //Combatant is needed
            string combatantClass = CharacterManager.Instance.GetRandomLowTierCombatant();
#if DEBUG_LOG
            log += "\nCurrent Combatants is less than needed, will create change to combatant class job";
            log += "\nCreate Change Class Job to " + combatantClass;
#endif
            owner.settlementJobTriggerComponent.TriggerChangeClassJob(combatantClass, null);
        }
    }
    private void ProcessNeededSpecialClasses(int numberOfAvailableVillagers, bool villagerCanBecomeSkinner, bool villagerCanBecomeMerchant, ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Special Worker Classes";
#endif
        int numOfChangeClassJob = owner.GetNumberOfJobsWith(JOB_TYPE.CHANGE_CLASS);
        if (numOfChangeClassJob < numberOfAvailableVillagers) {
#if DEBUG_LOG
            log += "\nChange Class Jobs is less than Non-Reserved Villagers, will try to create Special Classes";
            log += "\nChecking Workshop...";
#endif
            //If number of villagers that can still change class exceeds the number of change class jobs in settlement - this means that there are still spare residents that can change class to special worker class
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.WORKSHOP);
            if (noWorkerStructure != null) {
#if DEBUG_LOG
                log += "\nThere is unclaimed/non-reserved Workshop: " + noWorkerStructure.name;
                log += "\nCreate Change Class Job to CRAFTSMAN";
#endif
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Crafter", noWorkerStructure);
            } else {
#if DEBUG_LOG
                log += "\nChecking Skinner's Lodge...";
#endif
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.HUNTER_LODGE);
                if (noWorkerStructure != null && villagerCanBecomeSkinner) {
#if DEBUG_LOG
                    log += "\nThere is unclaimed/non-reserved Skinner's Lodge and there is a villager that can become a Skinner: " + noWorkerStructure.name;
                    log += "\nCreate Change Class Job to SKINNER";
#endif
                    owner.settlementJobTriggerComponent.TriggerChangeClassJob("Skinner", noWorkerStructure);
                } else {
#if DEBUG_LOG
                    log += "\nChecking Tavern...";
#endif
                    noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.TAVERN);
                    if (noWorkerStructure != null && villagerCanBecomeMerchant) {
#if DEBUG_LOG
                        log += "\nThere is unclaimed/non-reserved Tavern and there is a villager that can become a Merchant: " + noWorkerStructure.name;
                        log += "\nCreate Change Class Job to MERCHANT";
#endif
                        owner.settlementJobTriggerComponent.TriggerChangeClassJob("Merchant", noWorkerStructure);
                    } else {
#if DEBUG_LOG
                        log += "\nOtherwise, 35% to create Change Class Job to Combatant";
#endif
                        if (ChanceData.RollChance(CHANCE_TYPE.Create_Change_Class_Combatant, ref log)) {
                            string combatantClass = CharacterManager.Instance.GetRandomLowTierCombatant();
#if DEBUG_LOG
                            log += "\nCreate Change Class Job to " + combatantClass;
#endif
                            owner.settlementJobTriggerComponent.TriggerChangeClassJob(combatantClass, null);
                        }
                    }
                }
            }
        }
    }

    private bool ResourceProducersProcessing(STRUCTURE_TYPE primaryStructure, string primaryClass, STRUCTURE_TYPE secondaryStructure, string secondaryClass, ref string log) {
        log += "\nChecking " + primaryStructure.ToString() + "...";
        LocationStructure noWorkerPrimaryStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(primaryStructure);
        LocationStructure noWorkerSecondaryStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(secondaryStructure);
        if (noWorkerPrimaryStructure == null && noWorkerSecondaryStructure != null) {
#if DEBUG_LOG
            log += "\nThere is NO unclaimed/non-reserved " + primaryStructure.ToString() + " but there is unclaimed/non-reserved " + secondaryStructure.ToString() + ": " + noWorkerSecondaryStructure.name;
            log += "\nCreate Change Class Job to " + secondaryClass;
#endif
            m_bypass = true;
            owner.settlementJobTriggerComponent.TriggerChangeClassJob(secondaryClass, noWorkerSecondaryStructure);
            return true;
        } else {
            log += "\nChecking " + primaryStructure.ToString() + " no limit...";
            noWorkerPrimaryStructure = owner.GetFirstStructureOfType(primaryStructure);
            if (noWorkerPrimaryStructure != null) {
#if DEBUG_LOG
                log += "\nThere is a " + primaryStructure.ToString() + ": " + noWorkerPrimaryStructure.name;
                log += "\nCreate Change Class Job to " + primaryClass;
#endif
                m_bypass = true;
                owner.settlementJobTriggerComponent.TriggerChangeClassJob(primaryClass, null);
                return true;
            } else {
                log += "\nChecking " + secondaryStructure.ToString() + " no limit...";
                noWorkerSecondaryStructure = owner.GetFirstStructureOfType(secondaryStructure);
                if (noWorkerSecondaryStructure != null) {
#if DEBUG_LOG
                    log += "\nThere is a " + secondaryStructure.ToString() + ": " + noWorkerSecondaryStructure.name;
                    log += "\nCreate Change Class Job to " + secondaryClass;
#endif
                    m_bypass = true;
                    owner.settlementJobTriggerComponent.TriggerChangeClassJob(secondaryClass, null);
                    return true;
                }
            }
        }
        return false;
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataSettlementClassComponent data) {
        if (owner.locationType == LOCATION_TYPE.VILLAGE) {
            SchedulingManager.Instance.AddEntry(morningScheduleDateForProcessingOfNeededClasses, MorningProcessingOfNeededClasses, null);
            SchedulingManager.Instance.AddEntry(afternoonScheduleDateForProcessingOfNeededClasses, AfternoonProcessingOfNeededClasses, null);
        }
    }
#endregion

}

public class SaveDataSettlementClassComponent : SaveData<SettlementClassComponent> {
    public int currentClassOrderIndex;
    public List<string> currentResidentClasses;
    public GameDate morningScheduleDateForProcessingOfNeededClasses;
    public GameDate afternoonScheduleDateForProcessingOfNeededClasses;

    #region Overrides
    public override void Save(SettlementClassComponent data) {
        currentClassOrderIndex = data.currentClassOrderIndex;
        currentResidentClasses = new List<string>(data.currentResidentClasses);
        morningScheduleDateForProcessingOfNeededClasses = data.morningScheduleDateForProcessingOfNeededClasses;
        afternoonScheduleDateForProcessingOfNeededClasses = data.afternoonScheduleDateForProcessingOfNeededClasses;
    }

    public override SettlementClassComponent Load() {
        SettlementClassComponent component = new SettlementClassComponent(this);
        return component;
    }
#endregion
}