using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Factions.Faction_Types;
using Locations.Settlements;
using Traits;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class Faction : IJobOwner, ISavable, ILogFiller {
    
    public const int MAX_HISTORY_LOGS = 60;

    public string persistentID { get; }
    public int id { get; }
    public string name { get; private set; }
    public string description { get; private set; }
    public bool isMajorFaction { get; private set; }
    public ILeader leader { get; private set; }
    public Sprite emblem { get; private set; }
    public Color factionColor { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; }
    public List<Character> characters { get; }//List of characters that are part of the faction
    public List<BaseSettlement> ownedSettlements { get; }
    public List<Character> bannedCharacters { get; }
    public Dictionary<Faction, FactionRelationship> relationships { get; }
    public FactionType factionType { get; protected set; }
    public bool isActive { get; private set; }
    // public List<Log> history { get; }
    public List<JobQueueItem> availableJobs { get; }
    public FactionIdeologyComponent ideologyComponent { get; }
    public FactionJobTriggerComponent factionJobTriggerComponent { get; private set; }
    public PartyQuestBoard partyQuestBoard { get; private set; }
    public int newLeaderDesignationChance { get; private set; }
    public uint pathfindingTag { get; private set; }
    public uint pathfindingDoorTag { get; private set; }
    
    private readonly WeightedDictionary<Character> newLeaderDesignationWeights;

    public Heirloom factionHeirloom { get; private set; }

    #region getters/setters
    public bool isDestroyed => characters.Count <= 0;
    public bool isMajorOrVagrant => isMajorFaction || this == FactionManager.Instance.vagrantFaction;
    public bool isMajorNonPlayerOrVagrant => isMajorNonPlayer || this == FactionManager.Instance.vagrantFaction;
    public bool isMajorNonPlayer => isMajorFaction && !isPlayerFaction;
    public bool isNonMajorOrPlayer => !isMajorFaction || isPlayerFaction;
    public JobTriggerComponent jobTriggerComponent => factionJobTriggerComponent;
    public bool isPlayerFaction => factionType.type == FACTION_TYPE.Demons;
    public JOB_OWNER ownerType => JOB_OWNER.FACTION;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Faction;
    public System.Type serializedData => typeof(SaveDataFaction);
    public RACE race {
        get {
            switch (factionType.type) {
                case FACTION_TYPE.Elven_Kingdom:
                    return RACE.ELVES;
                case FACTION_TYPE.Human_Empire:
                    return RACE.HUMANS;
                case FACTION_TYPE.Demons:
                    return RACE.DEMON;
                default:
                    if (leader is Character character) {
                        return character.race;
                    }
                    return RACE.HUMANS;
            }
        }
    }
    #endregion

    public Faction(FACTION_TYPE _factionType) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        id = UtilityScripts.Utilities.SetID(this);
        SetName(RandomNameGenerator.GenerateKingdomName());
        // SetEmblem(FactionManager.Instance.GenerateFactionEmblem(this));
        SetFactionColor(UtilityScripts.Utilities.GetColorForFaction());
        SetFactionActiveState(true);
        SetFactionType(_factionType);
        //factionType = FactionManager.Instance.CreateFactionType(_factionType);
        characters = new List<Character>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        // history = new List<Log>();
        availableJobs = new List<JobQueueItem>();
        newLeaderDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        ideologyComponent = new FactionIdeologyComponent(this);
        factionJobTriggerComponent = new FactionJobTriggerComponent(this);
        partyQuestBoard = new PartyQuestBoard(this);
        ResetNewLeaderDesignationChance();
        AddListeners();
    }
    public Faction(SaveDataFaction data) {
        persistentID = data.persistentID;
        id = UtilityScripts.Utilities.SetID(this, data.id);
        ideologyComponent = new FactionIdeologyComponent(this);
        factionJobTriggerComponent = new FactionJobTriggerComponent(this);
        factionType = data.factionType.Load();
        partyQuestBoard = data.partyQuestBoard.Load();

        name = data.name;
        description = data.description;
        emblem = FactionManager.Instance.GetFactionEmblem(data.emblemName);
        factionColor = data.factionColor;
        isActive = data.isActive;
        isMajorFaction = data.isMajorFaction;
        newLeaderDesignationChance = data.newLeaderDesignationChance;
        pathfindingTag = data.pathfindingTag;
        pathfindingDoorTag = data.pathfindingDoorTag;

        characters = new List<Character>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        // history = new List<Log>();
        availableJobs = new List<JobQueueItem>();
        newLeaderDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();

        AddListeners();
    }

    #region Characters
    public bool JoinFaction(Character character, bool broadcastSignal = true, bool bypassIdeologyChecking = false, bool isInitial = false) {
        if (bypassIdeologyChecking || ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
            Faction prevFaction = character.prevFaction;
            if (AddCharacter(character)) {
                //Once a character joins any faction and the faction is not the owner of the character's current home settlement, leave the settlement also
                //Reason: One village = One faction, no other faction can co exist in a village, for simplification
                if (character.homeSettlement != null && character.homeSettlement.owner != null && character.homeSettlement.owner != this) {
                    character.MigrateHomeStructureTo(null);
                }

                if (!isInitial) {
                    //Whenever a character joins a new faction, add Transitioning status, so that if his new faction is hostile with his previous faction he will not be attacked immediately by the people of his previous faction
                    character.traitContainer.AddTrait(character, "Transitioning");
                }

                if (broadcastSignal) {
                    Messenger.Broadcast(Signals.CHARACTER_ADDED_TO_FACTION, character, this);
                }
            }
            return true;
        }
        return false;
    }
    public bool LeaveFaction(Character character) {
        if (characters.Remove(character)) {
            if (leader == character) {
                SetLeader(null); //so a new leader can be set if the leader is ever removed from the list of characters of this faction
            }
            character.SetFaction(null);
            Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_FACTION, character, this);
            return true;
        }
        return false;
    }
    public bool AddCharacter(Character character) {
        if (!characters.Contains(character)) {
            characters.Add(character);
            character.SetFaction(this);
            if (isPlayerFaction && character is Summon summon) {
                Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON, summon);
            }
            return true;
        }
        return false;
    }
    public void OnlySetLeader(ILeader newLeader) {
        if(leader != newLeader) {
            ILeader prevLeader = leader;
            leader = newLeader;
            if(prevLeader != null && prevLeader is Character prevCharacterLeader) {
                if(isMajorNonPlayer) {
                    prevCharacterLeader.behaviourComponent.RemoveBehaviourComponent(typeof(FactionLeaderBehaviour));
                    if (!prevCharacterLeader.isSettlementRuler) {
                        prevCharacterLeader.jobComponent.RemovePriorityJob(JOB_TYPE.JUDGE_PRISONER);
                    }
                }
            }
            if(leader != null) {
                if(leader is Character characterLeader) {
                    if (isMajorNonPlayer) {
                        characterLeader.behaviourComponent.AddBehaviourComponent(typeof(FactionLeaderBehaviour));
                        characterLeader.jobComponent.AddPriorityJob(JOB_TYPE.JUDGE_PRISONER);
                    }
                }
            }
            if (newLeader is Character character) {
                Messenger.Broadcast(Signals.ON_SET_AS_FACTION_LEADER, character, prevLeader);
            } else if (newLeader == null) {
                Messenger.Broadcast(Signals.ON_FACTION_LEADER_REMOVED, this, prevLeader);
            }
        }
    }
    private void OnCharacterRaceChange(Character character) {
        CheckIfCharacterStillFitsIdeology(character);
    }
    private void OnCharacterRemoved(Character character) {
        LeaveFaction(character);
    }
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (character == leader && trait is Cultist) {
            //leader became cultist
            ideologyComponent.OnLeaderBecameCultist(character);
        }
    }
    //Returns true if character left the faction, otherwise return false
    public void CheckIfCharacterStillFitsIdeology(Character character, bool willLog = true) {
        if (character.faction == this && !ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
            if (willLog) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_not_fit");
            } else {
                character.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
            }
            //character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
            //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "left_faction_not_fit");
            //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
            //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
    }
    public bool IsCharacterBannedFromJoining(Character character) {
        return HasCharacterBeenBanned(character);
    }
    private bool HasCharacterBeenBanned(Character character) {
        for (int i = 0; i < bannedCharacters.Count; i++) {
            if (character == bannedCharacters[i]) {
                return true;
            }
        }
        return false;
    }
    public void AddBannedCharacter(Character character) {
        if (!HasCharacterBeenBanned(character)) {
            bannedCharacters.Add(character);
        }
    }
    public bool RemoveBannedCharacter(Character character) {
        return bannedCharacters.Remove(character);
    }
    public void KickOutCharacter(Character character) {
        if(character.faction == this) {
            AddBannedCharacter(character);

            character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "kick_out_faction_character");
            //character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
            //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "kick_out_faction_character");
            //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
            //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
    }
    private void OnCharacterMissing(Character missingCharacter) {
        if (leader != null && missingCharacter == leader) {
            SetLeader(null);
        }
    }
    private void OnCharacterDied(Character deadCharacter) {
        if (leader != null && deadCharacter == leader) {
            SetLeader(null);
        }
    }
    public void SetLeader(ILeader newLeader) {
        if(!isMajorFaction && !isPlayerFaction) {
            //Neutral, Friendly Neutral, Disguised Factions cannot have a leader
            return;
        }
        OnlySetLeader(newLeader);

        if (newLeader != null) {
            if (newLeader is Character newRuler) {
                newRuler.currentRegion?.AddFactionHere(this);
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
            }
        } else {
            //if no leader was set, then roll every hour for a chance to designate a new leader.
            Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
        }
    }
    private void CheckForNewLeaderDesignation() {
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new faction leader designation for {name}";
        debugLog += $"\n-Chance: {newLeaderDesignationChance.ToString()}";
        int chance = Random.Range(0, 100);
        debugLog += $"\n-Roll: {chance.ToString()}";
        Debug.Log(debugLog);
        // chance = 0;
        if (chance < newLeaderDesignationChance) {
            DesignateNewLeader();
        } else {
            newLeaderDesignationChance += 2;
        }
    }
    public void DesignateNewLeader(bool willLog = true) {
        string log = $"Designating a new npcSettlement faction leader for: {name}(chance it triggered: {newLeaderDesignationChance.ToString()})";
        newLeaderDesignationWeights.Clear();
        for (int i = 0; i < characters.Count; i++) {
            Character member = characters[i];
            log += $"\n\n-{member.name}";
            if (member.isDead /*|| member.isMissing*/ || member.isBeingSeized || member.isInLimbo) {
                log += "\nEither dead, missing, in limbo or seized, will not be part of candidates for faction leader";
                continue;
            }

            if (member.crimeComponent.IsWantedBy(this)) {
                log += "\nMember is wanted by this faction, skipping...";
                continue;
            }
            int weight = 50;
            log += "\n  -Base Weight: +50";
            if (factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires)) {
                Vampire vampire = member.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampire != null && vampire.DoesFactionKnowThisVampire(this)) {
                    weight += 100;
                    log += "\n  -Faction reveres vampires and member is a known vampire: +100";
                }
            }
            if (factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                if (member.isLycanthrope && member.lycanData.DoesFactionKnowThisLycan(this)) {
                    weight += 100;
                    log += "\n  -Faction reveres werewolves and member is a known Lycanthrope: +100";
                }
            }
            if (member.isSettlementRuler) {
                weight += 30;
                log += "\n  -NPCSettlement Ruler: +30";
            }
            if (member.characterClass.className == "Noble") {
                weight += 40;
                log += "\n  -Noble: +40";
            }
            int numberOfFriends = 0;
            int numberOfEnemies = 0;
            for (int j = 0; j < member.relationshipContainer.charactersWithOpinion.Count; j++) {
                Character otherCharacter = member.relationshipContainer.charactersWithOpinion[j];
                if (otherCharacter.faction == this) {
                    if (otherCharacter.relationshipContainer.IsFriendsWith(member)) {
                        numberOfFriends++;
                    } else if (otherCharacter.relationshipContainer.IsEnemiesWith(member)) {
                        numberOfEnemies++;
                    }
                }
            }
            if (numberOfFriends > 0) {
                int weightToAdd = 0;
                if (member.traitContainer.HasTrait("Worker")) {
                    weightToAdd = Mathf.FloorToInt((numberOfFriends * 20) * 0.2f);
                } else {
                    weightToAdd = (numberOfFriends * 20);    
                }
                weight += weightToAdd;
                log += $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{weightToAdd}";
            }
            if (member.traitContainer.HasTrait("Inspiring")) {
                weight += 25;
                log += "\n  -Inspiring: +25";
            }
            if (member.traitContainer.HasTrait("Authoritative")) {
                weight += 50;
                log += "\n  -Authoritative: +50";
            }


            if (numberOfEnemies > 0) {
                weight += (numberOfEnemies * -10);
                log += $"\n  -Num of Enemies/Rivals in the NPCSettlement: {numberOfEnemies}, +{(numberOfEnemies * -10)}";
            }
            if (member.traitContainer.HasTrait("Unattractive")) {
                weight += -20;
                log += "\n  -Unattractive: -20";
            }
            if (member.hasUnresolvedCrime) {
                weight += -50;
                log += "\n  -Has Unresolved Crime: -50";
            }
            if (member.traitContainer.HasTrait("Worker")) {
                weight += -40;
                log += "\n  -Civilian: -40";
            }
            if (member.traitContainer.HasTrait("Ambitious")) {
                weight = Mathf.RoundToInt(weight * 1.5f);
                log += "\n  -Ambitious: x1.5";
            }
            if(weight < 1) {
                weight = 1;
                log += "\n  -Weight cannot be less than 1, setting weight to 1";
            }
            log += $"\n  -TOTAL WEIGHT: {weight}";
            if (weight > 0) {
                newLeaderDesignationWeights.AddElement(member, weight);
            }
        }
        if (newLeaderDesignationWeights.Count > 0) {
            Character chosenLeader = newLeaderDesignationWeights.PickRandomElementGivenWeights();
            if (chosenLeader != null) {
                log += $"\nCHOSEN LEADER: {chosenLeader.name}";
                if (willLog) {
                    chosenLeader.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, chosenLeader);
                } else {
                    SetLeader(chosenLeader);
                }
            } else {
                log += "\nCHOSEN LEADER: NONE";
            }
        } else {
            log += "\nCHOSEN LEADER: NONE";
        }
        ResetNewLeaderDesignationChance();
        Debug.Log(GameManager.Instance.TodayLogString() + log);
    }
    private void ResetNewLeaderDesignationChance() {
        newLeaderDesignationChance = 5;
    }
    public void SetNewLeaderDesignationChance(int amount) {
        newLeaderDesignationChance = amount;
    }
    public void GenerateInitialOpinionBetweenMembers() {
        for (int i = 0; i < characters.Count; i++) {
            Character character1 = characters[i];
            for (int j = 0; j < characters.Count; j++) {
                Character character2 = characters[j];
                if(character1 != character2) {
                    character1.relationshipContainer.AdjustOpinion(character1, character2, "Base", 0);
                }
            }
        }
    }
    //public bool HasAMemberThatIsAPartyLeader(PARTY_QUEST_TYPE partyType) {
    //    for (int i = 0; i < characters.Count; i++) {
    //        Character member = characters[i];
    //        if (member.partyComponent.hasParty && member.partyComponent.currentParty.IsLeader(member) && member.partyComponent.currentParty.partyType == partyType) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    #endregion

    #region Utilities
    private void AddListeners() {
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
    }
    private void SetFactionColor(Color color) {
        factionColor = color;
    }
    public void SetName(string name) {
        this.name = name;
    }
    private void SetDescription(string description) {
        this.description = description;
    }
    public void SetIsMajorFaction(bool state) {
        isMajorFaction = state;
    }
    public bool IsHostileWith(Faction faction) {
        if (faction == this) {
            return false;
        }
        FactionRelationship rel = GetRelationshipWith(faction);
        return rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile;
    }
    public bool IsFriendlyWith(Faction faction) {
        if (faction == this) {
            return false;
        }
        FactionRelationship rel = GetRelationshipWith(faction);
        return rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Friendly;
    }
    public override string ToString() {
        return name;
    }
    public void SetFactionActiveState(bool state) {
        if (isActive == state) {
            return; //ignore change
        }
        isActive = state;
        Messenger.Broadcast(Signals.FACTION_ACTIVE_CHANGED, this);
    }
    public string GetRaceText() {
        return $"{UtilityScripts.GameUtilities.GetNormalizedRaceAdjective(race)} Faction";
    }
    private void OnTickEnded() {
        ProcessForcedCancelJobsOnTickEnded();
    }
    private void OnDayStarted() {
        ClearAllBlacklistToAllExistingJobs();
    }
    #endregion

    #region Relationships
    public void AddNewRelationship(Faction relWith, FactionRelationship relationship) {
        if (!relationships.ContainsKey(relWith)) {
            relationships.Add(relWith, relationship);
        } 
        // else {
        //     throw new System.Exception(
        //         $"{this.name} already has a relationship with {relWith.name}, but something is trying to create a new one!");
        // }
    }
    public void RemoveRelationshipWith(Faction relWith) {
        if (relationships.ContainsKey(relWith)) {
            relationships.Remove(relWith);
        }
    }
    public FactionRelationship GetRelationshipWith(Faction faction) {
        if (relationships.ContainsKey(faction)) {
            return relationships[faction];
        }
        return null;
    }
    public bool HasRelationshipStatus(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.isPlayerFaction) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                return true;
            }
        }
        return false;
    }
    public bool HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS stat, Faction faction) {
        if (relationships.ContainsKey(faction)) {
            return relationships[faction].relationshipStatus == stat;
        }
        return false;
    }
    public Faction GetFactionWithRelationship(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.isPlayerFaction) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                return kvp.Key;
            }
        }
        return null;
    }
    public List<Faction> GetFactionsWithRelationship(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        List<Faction> factions = new List<Faction>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.isPlayerFaction) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                factions.Add(kvp.Key);
            }
        }
        return factions;
    }
    public bool SetRelationshipFor(Faction otherFaction, FACTION_RELATIONSHIP_STATUS status) {
        if (relationships.ContainsKey(otherFaction)) {
            return relationships[otherFaction].SetRelationshipStatus(status);
        } else {
            Debug.LogWarning($"There is no key for {otherFaction.name} in {this.name}'s relationship dictionary");
            return false;
        }
    }
    public bool IsAtWar() {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (kvp.Key.isActive && kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
                return true;
            }
        }
        return false;
    }
    public Faction GetRandomAtWarFaction() {
        List<Faction> factions = null;
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (kvp.Key.isActive && kvp.Key.isMajorNonPlayer && kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
                if(factions == null) { factions = new List<Faction>(); }
                factions.Add(kvp.Key);
            }
        }
        if(factions != null && factions.Count > 0) {
            return factions[UnityEngine.Random.Range(0, factions.Count)];
        }
        return null;
    }
    #endregion

    #region Death
    public void Death() {
        RemoveListeners();
        FactionManager.Instance.RemoveRelationshipsWith(this);
    }
    #endregion

    #region Areas
    public void AddToOwnedSettlements(BaseSettlement settlement) {
        if (!ownedSettlements.Contains(settlement)) {
            ownedSettlements.Add(settlement);
            Messenger.Broadcast(Signals.FACTION_OWNED_SETTLEMENT_ADDED, this, settlement);
        }
    }
    public void RemoveFromOwnedSettlements(BaseSettlement settlement) {
        if (ownedSettlements.Remove(settlement)) {
            Messenger.Broadcast(Signals.FACTION_OWNED_SETTLEMENT_REMOVED, this, settlement);
        }
    }
    public bool HasOwnedRegionWithLandmarkType(LANDMARK_TYPE type) {
        for (int i = 0; i < ownedSettlements.Count; i++) {
            if (ownedSettlements[i].HasStructure(type.GetStructureType())) {
                return true;
            }
        }
        return false;
    }
    public bool HasOwnedSettlementInRegion(Region region) {
        for (int i = 0; i < ownedSettlements.Count; i++) {
            if(ownedSettlements[i] is NPCSettlement settlement) {
                if(settlement.region == region) {
                    return true;
                }
            }
        }
        //for (int i = 0; i < ownedStructures.Count; i++) {
        //    if (ownedStructures[i].location == region) {
        //        return true;
        //    }
        //}
        return false;
    }
    public bool HasOwnedSettlement() {
        return ownedSettlements.Count > 0;
    }
    public bool HasOwnedSettlementExcept(NPCSettlement settlementException) {
        for (int i = 0; i < ownedSettlements.Count; i++) {
            if (ownedSettlements[i] is NPCSettlement settlement) {
                if (settlement != settlementException) {
                    return true;
                }
            }
        }
        return false;
    }
    public BaseSettlement GetRandomOwnedSettlement() {
        if(ownedSettlements.Count > 0) {
            return ownedSettlements[UnityEngine.Random.Range(0, ownedSettlements.Count)];
        }
        return null;
    }
    public BaseSettlement GetFirstOwnedSettlementThatMeetCriteria(Func<BaseSettlement, bool> criteria) {
        for (int i = 0; i < ownedSettlements.Count; i++) {
            BaseSettlement settlement = ownedSettlements[i];
            if (criteria.Invoke(settlement)) {
                return settlement;
            }
        }
        return null;
    }
    public LocationStructure GetFirstStructureOfTypeFromOwnedSettlementsWithLeastVillagers(STRUCTURE_TYPE structureType) {
        BaseSettlement leastVillagersSettlement = null;
        LocationStructure structure = null;
        for (int i = 0; i < ownedSettlements.Count; i++) {
            BaseSettlement settlement = ownedSettlements[i];
            if (structure == null || leastVillagersSettlement == null || settlement.residents.Count < leastVillagersSettlement.residents.Count) {
                structure = settlement.GetFirstStructureOfType(structureType);
            }
        }
        return null;
    }
    //public bool HasOwnedStructures() {
    //    return ownedStructures.Count > 0;
    //}
    //public bool HasOwnedStructureExcept(LocationStructure structureException) {
    //    for (int i = 0; i < ownedStructures.Count; i++) {
    //        if (ownedStructures[i] != structureException) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    #endregion

    #region Emblems
    public void SetEmblem(Sprite sprite) {
        emblem = sprite;
    }
    #endregion
    
    // #region Logs
    // public void AddHistory(Log log) {
    //     if (!history.Contains(log)) {
    //         history.Add(log);
    //         if (history.Count > MAX_HISTORY_LOGS) {
    //             history.RemoveAt(0);
    //         }
    //         Messenger.Broadcast(Signals.FACTION_LOG_ADDED, this);
    //     }
    // }
    // #endregion
    
    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job, int position = -1) {
        if (position == -1) {
            availableJobs.Add(job);    
        } else {
            availableJobs.Insert(position, job);
        }
        if (job is GoapPlanJob goapJob) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI} was added to {name}'s available jobs");
        } else {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was added to {name}'s available jobs");    
        }
        
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI?.name} was removed from {name}'s available jobs");
            } else {
                Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was removed from {name}'s available jobs");    
            }
            OnJobRemovedFromAvailableJobs(job);
            return true;
        }
        return false;
    }
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == type) {
                count++;
            }
        }
        return count;
    }
    public bool HasJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (availableJobs[i].jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(GoapEffect effect, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (effect.conditionType == gpj.goal.conditionType
                    && effect.conditionKey == gpj.goal.conditionKey
                    && effect.target == gpj.goal.target
                    && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public JobQueueItem GetJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                JobQueueItem job = availableJobs[i];
                if (job.jobType == jobTypes[j]) {
                    return job;
                }
            }
        }
        return null;
    }
    public List<JobQueueItem> GetJobs(params JOB_TYPE[] jobTypes) {
        List<JobQueueItem> jobs = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (jobTypes.Contains(job.jobType)) {
                jobs.Add(job);
            }
        }
        return jobs;
    }
    public JobQueueItem GetJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return gpj;
                }
            }
        }
        return null;
    }
    public JobQueueItem GetFirstUnassignedJobToCharacterJob(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(job)) {
                return job;
            }
        }
        return null;
    }
    private void OnJobRemovedFromAvailableJobs(JobQueueItem job) {
        JobManager.Instance.OnFinishJob(job);
    }
    private void ClearAllBlacklistToAllExistingJobs() {
        for (int i = 0; i < availableJobs.Count; i++) {
            availableJobs[i].ClearBlacklist();
        }
    }
    public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    AddForcedCancelJobsOnTickEnded(goapJob);
                }
            }
        }
    }
    public bool HasActiveReportDemonicStructureJob(LocationStructure demonicStructure) {
        for (int i = 0; i < characters.Count; i++) {
            Character factionMember = characters[i];
            JobQueueItem job = factionMember.jobQueue.GetJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE);
            if(job != null && job is GoapPlanJob goapJob) {
                OtherData[] otherData = goapJob.GetOtherData(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
                if(otherData != null && otherData.Length == 1 && otherData[0].obj == demonicStructure) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) { }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (!job.IsJobStillApplicable()) {
            RemoveFromAvailableJobs(job);
        }
    }
    public bool ForceCancelJob(JobQueueItem job) {
        return RemoveFromAvailableJobs(job);
    }
    public void AddForcedCancelJobsOnTickEnded(JobQueueItem job) {
        if (!forcedCancelJobsOnTickEnded.Contains(job)) {
            forcedCancelJobsOnTickEnded.Add(job);
        }
    }
    public void ProcessForcedCancelJobsOnTickEnded() {
        if (forcedCancelJobsOnTickEnded.Count > 0) {
            for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded[i].ForceCancelJob(false);
            }
            forcedCancelJobsOnTickEnded.Clear();
        }
    }
    #endregion

    //#region Party
    //public bool HasActiveParty(params PARTY_QUEST_TYPE[] party) {
    //    for (int i = 0; i < characters.Count; i++) {
    //        Character factionMember = characters[i];
    //        if (factionMember.partyComponent.hasParty) {
    //            for (int j = 0; j < party.Length; j++) {
    //                PARTY_QUEST_TYPE partyType = party[j];
    //                if (factionMember.partyComponent.currentParty.partyType == partyType) {
    //                    return true;
    //                }
    //            }
    //        }
    //    }
    //    return false;
    //}
    //public bool HasActivePartywithTarget(PARTY_QUEST_TYPE partyType, IPartyQuestTarget target) {
    //    return GetActivePartywithTarget(partyType, target) != null;
    //}
    //public Party GetActivePartywithTarget(PARTY_QUEST_TYPE partyType, IPartyQuestTarget target) {
    //    for (int i = 0; i < characters.Count; i++) {
    //        Character factionMember = characters[i];
    //        if (factionMember.partyComponent.hasParty) {
    //            Party party = factionMember.partyComponent.currentParty;
    //            if (party.partyType == partyType && party.target == target) {
    //                return party;
    //            }
    //        }
    //    }
    //    return null;
    //}
    //#endregion

    #region War Declaration
    public void CheckForWar(Faction targetFaction, CRIME_SEVERITY crimeSeverity, Character crimeCommitter, Character crimeTarget, ActualGoapNode crime) {
        if (targetFaction != this && targetFaction != null) {
            string debugLog = $"Checking for war {name} against {targetFaction.name}";
            if (!factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful)) {
                debugLog += $"\n{name} is not a peaceful faction.";
                bool isTargetPartOfFaction = crimeTarget != null && crimeTarget.faction == this;
                debugLog += $"\nTarget of committed crime is part of faction {name}: {isTargetPartOfFaction.ToString()}";
                debugLog += $"\nSeverity of committed crime is {crimeSeverity.ToString()}.";
                float chance = 0f;
                if (isTargetPartOfFaction) {
                    switch (crimeSeverity) {
                        case CRIME_SEVERITY.Misdemeanor:
                            if (crimeTarget.isFactionLeader) {
                                chance = 30f;
                            } else if (crimeTarget.isSettlementRuler) {
                                chance = 15f;
                            } else {
                                chance = 5f;
                            }
                            break;
                        case CRIME_SEVERITY.Serious:
                            if (crimeTarget.isFactionLeader) {
                                chance = 60f;
                            } else if (crimeTarget.isSettlementRuler) {
                                chance = 40f;
                            } else {
                                chance = 20f;
                            }
                            break;
                        case CRIME_SEVERITY.Heinous:
                            if (crimeTarget.isFactionLeader) {
                                chance = 90f;
                            } else if (crimeTarget.isSettlementRuler) {
                                chance = 65f;
                            } else {
                                chance = 35f;
                            }
                            break;
                    }
                    if (factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
                        debugLog += $"\n{name} is a warmonger faction.";
                        chance *= 1.5f;
                    }
                } else {
                    debugLog += $"\nTarget is not part of faction.";
                    //target is not part of faction
                    if (crimeSeverity == CRIME_SEVERITY.Heinous && (crimeCommitter.isFactionLeader || crimeCommitter.isSettlementRuler)) {
                        debugLog += $"\nCrime severity   Heinous and {crimeCommitter.name} is Faction Leader or Settlement Ruler";
                        chance = 50f;
                    }
                    if (factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
                        debugLog += $"\n{name} is a warmonger faction.";
                        chance *= 1.5f;
                    }
                }

                float roll = Random.Range(0f, 100f);
                debugLog += $"\nChance for war is {chance.ToString()}. Roll is {roll.ToString()}";
                if (roll < chance) {
                    debugLog += $"\nChance for war met, setting {name} and {targetFaction.name} as Hostile.";
                    if (SetRelationshipFor(targetFaction, FACTION_RELATIONSHIP_STATUS.Hostile)) {
                        debugLog += $"\nSuccessfully set {name} and {targetFaction.name} as Hostile.";
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_war", providedTags: LOG_TAG.Life_Changes);
                        log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                        log.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
                        log.AddToFillers(crime.descriptionLog.fillers);
                        log.AddToFillers(null, crime.descriptionLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                        log.AddLogToDatabase();    
                        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                    } else {
                        debugLog += $"\nCould not set {name} and {targetFaction.name} as Hostile.";
                    }
                }
            } else {
                debugLog += $"\n{name} is a peaceful faction.";
            }
            Debug.Log(debugLog);
        }
    }
    #endregion

    #region Crime
    public CRIME_SEVERITY GetCrimeSeverity(Character actor, IPointOfInterest target, CRIME_TYPE crimeType) {
        return factionType.GetCrimeSeverity(actor, target, crimeType);
    }
    #endregion

    #region Heirloom
    public void SetFactionHeirloom(TileObject heirloom) {
        if(factionHeirloom != heirloom) {
            factionHeirloom = heirloom as Heirloom;
            if(factionHeirloom != null) {
                factionHeirloom.SetStructureSpot(factionHeirloom.currentStructure);
                StartHeirloomSearch();
            }
        }
    }
    private void StartHeirloomSearch() {
        HeirloomSearch();
    }
    private void HeirloomSearch() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
        SchedulingManager.Instance.AddEntry(dueDate, DoneHeirloomSearch, this);
    }
    private void DoneHeirloomSearch() {
        if(factionHeirloom != null) {
            if(factionHeirloom.gridTileLocation != null && !factionHeirloom.IsInStructureSpot() && factionHeirloom.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                && factionHeirloom.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType == BIOMES.DESERT && !partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Heirloom_Hunt) && !HasJob(JOB_TYPE.HUNT_HEIRLOOM)) {
                factionJobTriggerComponent.TriggerHeirloomHuntJob(factionHeirloom.gridTileLocation.structure.region);
            }
            HeirloomSearch();
        }
    }
    #endregion

    #region Pathfinding
    public void SetPathfindingTag(uint tag) {
        pathfindingTag = tag;
    }
    public void SetPathfindingDoorTag(uint tag) {
        pathfindingDoorTag = tag;
    }
    #endregion

    #region Faction Type
    public bool SetFactionType(FactionType factionType) {
        if(this.factionType == null || this.factionType.type != factionType.type) {
            this.factionType = factionType;
            return true;
        }
        return false;
    }
    public bool SetFactionType(FACTION_TYPE type) {
        if(factionType == null || factionType.type != type) {
            FactionType newFactionType = FactionManager.Instance.CreateFactionType(type);
            return SetFactionType(newFactionType);
        }
        return false;
    }
    public void ChangeFactionType(FACTION_TYPE type) {
        if (SetFactionType(type)) {
            this.factionType.SetAsDefault();
            FactionInfoHubUI.Instance.UpdateFactionItem(this);
        }
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataFaction data) {
        if (!data.isLeaderPlayer) {
            if (!string.IsNullOrEmpty(data.leaderID)) {
                Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.leaderID);
                leader = character;
            }
        }
        for (int i = 0; i < data.characterIDs.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.characterIDs[i]);
            if (character != null) { 
                //added checker for safety because it is possible in previous versions that the game was saved with a character that should no longer exist
                //aka. In Limbo lycanthropes that are already dead, but they weren't removed from the Wild Monsters Faction.
                characters.Add(character);    
            }
        }
        for (int i = 0; i < data.bannedCharacterIDs.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.bannedCharacterIDs[i]);
            bannedCharacters.Add(character);
        }

        foreach (KeyValuePair<string, SaveDataFactionRelationship> item in data.relationships) {
            Faction faction2 = FactionManager.Instance.GetFactionByPersistentID(item.Key);
            FactionRelationship rel = GetRelationshipWith(faction2); //check first if this faction has reference to relationship with faction 2
            if (rel == null) {
                rel = faction2.GetRelationshipWith(this); //if none, check if faction 2 has reference to relationship with faction 1
                if (rel == null) {
                    rel = item.Value.Load(); //if still none, then load new instance of relationship between the 2 factions
                }
            }
            Assert.IsNotNull(rel, $"Relationship between {name} and {faction2.name} is null!");
            AddNewRelationship(faction2, rel);
            faction2.AddNewRelationship(this, rel);
            
            // rel = faction2.GetRelationshipWith(this);
            // if (rel == null) {
            //     rel = item.Value.Load();
            // }
            // faction2.AddNewRelationship(this, rel);
        }

        // for (int i = 0; i < data.history.Count; i++) {
        //     Log log = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.history[i]);
        //     history.Add(log);
        // }

        for (int i = 0; i < data.ownedSettlementIDs.Count; i++) {
            BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.ownedSettlementIDs[i]);
            ownedSettlements.Add(settlement);
        }

        partyQuestBoard.LoadReferences(data.partyQuestBoard);
    }
    #endregion
}