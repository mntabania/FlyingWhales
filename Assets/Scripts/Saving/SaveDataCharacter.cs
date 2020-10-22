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
    public string name;
    public string firstName;
    public string surName;
    public bool isDead;
    public GENDER gender;
    public SEXUALITY sexuality;
    public string className;
    public RACE race;
    public bool isAlliedWithPlayer;
    public string previousClassName;

    public int currentHP;
    public int doNotRecoverHP;
    public int attackPowerMod;
    public int speedMod;
    public int maxHPMod;
    public int attackPowerPercentMod;
    public int speedPercentMod;
    public int maxHPPercentMod;

    public Vector3 worldPos;
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
    public Dictionary<RESOURCE, int> storedResources;
    public bool hasUnresolvedCrime;
    public bool isInLimbo;
    public bool isLimboCharacter;
    public bool destroyMarkerOnDeath;
    public bool isWanderer;
    public bool hasRisen;
    public bool isPreplaced;
    public List<string> interestedItemNames;

    public POI_STATE state;
    public int canWitnessValue;
    public int canMoveValue;
    public int canBeAttackedValue;
    public int canPerformValue;
    public int canTakeJobsValue;
    public int sociableValue;

    public bool raisedFromDeadAsSkeleton;

    public SaveDataLycanthropeData lycanData;
    public bool hasLycan;

    //References
    public string grave;
    public string ruledSettlement;
    public Log deathLog;
    public string homeRegion;
    public string homeSettlement;
    public string homeStructure;
    public string currentRegion;
    public string currentStructure;
    public string faction;
    public string prevFaction;

    public string currentJob;
    public string currentActionNode;
    public string previousCurrentActionNode;

    public List<string> territories;
    public List<string> items;
    public List<string> ownedItems;
    public List<string> jobs;
    public List<string> forceCancelJobsOnTickEnded;
    
    public SaveDataTraitContainer saveDataTraitContainer;
    public SaveDataBaseRelationshipContainer saveDataBaseRelationshipContainer;

    public SaveDataTrapStructure trapStructure;
    public SaveDataCharacterNeedsComponent needsComponent;
    public SaveDataBuildStructureComponent buildStructureComponent;
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
    public SaveDataPartyComponent partyComponent;
    public SaveDataGatheringComponent gatheringComponent;
    public SaveDataTileObjectComponent tileObjectComponent;
    public SaveDataCrimeComponent crimeComponent;
    public SaveDataReligionComponent religionComponent;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Character;
    #endregion

    public override void Save(Character data) {
        persistentID = data.persistentID;
        id = data.id;
        name = data.name;
        firstName = data.firstName;
        surName = data.surName;
        isDead = data.isDead;
        gender = data.gender;
        sexuality = data.sexuality;
        className = data.characterClass.className;
        race = data.race;
        isAlliedWithPlayer = data.isAlliedWithPlayer;
        currentHP = data.currentHP;
        doNotRecoverHP = data.doNotRecoverHP;
        attackPowerMod = data.attackPowerMod;
        speedMod = data.speedMod;
        maxHPMod = data.maxHPMod;
        attackPowerPercentMod = data.attackPowerPercentMod;
        speedPercentMod = data.speedPercentMod;
        maxHPPercentMod = data.maxHPPercentMod;
        portraitSettings = data.visuals.portraitSettings;
        advertisedActions = data.advertisedActions;
        canCombat = data.canCombat;
        deathStr = data.deathStr;
        storedResources = data.storedResources;
        hasUnresolvedCrime = data.hasUnresolvedCrime;
        isInLimbo = data.isInLimbo;
        isLimboCharacter = data.isLimboCharacter;
        destroyMarkerOnDeath = data.destroyMarkerOnDeath;
        isWanderer = data.isWanderer;
        hasRisen = data.hasRisen;
        interestedItemNames = data.interestedItemNames;
        state = data.state;
        canWitnessValue = data.canWitnessValue;
        canMoveValue = data.canMoveValue;
        canBeAttackedValue = data.canBeAttackedValue;
        canPerformValue = data.canPerformValue;
        canTakeJobsValue = data.canTakeJobsValue;
        sociableValue = data.sociableValue;
        raisedFromDeadAsSkeleton = data.raisedFromDeadAsSkeleton;
        previousClassName = data.previousClassName;
        isPreplaced = data.isPreplaced;

        if (data.marker) {
            hasMarker = true;
            worldPos = data.marker.transform.position;
            rotation = data.marker.visualsParent.transform.localRotation;

            if (data.marker.hasExpiry) {
                hasExpiry = true;
                markerExpiryDate = data.marker.destroyDate;
            }
        }

        trapStructure = new SaveDataTrapStructure(); trapStructure.Save(data.trapStructure);
        needsComponent = new SaveDataCharacterNeedsComponent(); needsComponent.Save(data.needsComponent);
        buildStructureComponent = new SaveDataBuildStructureComponent(); buildStructureComponent.Save(data.buildStructureComponent);
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
        partyComponent = new SaveDataPartyComponent(); partyComponent.Save(data.partyComponent);
        gatheringComponent = new SaveDataGatheringComponent(); gatheringComponent.Save(data.gatheringComponent);
        tileObjectComponent = new SaveDataTileObjectComponent(); tileObjectComponent.Save(data.tileObjectComponent);
        crimeComponent = new SaveDataCrimeComponent(); crimeComponent.Save(data.crimeComponent);
        religionComponent = new SaveDataReligionComponent(); religionComponent.Save(data.religionComponent);            

        if(data.currentJob != null && data.currentJob.jobType != JOB_TYPE.NONE) {
            currentJob = data.currentJob.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentJob);
        }

        if (data.currentActionNode != null) {
            currentActionNode = data.currentActionNode.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentActionNode);
        }

        if (data.previousCurrentActionNode != null) {
            previousCurrentActionNode = data.previousCurrentActionNode.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.previousCurrentActionNode);
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
        if (data.ruledSettlement != null) {
            ruledSettlement = data.ruledSettlement.persistentID;
        }
        if (data.deathLog.hasValue) {
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
        if (data.prevFaction != null) {
            prevFaction = data.prevFaction.persistentID;
        }

        territories = new List<string>();
        for (int i = 0; i < data.territories.Count; i++) {
            territories.Add(data.territories[i].persistentID);
        }
        items = new List<string>();
        for (int i = 0; i < data.items.Count; i++) {
            items.Add(data.items[i].persistentID);
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
