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
    public List<string> interestedItemNames;

    public POI_STATE state;
    public int canWitnessValue;
    public int canMoveValue;
    public int canBeAttackedValue;
    public int canPerformValue;
    public int canTakeJobsValue;
    public int sociableValue;

    public bool returnedToLife;

    public SaveDataLycanthropeData lycanData;
    public bool hasLycan;

    //References
    public string grave;
    public string ruledSettlement;
    public string deathLog;
    public string homeRegion;
    public string homeSettlement;
    public string homeStructure;
    public string currentRegion;
    public string currentStructure;
    public string faction;
    public string prevFaction;

    public List<string> territories;
    public List<string> items;
    public List<string> ownedItems;

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
    public SaveDataTileObjectComponent tileObjectComponent;
    public SaveDataCrimeComponent crimeComponent;

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
        returnedToLife = data.returnedToLife;

        if (data.marker) {
            hasMarker = true;
            worldPos = data.marker.transform.position;
            rotation = data.marker.transform.localRotation;
        }
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
        tileObjectComponent = new SaveDataTileObjectComponent(); tileObjectComponent.Save(data.tileObjectComponent);
        crimeComponent = new SaveDataCrimeComponent(); crimeComponent.Save(data.crimeComponent);

        if (data.minion != null) {
            hasMinion = true;
            SaveDataMinion saveMinion = new SaveDataMinion();
            saveMinion.Save(data.minion);
        }
        if(data.lycanData != null) {
            hasLycan = true;
            SaveDataLycanthropeData saveLycan = new SaveDataLycanthropeData();
            saveLycan.Save(data.lycanData);
        }

        if(data.grave != null) {
            grave = data.grave.persistentID;
        }
        if (data.ruledSettlement != null) {
            ruledSettlement = data.ruledSettlement.persistentID;
        }
        if (data.deathLog != null) {
            deathLog = data.deathLog.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.deathLog);
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
    }
    public override Character Load() {
        Character character = CharacterManager.Instance.CreateNewCharacter(this);
        return base.Load();
    }
}
