using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

[System.Serializable]
public class SaveDataCharacter : SaveData<Character> {
    public int id;
    public string name;
    public string firstName;
    private string surName;
    public bool isDead;
    public GENDER gender;
    public SEXUALITY sexuality;
    public string className;
    public RACE race;
    //public int factionID; //Will no longer save faction of character since the SaveDataFaction already holds the characters inside faction
    public bool isAlliedWithPlayer;

    public int currentHP;
    public int doNotRecoverHP;
    public int attackPowerMod;
    public int speedMod;
    public int maxHPMod;
    public int attackPowerPercentMod;
    public int speedPercentMod;
    public int maxHPPercentMod;

    //private LocationStructure _currentStructure; //Will no longer save current structure because the saved location grid tile coordinates will be saved, and once you place the marker on the grid tile it will automatically set the current structure
    public int currentRegionID;
    public int currentLocationGridTileX;
    public int currentLocationGridTileY;

    public PortraitSettings portraitSettings;

    public SaveDataMinion minion;

    public int homeRegionID;
    public int homeSettlementID;
    public int homeStructureID;
    public List<INTERACTION_TYPE> advertisedActions;
    public List<int> itemsID;
    public List<int> ownedItemsID;
    public bool canCombat;
    public bool isDisabledByPlayer;
    public string deathStr;
    public int prevFactionID;
    public Dictionary<RESOURCE, int> storedResources;
    public bool hasUnresolvedCrime;
    public bool isInLimbo;
    public bool isLimboCharacter;
    public bool destroyMarkerOnDeath;
    public bool isWanderer;
    public bool hasRisen;
    public int deathLogID;
    public List<string> interestedItemNames;

    public List<int> territoryIDs;
    public NPCSettlement ruledSettlement;

    public POI_STATE state;
    public int canWitnessValue;
    public int canMoveValue;
    public int canBeAttackedValue;
    public int canPerformValue;
    public int canTakeJobsValue;
    public int sociableValue;

    public bool returnedToLife;

    public SaveDataLycanthropeData lycanData;

    public List<SaveDataLog> history;

    public override void Save(Character character) {
        id = character.id;
        name = character.name;

        isDead = character.isDead;
        gender = character.gender;
        sexuality = character.sexuality;
        className = character.characterClass.className;
        race = character.race;
        // roleType = character.role.roleType;
        portraitSettings = character.visuals.portraitSettings;
        //characterColor = character.characterColor;
    //    isStoppedByOtherCharacter = character.numOfActionsBeingPerformedOnThis;

    //    normalTraits = new List<SaveDataTrait>();
    //    for (int i = 0; i < character.traitContainer.allTraitsAndStatuses.Count; i++) {
    //        Trait trait = character.traitContainer.allTraitsAndStatuses[i];

    //        SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(trait);
    //        if (saveDataTrait != null) {
    //            saveDataTrait.Save(trait);
    //            normalTraits.Add(saveDataTrait);
    //        }
    //    }

    //    currentHP = character.currentHP;
    //    maxHP = character.maxHP;
    //    //level = character.level;
    //    //experience = character.experience;
    //    //maxExperience = character.maxExperience;
    //    attackPowerMod = character.attackPowerMod;
    //    speedMod = character.speedMod;
    //    maxHPMod = character.maxHPMod;
    //    attackPowerPercentMod = character.attackPowerPercentMod;
    //    speedPercentMod = character.speedPercentMod;
    //    maxHPPercentMod = character.maxHPPercentMod;

    //    //currentInteractionTypes = character.currentInteractionTypes;
    //    supply = character.supply;
    //    moodValue = character.moodComponent.moodValue;
    //    isCombatant = character.canCombat;
    //    isDisabledByPlayer = character.isDisabledByPlayer;
    //    //speedModifier = character.speedModifier;
    //    deathStr = character.deathStr;

    //    state = character.state;

    //    // items = new List<SaveDataItem>();
    //    // for (int i = 0; i < character.items.Count; i++) {
    //    //     SaveDataItem newSaveDataItem = new SaveDataItem();
    //    //     newSaveDataItem.Save(character.items[i]);
    //    //     items.Add(newSaveDataItem);
    //    // }

    //    tiredness = character.needsComponent.tiredness;
    //    fullness = character.needsComponent.fullness;
    //    happiness = character.needsComponent.happiness;
    //    fullnessDecreaseRate = character.needsComponent.fullnessDecreaseRate;
    //    tirednessDecreaseRate = character.needsComponent.tirednessDecreaseRate;
    //    happinessDecreaseRate = character.needsComponent.happinessDecreaseRate;

    //    //originalClassName = character.originalClassName;
    //    isMinion = character.minion != null;
    //    isSummon = character is Summon;
    //    if (isSummon) {
    //        Summon summon = character as Summon;
    //        summonType = summon.summonType;
    //    }

    //    //currentAlterEgoName = character.currentAlterEgoName;
    //    //alterEgos = new List<SaveDataAlterEgo>();
    //    //foreach (AlterEgoData alterEgo in character.alterEgos.Values) {
    //    //    SaveDataAlterEgo saveDataAlterEgo = new SaveDataAlterEgo();
    //    //    saveDataAlterEgo.Save(alterEgo);
    //    //    alterEgos.Add(saveDataAlterEgo);
    //    //}

    //    fullnessForcedTick = character.needsComponent.fullnessForcedTick;
    //    tirednessForcedTick = character.needsComponent.tirednessForcedTick;
    //    forcedFullnessRecoveryTimeInWords = character.needsComponent.forcedFullnessRecoveryTimeInWords;
    //    forcedTirednessRecoveryTimeInWords = character.needsComponent.forcedTirednessRecoveryTimeInWords;

    //    returnedToLife = character.returnedToLife;

    //    if (character.gridTileLocation != null) {
    //        gridTileLocation = new Vector3Save(character.gridTileLocation.localPlace.x, character.gridTileLocation.localPlace.y, 0f);
    //        gridTileLocationAreaID = character.gridTileLocation.structure.location.id; //TODO: Refactor this, because structure location is no longer guaranteed to be an npcSettlement.
    //    } else {
    //        gridTileLocation = new Vector3Save(0f, 0f, -1f);
    //    }

    //    if (character.marker) {
    //        lethalHostilesInRangeIDs = new List<int>();
    //        nonLethalHostilesInRangeIDs = new List<int>();
    //        avoidInRangeIDs = new List<int>();

    //        for (int i = 0; i < character.combatComponent.hostilesInRange.Count; i++) {
    //            IPointOfInterest poi = character.combatComponent.hostilesInRange[i];
    //            if (poi is Character) {
    //                Character hostile = poi as Character;
    //                if (character.combatComponent.IsLethalCombatForTarget(hostile)) {
    //                    lethalHostilesInRangeIDs.Add(hostile.id);
    //                } else {
    //                    nonLethalHostilesInRangeIDs.Add(hostile.id);
    //                }
    //            }
    //        }
    //        for (int i = 0; i < character.combatComponent.avoidInRange.Count; i++) {
    //            avoidInRangeIDs.Add(character.combatComponent.avoidInRange[i].id);
    //        }
    //    }

    //    hasCurrentState = character.stateComponent.currentState != null && character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT && character.stateComponent.currentState.job == null;
    //    if (hasCurrentState) {
    //        currentState = SaveUtilities.CreateCharacterStateSaveDataInstance(character.stateComponent.currentState);
    //        currentState.Save(character.stateComponent.currentState);
    //    }

    //    jobs = new List<SaveDataJobQueueItem>();
    //    for (int i = 0; i < character.jobQueue.jobsInQueue.Count; i++) {
    //        JobQueueItem job = character.jobQueue.jobsInQueue[i];
    //        if (job.isNotSavable) {
    //            continue;
    //        }
    //        //SaveDataJobQueueItem data = System.Activator.CreateInstance(System.Type.GetType("SaveData" + job.GetType().ToString())) as SaveDataJobQueueItem;
    //        SaveDataJobQueueItem data = null;
    //        if(job is GoapPlanJob) {
    //            data = new SaveDataGoapPlanJob();
    //        }else if(job is CharacterStateJob) {
    //            data = new SaveDataCharacterStateJob();
    //        }
    //        data.Save(job);
    //        jobs.Add(data);
    //    }

    //    currentSleepTicks = character.needsComponent.currentSleepTicks;
    //    sleepScheduleJobID = character.needsComponent.sleepScheduleJobID;
    //    hasCancelledSleepSchedule = character.needsComponent.hasCancelledSleepSchedule;

    //    history = new List<SaveDataLog>();
    //    for (int i = 0; i < character.logComponent.history.Count; i++) {
    //        SaveDataLog data = new SaveDataLog();
    //        data.Save(character.logComponent.history[i]);
    //        history.Add(data);
    //    }
    //}

    //public void Load() {
    //    Character character = null;
    //    if (isSummon) {
    //        //character = CharacterManager.Instance.CreateNewSummon(this);
    //    } else {
    //        character = CharacterManager.Instance.CreateNewCharacter(this);
    //    }
    //    if (!isMinion && !isSummon && isDead) {
    //        //Do not process dead save data if character is a minion or summon, there is a separate process for that in Player
    //        //character.ownParty.PartyDeath();
    //        Region deathLocation = character.currentRegion;
    //        LocationStructure deathStructure = character.currentStructure;
    //        deathLocation?.RemoveCharacterFromLocation(character);
    //        character.SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
    //        character.SetCurrentStructureLocation(deathStructure, false);

    //        // if (character.role != null) {
    //        //     character.role.OnDeath(character);
    //        // }
    //    }
    }

    public void LoadTraits(Character character) {
        //for (int i = 0; i < normalTraits.Count; i++) {
        //    Character responsibleCharacter = null;
        //    Trait trait = normalTraits[i].Load(ref responsibleCharacter);
        //    character.traitContainer.AddTrait(character, trait, responsibleCharacter);
        //}
        //character.LoadAllStatsOfCharacter(this);
    }

    public void LoadRelationships(Character character) {
        //for (int i = 0; i < alterEgos.Count; i++) {
        //    alterEgos[i].LoadRelationships(character);
        //}
    }

    public void LoadHomeStructure(Character character) {
        if (homeStructureID != -1) {
            // NPCSettlement npcSettlement = LandmarkManager.Instance.GetAreaByID(homeStructureAreaID);
            // LocationStructure structure = npcSettlement.GetStructureByID(homeStructureType, homeStructureID);
            // character.MigrateHomeStructureTo(structure as IDwelling);
        }
    }

    public void LoadCharacterGridTileLocation(Character character) {
        //if (gridTileLocation.z != -1f) {
            // NPCSettlement npcSettlement = LandmarkManager.Instance.GetAreaByID(gridTileLocationAreaID);
            // LocationGridTile gridTile = npcSettlement.innerMap.map[(int) gridTileLocation.x, (int) gridTileLocation.y];
            //
            // if (!character.marker) {
            //     character.CreateMarker();
            // }
            // character.LoadInitialCharacterPlacement(gridTile);
            //
            // for (int i = 0; i < lethalHostilesInRangeIDs.Count; i++) {
            //     Character target = CharacterManager.Instance.GetCharacterByID(lethalHostilesInRangeIDs[i]);
            //     character.combatComponent.Fight(target, isLethal: true);
            // }
            // for (int i = 0; i < nonLethalHostilesInRangeIDs.Count; i++) {
            //     Character target = CharacterManager.Instance.GetCharacterByID(nonLethalHostilesInRangeIDs[i]);
            //     character.combatComponent.Fight(target, isLethal: false);
            // }
            // for (int i = 0; i < avoidInRangeIDs.Count; i++) {
            //     Character target = CharacterManager.Instance.GetCharacterByID(avoidInRangeIDs[i]);
            //     character.combatComponent.Flight(target);
            // }
            //
            // if (character.isDead) {
            //     character.marker.OnDeath(gridTile);
            // }
        //}
    }

    /// <summary>
    /// Load the characters current state. NOTE: This should only load states that are not combat and are not part of jobs.
    /// </summary>
    /// <param name="character"></param>
    public void LoadCharacterCurrentState(Character character) {
        //if (hasCurrentState) {
        //    Character targetCharacter = null;
        //    NPCSettlement targetNpcSettlement = null;
        //    if (currentState.targetCharacterID != -1) {
        //        targetCharacter = CharacterManager.Instance.GetCharacterByID(currentState.targetCharacterID);
        //    }
        //    //if (currentState.targetAreaID != -1) {
        //    //    targetNpcSettlement = LandmarkManager.Instance.GetAreaByID(currentState.targetAreaID);
        //    //}
        //    //CharacterState loadedState = character.stateComponent.SwitchToState(currentState.characterState, targetCharacter, targetNpcSettlement, currentState.duration, currentState.level);
        //    //loadedState.SetCurrentDuration(currentState.currentDuration);
        //    ////loadedState.SetIsUnending(currentState.isUnending);

        //    //if (currentState.isPaused) {
        //    //    loadedState.PauseState();
        //    //}
        //}
    }

    public void LoadCharacterJobs(Character character) {
        //for (int i = 0; i < jobs.Count; i++) {
        //    JobQueueItem job = jobs[i].Load();
        //    character.jobQueue.AddJobInQueue(job);

        //    //if (jobs[i] is SaveDataCharacterStateJob) {
        //    //    SaveDataCharacterStateJob dataStateJob = jobs[i] as SaveDataCharacterStateJob;
        //    //    CharacterStateJob stateJob = job as CharacterStateJob;
        //    //    if (dataStateJob.assignedCharacterID != -1) {
        //    //        Character assignedCharacter = CharacterManager.Instance.GetCharacterByID(dataStateJob.assignedCharacterID);
        //    //        stateJob.SetAssignedCharacter(assignedCharacter);
        //    //        CharacterState newState = assignedCharacter.stateComponent.SwitchToState(stateJob.targetState, null, stateJob.targetNpcSettlement);
        //    //        if (newState != null) {
        //    //            stateJob.SetAssignedState(newState);
        //    //        } else {
        //    //            throw new System.Exception(assignedCharacter.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
        //    //        }
        //    //    }
        //    //}
        //}
    }

    public void LoadCharacterHistory(Character character) {
        for (int i = 0; i < history.Count; i++) {
            character.logComponent.AddHistory(history[i].Load());
        }
    }
}
