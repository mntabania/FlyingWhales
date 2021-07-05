using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

[System.Serializable]
public class SaveDataCharacter : SaveData<Character>, ISavableCounterpart {
    public string persistentID { get; set; }
    public int id;
    public string firstName;
    public string surName;
    public bool isDead;
    public GENDER gender;
    public SEXUALITY sexuality;
    //public string className; //Moved to CharacterClassComponent
    public RACE race;
    //public string previousClassName; //Moved to CharacterClassComponent

    public int currentHP;
    public int doNotRecoverHP;

    public Vector3 worldPos;
    public TileLocationSave deathTileLocation;
    public Quaternion rotation;
    public bool hasMarker;
    public bool hasExpiry;
    public GameDate markerExpiryDate;

    public PortraitSettings portraitSettings;
    public SaveDataMinion minion;
    public bool hasMinion;

    public List<INTERACTION_TYPE> advertisedActions;
    public bool canCombat;
    public string deathStr;
    public bool hasUnresolvedCrime;
    public bool isInLimbo;
    public bool isLimboCharacter;
    public bool destroyMarkerOnDeath;
    public bool isWanderer;
    public bool hasBeenRaisedFromDead;
    public bool isPreplaced;
    public bool isStoredAsTarget;
    public bool isDeadReference;
    public List<string> interestedItemNames;

    public bool isRaisedByNecro;

    public POI_STATE state;
    public INTERACTION_TYPE causeOfDeath;
    public List<PLAYER_SKILL_TYPE> afflictionsSkillsInflictedByPlayer;
    
    public SaveDataLycanthropeData lycanData;
    public bool hasLycan;

    //References
    public string grave;
    public Log deathLog;
    public string homeRegion;
    public string homeSettlement;
    public string homeStructure;
    public string currentRegion;
    public string currentStructure;
    public string faction;

    public string currentJob;
    public string currentActionNode;

    public string territory;
    public List<string> items;
    public List<string> equipmentInventory;
    public List<string> ownedItems;
    public List<string> jobs;
    public List<string> forceCancelJobsOnTickEnded;

    public bool isInfoUnlocked;
    public string deployedAtStructure;

    public SaveDataTraitContainer saveDataTraitContainer;
    public SaveDataBaseRelationshipContainer saveDataBaseRelationshipContainer;

    public SaveDataTrapStructure trapStructure;
    public SaveDataCharacterClassComponent classComponent;
    public SaveDataCharacterNeedsComponent needsComponent;
    public SaveDataCharacterStructureComponent structureComponent;
    public SaveDataCharacterStateComponent stateComponent;
    public SaveDataNonActionEventsComponent nonActionEventsComponent;
    public SaveDataInterruptComponent interruptComponent;
    public SaveDataBehaviourComponent behaviourComponent;
    public SaveDataMoodComponent moodComponent;
    public SaveDataCharacterJobTriggerComponent jobComponent;
    public SaveDataReactionComponent reactionComponent;
    public SaveDataLogComponent logComponent;
    public SaveDataCombatComponent combatComponent;
    public SaveDataRumorComponent rumorComponent;
    public SaveDataAssumptionComponent assumptionComponent;
    public SaveDataMovementComponent movementComponent;
    public SaveDataStateAwarenessComponent stateAwarenessComponent;
    public SaveDataCarryComponent carryComponent;
    public SaveDataCharacterPartyComponent partyComponent;
    public SaveDataGatheringComponent gatheringComponent;
    public SaveDataCharacterTileObjectComponent tileObjectComponent;
    public SaveDataCrimeComponent crimeComponent;
    public SaveDataReligionComponent religionComponent;
    public SaveDataLimiterComponent limiterComponent;
    public SaveDataPiercingAndResistancesComponent piercingAndResistancesComponent;
    public SaveDataPreviousCharacterDataComponent previousCharacterDataComponent;
    public SaveDataCharacterTraitComponent traitComponent;
    public SaveDataCharacterMoneyComponent moneyComponent;
    //public SaveDataBuffStatsBonus buffStatusBonus;
    public SaveDataCharacterTalentComponent talentComponent;
    public SaveDataResourceStorageComponent resourceStorageComponent;
    public SaveDataDailyScheduleComponent dailyScheduleComponent;
    //public SaveDataEquipmentComponent equipmentComponent;
    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Character;
    public SaveDataVillagerWantsComponent villagerWantsComponent { get; set; }
    #endregion

    public override void Save(Character data) {
        persistentID = data.persistentID;
        id = data.id;
        firstName = data.firstName;
        surName = data.surName;
        isDead = data.isDead;
        gender = data.gender;
        sexuality = data.sexuality;
        //className = data.characterClass.className;//Moved to CharacterClassComponent
        race = data.race;
        //isAlliedWithPlayer = data.isAlliedWithPlayer;
        currentHP = data.currentHP;
        doNotRecoverHP = data.doNotRecoverHP;
        portraitSettings = data.visuals.portraitSettings;
        advertisedActions = data.advertisedActions;
        canCombat = data.canCombat;
        deathStr = data.deathStr;
        hasUnresolvedCrime = data.hasUnresolvedCrime;
        isInLimbo = data.isInLimbo;
        isLimboCharacter = data.isLimboCharacter;
        destroyMarkerOnDeath = data.destroyMarkerOnDeath;
        isWanderer = data.isWanderer;
        hasBeenRaisedFromDead = data.hasBeenRaisedFromDead;
        interestedItemNames = data.interestedItemNames;
        state = data.state;
        causeOfDeath = data.causeOfDeath;
        //previousClassName = data.classComponent.previousClassName; //Moved to CharacterClassComponent
        isPreplaced = data.isPreplaced;
        afflictionsSkillsInflictedByPlayer = data.afflictionsSkillsInflictedByPlayer;
        isStoredAsTarget = data.isStoredAsTarget;
        isDeadReference = data.isDeadReference;

        if (data.marker) {
            hasMarker = true;
            worldPos = data.gridTileWorldPosition;
            //rotation = data.marker.visualsParent.transform.localRotation;

            if (data.marker.hasExpiry) {
                hasExpiry = true;
                markerExpiryDate = data.marker.destroyDate;
            }
        }
        
        deathTileLocation = data.deathTilePosition != null ? new TileLocationSave(data.deathTilePosition) : new TileLocationSave();

        trapStructure = new SaveDataTrapStructure(); trapStructure.Save(data.trapStructure);
        classComponent = new SaveDataCharacterClassComponent(); classComponent.Save(data.classComponent);
        needsComponent = new SaveDataCharacterNeedsComponent(); needsComponent.Save(data.needsComponent);
        structureComponent = new SaveDataCharacterStructureComponent(); structureComponent.Save(data.structureComponent);
        stateComponent = new SaveDataCharacterStateComponent(); stateComponent.Save(data.stateComponent);
        nonActionEventsComponent = new SaveDataNonActionEventsComponent(); nonActionEventsComponent.Save(data.nonActionEventsComponent);
        interruptComponent = new SaveDataInterruptComponent(); interruptComponent.Save(data.interruptComponent);
        behaviourComponent = new SaveDataBehaviourComponent(); behaviourComponent.Save(data.behaviourComponent);
        moodComponent = new SaveDataMoodComponent(); moodComponent.Save(data.moodComponent);
        jobComponent = new SaveDataCharacterJobTriggerComponent(); jobComponent.Save(data.jobComponent);
        reactionComponent = new SaveDataReactionComponent(); reactionComponent.Save(data.reactionComponent);
        logComponent = new SaveDataLogComponent(); logComponent.Save(data.logComponent);
        combatComponent = new SaveDataCombatComponent(); combatComponent.Save(data.combatComponent);
        rumorComponent = new SaveDataRumorComponent(); rumorComponent.Save(data.rumorComponent);
        assumptionComponent = new SaveDataAssumptionComponent(); assumptionComponent.Save(data.assumptionComponent);
        movementComponent = new SaveDataMovementComponent(); movementComponent.Save(data.movementComponent);
        stateAwarenessComponent = new SaveDataStateAwarenessComponent(); stateAwarenessComponent.Save(data.stateAwarenessComponent);
        carryComponent = new SaveDataCarryComponent(); carryComponent.Save(data.carryComponent);
        partyComponent = new SaveDataCharacterPartyComponent(); partyComponent.Save(data.partyComponent);
        gatheringComponent = new SaveDataGatheringComponent(); gatheringComponent.Save(data.gatheringComponent);
        tileObjectComponent = new SaveDataCharacterTileObjectComponent(); tileObjectComponent.Save(data.tileObjectComponent);
        crimeComponent = new SaveDataCrimeComponent(); crimeComponent.Save(data.crimeComponent);
        religionComponent = new SaveDataReligionComponent(); religionComponent.Save(data.religionComponent);
        limiterComponent = new SaveDataLimiterComponent(); limiterComponent.Save(data.limiterComponent);
        piercingAndResistancesComponent = new SaveDataPiercingAndResistancesComponent(); piercingAndResistancesComponent.Save(data.piercingAndResistancesComponent);
        previousCharacterDataComponent = new SaveDataPreviousCharacterDataComponent(); previousCharacterDataComponent.Save(data.previousCharacterDataComponent);
        traitComponent = new SaveDataCharacterTraitComponent(); traitComponent.Save(data.traitComponent);
        //buffStatusBonus = new SaveDataBuffStatsBonus(); buffStatusBonus.Save(data.buffStatsBonus);
        moneyComponent = new SaveDataCharacterMoneyComponent(); moneyComponent.Save(data.moneyComponent);
        resourceStorageComponent = new SaveDataResourceStorageComponent(); resourceStorageComponent.Save(data.resourceStorageComponent);
        dailyScheduleComponent = new SaveDataDailyScheduleComponent(); dailyScheduleComponent.Save(data.dailyScheduleComponent);

        if (data.talentComponent != null) {
            talentComponent = new SaveDataCharacterTalentComponent(); talentComponent.Save(data.talentComponent);
        }
        if (data.villagerWantsComponent != null) {
            villagerWantsComponent = new SaveDataVillagerWantsComponent(); villagerWantsComponent.Save(data.villagerWantsComponent);
        }
        //equipmentComponent = new SaveDataEquipmentComponent(); equipmentComponent.Save(data.equipmentComponent);

        isInfoUnlocked = data.isInfoUnlocked;

        if (data.currentJob != null && data.currentJob.jobType != JOB_TYPE.NONE) {
            currentJob = data.currentJob.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentJob);
        }

        if (data.currentActionNode != null) {
            currentActionNode = data.currentActionNode.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentActionNode);
        }

        if (data.minion != null) {
            hasMinion = true;
            minion = new SaveDataMinion();
            minion.Save(data.minion);

        }
        if(data.isLycanthrope) {
            hasLycan = true;
            lycanData = new SaveDataLycanthropeData();
            lycanData.Save(data.lycanData);
        }

        if(data.grave != null) {
            grave = data.grave.persistentID;
        }
        if (data.deathLog != null) {
            deathLog = data.deathLog;
            // SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.deathLog);
        }
        if (data.homeRegion != null) {
            homeRegion = data.homeRegion.persistentID;
        }
        if (data.homeSettlement != null) {
            homeSettlement = data.homeSettlement.persistentID;
        }
        if (data.homeStructure != null) {
            homeStructure = data.homeStructure.persistentID;
        }
        if (data.currentRegion != null) {
            currentRegion = data.currentRegion.persistentID;
        }
        if (data.currentStructure != null) {
            currentStructure = data.currentStructure.persistentID;
        }
        if (data.faction != null) {
            faction = data.faction.persistentID;
        }

        territory = string.Empty;
        if (data.HasTerritory()) {
            territory = data.territory.persistentID;
        }

        items = new List<string>();
        for (int i = 0; i < data.items.Count; i++) {
            items.Add(data.items[i].persistentID);
        }
        equipmentInventory = new List<string>();
        for (int i = 0; i < data.equipmentInventory.Count; i++) {
            equipmentInventory.Add(data.equipmentInventory[i].persistentID);
        }
        ownedItems = new List<string>();
        for (int i = 0; i < data.ownedItems.Count; i++) {
            ownedItems.Add(data.ownedItems[i].persistentID);
        }
        
        jobs = new List<string>();
        for (int i = 0; i < data.jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem jobQueueItem = data.jobQueue.jobsInQueue[i];
            jobs.Add(jobQueueItem.persistentID);
        }
        
        forceCancelJobsOnTickEnded = new List<string>();
        for (int i = 0; i < data.forcedCancelJobsOnTickEnded.Count; i++) {
            JobQueueItem jobQueueItem = data.forcedCancelJobsOnTickEnded[i];
            if(jobQueueItem.jobType != JOB_TYPE.NONE) {
                forceCancelJobsOnTickEnded.Add(jobQueueItem.persistentID);
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(jobQueueItem);
            }
        }

        if (data.deployedAtStructure != null) {
            deployedAtStructure = data.deployedAtStructure.persistentID;
        }

        saveDataTraitContainer = new SaveDataTraitContainer();
        saveDataTraitContainer.Save(data.traitContainer);
        
        saveDataBaseRelationshipContainer = new SaveDataBaseRelationshipContainer();
        saveDataBaseRelationshipContainer.Save(data.relationshipContainer as BaseRelationshipContainer);
    }
    public override Character Load() {
        Character character = CharacterManager.Instance.CreateNewCharacter(this);
        return character;
    }
}
