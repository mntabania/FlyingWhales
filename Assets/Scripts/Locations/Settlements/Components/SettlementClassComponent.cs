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

    public GameDate scheduleDateForProcessingOfNeededClasses { get; private set; }

    #region getters
    public int currentClassOrderIndex => _currentClassOrderIndex;
    public List<string> currentResidentClasses => _currentResidentClasses;
    #endregion

    public SettlementClassComponent() {
        _currentClassOrderIndex = 0;
        _currentResidentClasses = new List<string>();
        InitialScheduleProcessingOfNeededClasses();
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
            int minimumTick = 2 * GameManager.ticksPerHour; //2 AM in ticks
            int maximumTick = 5 * GameManager.ticksPerHour; //5 AM in ticks

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
        owner.ForceCancelJobTypes(JOB_TYPE.CHANGE_CLASS);
        int numOfActiveResidents = owner.GetNumberOfResidentsThatIsAliveVillager();
        int foodSupplyCapacity = owner.resourcesComponent.GetFoodSupplyCapacity();
        int resourceSupplyCapacity = owner.resourcesComponent.GetResourceSupplyCapacity();
        int numOfCombatants = owner.GetNumOfResidentsThatIsAliveCombatant();
        int neededCombatants = 5;
        if (numOfActiveResidents > 8) {
            neededCombatants = (numOfActiveResidents / 8) * 5;
        }

        //Determine who should change classes and who should not change class
        //Put this here so that looping through all residents is only done once
        int reservedCombatantCount = 0;
        int numberOfAvailableVillagers = 0;
        for (int i = 0; i < owner.residents.Count; i++) {
            Character c = owner.residents[i];
            if (!c.isDead) {
                if (c.characterClass.IsCombatant()) {
                    if (reservedCombatantCount < neededCombatants) {
                        c.classComponent.SetShouldChangeClass(false);
                        reservedCombatantCount++;
                    } else {
                        c.classComponent.SetShouldChangeClass(true);
                        numberOfAvailableVillagers++;
                    }
                } else {
                    if (c.structureComponent.HasWorkPlaceStructure()) {
                        if (c.characterClass.className.IsFoodProducerClassName() && numOfActiveResidents > foodSupplyCapacity) {
                            c.classComponent.SetShouldChangeClass(false);
                        } else if (c.characterClass.className.IsResourceProducerClassName() && numOfActiveResidents > resourceSupplyCapacity) {
                            c.classComponent.SetShouldChangeClass(false);
                        } else {
                            c.classComponent.SetShouldChangeClass(true);
                            numberOfAvailableVillagers++;
                        }
                    } else {
                        c.classComponent.SetShouldChangeClass(true);
                        numberOfAvailableVillagers++;
                    }
                }
            }
        }
        
        ProcessNeededFoodProducerClasses(numOfActiveResidents, foodSupplyCapacity);
        ProcessNeededResourceClasses(numOfActiveResidents, resourceSupplyCapacity);
        ProcessNeededCombatantClasses(numOfCombatants, neededCombatants);
        ProcessNeededSpecialClasses(numberOfAvailableVillagers);
    }
    private void ProcessNeededFoodProducerClasses(int numOfActiveResidents, int foodSupplyCapacity) {
        if (numOfActiveResidents > foodSupplyCapacity) {
            //If Villagers exceeds Food Supply Capacity, check if there is no existing Change To A Needed Class job for Food Producers
            //If no settlement job to change class to a food producer, proceed here
            //Identify which food producer is needed
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.FARM);
            if (noWorkerStructure != null) {
                //if there is a Farm in the Village that hasn't been claimed yet
                //Create Change Class Job To Farmer
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Farmer");
            } else {
                //otherwise, if there is a Fishery in the Village that hasn't been claimed yet and there is a resident that can become a Fisher
                //Create Change Class Job To Fisher
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.FISHERY);
                if (noWorkerStructure != null) {
                    if (owner.GetFirstResidentThatIsAbleAndCanBecomeClass("Fisher") != null) {
                        owner.settlementJobTriggerComponent.TriggerChangeClassJob("Fisher");
                    }
                } else {
                    //otherwise, if there is a Butcher's Shop in the Village that hasn't been claimed yet and there is a resident that can become a Butcher
                    //Create Change Class Job To Butcher
                    noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.BUTCHERS_SHOP);
                    if (noWorkerStructure != null) {
                        if (owner.GetFirstResidentThatIsAbleAndCanBecomeClass("Butcher") != null) {
                            owner.settlementJobTriggerComponent.TriggerChangeClassJob("Butcher");
                        }
                    }
                }
            }
        }
    }
    private void ProcessNeededResourceClasses(int numOfActiveResidents, int resourceSupplyCapacity) {
        if (numOfActiveResidents > resourceSupplyCapacity) {
            //If Villagers exceeds Resource Supply Capacity, a Basic Resource Gatherer is needed
            //Identify which resource producer is needed
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.LUMBERYARD);
            if (noWorkerStructure != null) {
                //if there is a Lumberyard in the Village that hasn't been claimed yet
                //Create Change Class Job To Logger
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Logger");
            } else {
                //if there is a Mine in the Village that hasn't been claimed yet and there is a resident that can become a Miner
                //Create Change Class Job To Miner
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.MINE);
                if (noWorkerStructure != null) {
                    owner.settlementJobTriggerComponent.TriggerChangeClassJob("Miner");
                }
            }
        }
    }
    private void ProcessNeededCombatantClasses(int numOfCombatants, int neededCombatants) {
        if (numOfCombatants < neededCombatants) {
            //Combatant is needed
            string combatantClass = CharacterManager.Instance.GetRandomCombatant();
            owner.settlementJobTriggerComponent.TriggerChangeClassJob(combatantClass);
        }
    }
    private void ProcessNeededSpecialClasses(int numberOfAvailableVillagers) {
        int numOfChangeClassJob = owner.GetNumberOfJobsWith(JOB_TYPE.CHANGE_CLASS);
        if (numOfChangeClassJob < numberOfAvailableVillagers) {
            //If number of villagers that can still change class exceeds the number of change class jobs in settlement - this means that there are still spare residents that can change class to special worker class
            LocationStructure noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.HUNTER_LODGE);
            if (noWorkerStructure != null) {
                //if there is a Skinner's Lodge in the Village that hasn't been claimed yet
                //Create Change Class Job To Skinner
                owner.settlementJobTriggerComponent.TriggerChangeClassJob("Skinner");
            } else {
                //otherwise, if there is a Workshop in the Village that hasn't been claimed yet and there is a resident that can become a Craftsman
                //Create Change Class Job To Craftsman
                noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.WORKSHOP);
                if (noWorkerStructure != null) {
                    owner.settlementJobTriggerComponent.TriggerChangeClassJob("Craftsman");
                } else {
                    //otherwise, if there is a Tavern in the Village that hasn't been claimed yet and there is a resident that can become a Merchant
                    //Create Change Class Job To Merchant
                    noWorkerStructure = owner.GetFirstStructureOfTypeThatHasNoWorker(STRUCTURE_TYPE.TAVERN);
                    if (noWorkerStructure != null) {
                        owner.settlementJobTriggerComponent.TriggerChangeClassJob("Merchant");
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
