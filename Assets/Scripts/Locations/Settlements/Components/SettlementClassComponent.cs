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

    public GameDate scheduleDateForProcessingOfNeededClasses { get; private set; }

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
        scheduleDateForProcessingOfNeededClasses = data.scheduleDateForProcessingOfNeededClasses;
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
    public void InitialScheduleProcessingOfNeededClasses() {
        if (owner.locationType == LOCATION_TYPE.VILLAGE) {
            int minimumTick = GameManager.Instance.GetTicksBasedOnHour(2); //2 AM in ticks
            int maximumTick = GameManager.Instance.GetTicksBasedOnHour(5); //5 AM in ticks

            int scheduledTick = GameUtilities.RandomBetweenTwoNumbers(minimumTick, maximumTick);
            GameDate schedule = GameManager.Instance.Today().AddDays(1);
            schedule.SetTicks(scheduledTick);
            scheduleDateForProcessingOfNeededClasses = schedule;
            SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfNeededClasses, ProcessingOfNeededClasses, null);
        }
    }
    private void ProcessingOfNeededClasses() {
        if (owner.HasResidentThatIsNotDead()) {
            ProcessNeededClasses();
        }
        scheduleDateForProcessingOfNeededClasses = GameManager.Instance.Today().AddDays(1);
        SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfNeededClasses, ProcessingOfNeededClasses, null);
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

        //Determine who should change classes and who should not change class
        //Put this here so that looping through all residents is only done once
        int reservedCombatantCount = 0;
        int numberOfAvailableVillagers = 0;
        List<Character> sortedFoodProducers = RuinarchListPool<Character>.Claim();
        List<int> sortedFoodProducersSupplyCapacity = RuinarchListPool<int>.Claim();
        List<Character> sortedResourceProducers = RuinarchListPool<Character>.Claim();
        List<int> sortedResourceProducersSupplyCapacity = RuinarchListPool<int>.Claim();
        List<Character> reservedCombatantCharacters = RuinarchListPool<Character>.Claim();
        for (int i = 0; i < owner.residents.Count; i++) {
            Character c = owner.residents[i];
            if (!c.isDead) {
                if (c.characterClass.IsCombatant()) {
                    if (reservedCombatantCount < neededCombatants) {
                        c.classComponent.SetShouldChangeClass(false);
                        reservedCombatantCharacters.Add(c);
                        reservedCombatantCount++;
                    } else {
                        c.classComponent.SetShouldChangeClass(true);
                        numberOfAvailableVillagers++;
                    }
                } else {
                    if (c.characterClass.className.IsFoodProducerClassName()) {
                        //Add all food producer characters in order of their supply value from highest to lowest
                        int supply = c.classComponent.GetFoodSupplyCapacityValue();
                        if (supply == 0) {
                            //If supply is 0, it is automatically added at the bottom of the list
                            sortedFoodProducers.Add(c);
                            sortedFoodProducersSupplyCapacity.Add(supply);
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
                            sortedResourceProducers.Add(c);
                            sortedResourceProducersSupplyCapacity.Add(supply);
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
        log += "\nReserved Food Producers: ";
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
        log += "\nReserved Resource Producers: ";
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

        RuinarchListPool<Character>.Release(sortedFoodProducers);
        RuinarchListPool<int>.Release(sortedFoodProducersSupplyCapacity);
        RuinarchListPool<Character>.Release(sortedResourceProducers);
        RuinarchListPool<int>.Release(sortedResourceProducersSupplyCapacity);

#if DEBUG_LOG
        log += "\nVillagers = " + numOfActiveResidents + ", FSP = " + foodSupplyCapacity + ", RSP = " + resourceSupplyCapacity + ", Combatants = " + numOfCombatants + ", Needed Combatants = " + neededCombatants + ", Non-Reserved = " + numberOfAvailableVillagers;
#endif

        ProcessNeededFoodProducerClasses(numOfActiveResidents, foodSupplyCapacity, ref log);
        ProcessNeededResourceClasses(numOfActiveResidents, resourceSupplyCapacity, ref log);
        if (m_bypass) {
#if DEBUG_LOG
            log += "\nBypass:";
            log += GameManager.Instance.TodayLogString() + owner.name + " will NOT process Combatants and special classes because food and resource producers are already available";
#endif
            for (int x = 0; x < reservedCombatantCharacters.Count; ++x) {
                reservedCombatantCharacters[x].classComponent.SetShouldChangeClass(true);
                reservedCombatantCount--;
            }
        } else {
#if DEBUG_LOG
            log += "\nNo Bypass:";
            log += GameManager.Instance.TodayLogString() + owner.name + " will process Combatants and special classes because of bypass";
#endif
            ProcessNeededCombatantClasses(numOfCombatants, neededCombatants, ref log);
            ProcessNeededSpecialClasses(numberOfAvailableVillagers, ref log);

        }
        RuinarchListPool<Character>.Release(reservedCombatantCharacters);
        


#if DEBUG_LOG
        Debug.Log(log);
#endif
    }
    public static int GetNumberOfNeededCombatants(int numOfActiveResidents) {
        return Mathf.CeilToInt((numOfActiveResidents / 8f) * 3f);
    }
    private void ProcessNeededFoodProducerClasses(int numOfActiveResidents, int foodSupplyCapacity, ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Food Producers";
#endif
        if (numOfActiveResidents > foodSupplyCapacity) {
#if DEBUG_LOG
            log += "\nVillagers exceeds Food Supply Capacity";
#endif
            //If Villagers exceeds Food Supply Capacity, check if there is no existing Change To A Needed Class job for Food Producers
            //If no settlement job to change class to a food producer, proceed here
            //Identify which food producer is needed
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.FARM);
            if (noWorkerStructure != null) {
#if DEBUG_LOG
                log += "\nUnclaimed Farm: " + noWorkerStructure.name;
                log += "\nCreate Change Class Job to FARMER";
#endif
                m_bypass = true;
                //if there is a Farm in the Village that hasn't been claimed yet
                //Create Change Class Job To Farmer
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Farmer", noWorkerStructure);
            } else {
                //otherwise, if there is a Fishery in the Village that hasn't been claimed yet and there is a resident that can become a Fisher
                //Create Change Class Job To Fisher
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.FISHERY);
                if (noWorkerStructure != null) {
#if DEBUG_LOG
                    log += "\nUnclaimed Fishery: " + noWorkerStructure.name;
#endif
                    if (owner.GetFirstResidentThatIsAbleAndCanBecomeClass("Fisher") != null) {
#if DEBUG_LOG
                        log += "\nHas Villager that can become Fisher";
                        log += "\nCreate Change Class Job to FISHER";
#endif
                        m_bypass = true;
                        owner.settlementJobTriggerComponent.TriggerChangeClassJob("Fisher", noWorkerStructure);
                    }
                } else {
                    //otherwise, if there is a Butcher's Shop in the Village that hasn't been claimed yet and there is a resident that can become a Butcher
                    //Create Change Class Job To Butcher
                    noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.BUTCHERS_SHOP);
                    if (noWorkerStructure != null) {
#if DEBUG_LOG
                        log += "\nUnclaimed Butcher's Shop: " + noWorkerStructure.name;
#endif
                        if (owner.GetFirstResidentThatIsAbleAndCanBecomeClass("Butcher") != null) {
#if DEBUG_LOG
                            log += "\nHas Villager that can become Butcher";
                            log += "\nCreate Change Class Job to BUTCHER";
#endif
                            m_bypass = true;
                            owner.settlementJobTriggerComponent.TriggerChangeClassJob("Butcher", noWorkerStructure);
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
            log += "\nVillagers exceeds Resource Supply Capacity";
#endif
            //If Villagers exceeds Resource Supply Capacity, a Basic Resource Gatherer is needed
            //Identify which resource producer is needed
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.LUMBERYARD);
            if (noWorkerStructure != null) {
#if DEBUG_LOG
                log += "\nUnclaimed Lumberyard: " + noWorkerStructure.name;
                log += "\nCreate Change Class Job to LOGGER";
#endif
                //if there is a Lumberyard in the Village that hasn't been claimed yet
                //Create Change Class Job To Logger
                m_bypass = true;
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Logger", noWorkerStructure);
            } else {
                //if there is a Mine in the Village that hasn't been claimed yet and there is a resident that can become a Miner
                //Create Change Class Job To Miner
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.MINE);
                if (noWorkerStructure != null) {
#if DEBUG_LOG
                    log += "\nUnclaimed Mine: " + noWorkerStructure.name;
                    log += "\nCreate Change Class Job to MINER";
#endif
                    m_bypass = true;
                    owner.settlementJobTriggerComponent.TriggerChangeClassJob("Miner", noWorkerStructure);
                }
            }
        }
    }
    private void ProcessNeededCombatantClasses(int numOfCombatants, int neededCombatants, ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Combatants";
#endif
        if (numOfCombatants < neededCombatants) {
            //Combatant is needed
            string combatantClass = CharacterManager.Instance.GetRandomCombatant();
#if DEBUG_LOG
            log += "\nCurrent Combatants is less than needed, will create change to combatant class job";
            log += "\nCreate Change Class Job to " + combatantClass;
#endif
            owner.settlementJobTriggerComponent.TriggerChangeClassJob(combatantClass, null);
        }
    }
    private void ProcessNeededSpecialClasses(int numberOfAvailableVillagers, ref string log) {
#if DEBUG_LOG
        log += "\nProcess Needed Special Worker Classes";
#endif
        int numOfChangeClassJob = owner.GetNumberOfJobsWith(JOB_TYPE.CHANGE_CLASS);
        if (numOfChangeClassJob < numberOfAvailableVillagers) {
#if DEBUG_LOG
            log += "\nChange Class Jobs is less than Non-Reserved Villagers";
#endif
            //If number of villagers that can still change class exceeds the number of change class jobs in settlement - this means that there are still spare residents that can change class to special worker class
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.HUNTER_LODGE);
            if (noWorkerStructure != null) {
#if DEBUG_LOG
                log += "\nUnclaimed Skinner's Lodge: " + noWorkerStructure.name;
                log += "\nCreate Change Class Job to SKINNER";
#endif
                //if there is a Skinner's Lodge in the Village that hasn't been claimed yet
                //Create Change Class Job To Skinner
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Skinner", noWorkerStructure);
            } else {
                //otherwise, if there is a Workshop in the Village that hasn't been claimed yet and there is a resident that can become a Craftsman
                //Create Change Class Job To Craftsman
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.WORKSHOP);
                if (noWorkerStructure != null) {
#if DEBUG_LOG
                    log += "\nUnclaimed Workshop: " + noWorkerStructure.name;
                    log += "\nCreate Change Class Job to CRAFTSMAN";
#endif
                    owner.settlementJobTriggerComponent.TriggerChangeClassJob("Craftsman", noWorkerStructure);
                } else {
                    //otherwise, if there is a Tavern in the Village that hasn't been claimed yet and there is a resident that can become a Merchant
                    //Create Change Class Job To Merchant
                    noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.TAVERN);
                    if (noWorkerStructure != null) {
#if DEBUG_LOG
                        log += "\nUnclaimed Tavern: " + noWorkerStructure.name;
                        log += "\nCreate Change Class Job to MERCHANT";
#endif
                        owner.settlementJobTriggerComponent.TriggerChangeClassJob("Merchant", noWorkerStructure);
                    }
                }
            }
        }
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataSettlementClassComponent data) {
        if (owner.locationType == LOCATION_TYPE.VILLAGE) {
            SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfNeededClasses, ProcessingOfNeededClasses, null);
        }
    }
#endregion

}

public class SaveDataSettlementClassComponent : SaveData<SettlementClassComponent> {
    public int currentClassOrderIndex;
    public List<string> currentResidentClasses;
    public GameDate scheduleDateForProcessingOfNeededClasses;

#region Overrides
    public override void Save(SettlementClassComponent data) {
        currentClassOrderIndex = data.currentClassOrderIndex;
        currentResidentClasses = data.currentResidentClasses;
        scheduleDateForProcessingOfNeededClasses = data.scheduleDateForProcessingOfNeededClasses;
    }

    public override SettlementClassComponent Load() {
        SettlementClassComponent component = new SettlementClassComponent(this);
        return component;
    }
#endregion
}