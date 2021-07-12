using System;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Components;
using UnityEngine;
using Factions.Faction_Types;
using Factions.Faction_Succession;
using Inner_Maps;
using Locations.Settlements;
using Traits;
using Inner_Maps.Location_Structures;
using Logs;
using Object_Pools;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Faction : IJobOwner, ISavable, ILogFiller {

    public string persistentID { get; }
    public int id { get; }
    public string name { get; private set; }
    public string description { get; private set; }
    public bool isMajorFaction { get; private set; }
    public ILeader leader { get; private set; }
    public Sprite emblem { get; private set; }
    public string emblemName { get; private set; }
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
    public FactionJobTriggerComponent factionJobTriggerComponent { get; private set; }
    public PartyQuestBoard partyQuestBoard { get; private set; }
    public int newLeaderDesignationChance { get; private set; }
    public uint pathfindingTag { get; private set; }
    public uint pathfindingDoorTag { get; private set; }
    public Heirloom factionHeirloom { get; private set; }
    public FactionEventDispatcher factionEventDispatcher { get; private set; }
    public bool isDisbanded { get; private set; }

    //Components
    public FactionIdeologyComponent ideologyComponent { get; private set; }
    public FactionSuccessionComponent successionComponent { get; private set; }

    //private readonly WeightedDictionary<Character> newLeaderDesignationWeights;

    public bool isInfoUnlocked = true;


    #region getters/setters
    public bool isMajorOrVagrant => isMajorFaction || this.factionType.type == FACTION_TYPE.Vagrants;
    public bool isMajorNonPlayerOrVagrant => isMajorNonPlayer || this.factionType.type == FACTION_TYPE.Vagrants;
    public bool isMajorNonPlayer => isMajorFaction && !isPlayerFaction;
    public bool isNonMajorOrPlayer => !isMajorFaction || isPlayerFaction;
    public JobTriggerComponent jobTriggerComponent => factionJobTriggerComponent;
    public bool isPlayerFaction => factionType.type == FACTION_TYPE.Demons;
    public string nameWithColor => GetNameWithColor();
    public JOB_OWNER ownerType => JOB_OWNER.FACTION;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Faction;
    public System.Type serializedData => typeof(SaveDataFaction);
    public RACE race { get; private set; }
    #endregion

    public Faction(FACTION_TYPE p_factionType, RACE p_race = RACE.NONE) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        id = UtilityScripts.Utilities.SetID(this);
        SetName(RandomNameGenerator.GenerateFactionName());
        SetFactionColor(UtilityScripts.Utilities.GetColorForFaction());
        SetFactionActiveState(true);
        SetFactionType(p_factionType);
        isInfoUnlocked = true;
        race = p_race == RACE.NONE ? p_factionType.GetRaceForFactionType() : p_race;
        characters = new List<Character>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        availableJobs = new List<JobQueueItem>();
        //newLeaderDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        partyQuestBoard = new PartyQuestBoard(this);
        factionEventDispatcher = new FactionEventDispatcher();
        factionJobTriggerComponent = new FactionJobTriggerComponent(this);

        //Components
        ideologyComponent = new FactionIdeologyComponent(); ideologyComponent.SetOwner(this);
        successionComponent = new FactionSuccessionComponent(); successionComponent.SetOwner(this);

        ResetNewLeaderDesignationChance();
        AddListeners();
    }
    public Faction(SaveDataFaction data) {
        persistentID = data.persistentID;
        id = UtilityScripts.Utilities.SetID(this, data.id);
        factionJobTriggerComponent = new FactionJobTriggerComponent(this);
        factionType = data.factionType.Load();
        partyQuestBoard = data.partyQuestBoard.Load();

        //Components
        ideologyComponent = data.ideologyComponent.Load(); ideologyComponent.SetOwner(this);
        successionComponent = data.successionComponent.Load(); successionComponent.SetOwner(this);

        name = data.name;
        description = data.description;
        emblem = FactionManager.Instance.GetFactionEmblem(data);
        emblemName = emblem.name;
        factionColor = data.factionColor;
        race = data.race;
        isActive = data.isActive;
        isMajorFaction = data.isMajorFaction;
        newLeaderDesignationChance = data.newLeaderDesignationChance;
        pathfindingTag = data.pathfindingTag;
        pathfindingDoorTag = data.pathfindingDoorTag;

        characters = new List<Character>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        availableJobs = new List<JobQueueItem>();
        //newLeaderDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        factionEventDispatcher = new FactionEventDispatcher();
        isInfoUnlocked = data.isInfoUnlocked;
        isDisbanded = data.isDisbanded;
        AddListeners();
    }

    #region Characters
    public bool JoinFaction(Character character, bool broadcastSignal = true, bool bypassIdeologyChecking = false, bool isInitial = false) {
        if (bypassIdeologyChecking || ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
            if (AddCharacter(character)) {
                //Once a character joins any faction and the faction is not the owner of the character's current home settlement, leave the settlement also
                //Reason: One village = One faction, no other faction can co exist in a village, for simplification
                if (character.homeSettlement != null && character.homeSettlement.owner != null && character.homeSettlement.owner != this) {
                    character.MigrateHomeStructureTo(null);
                }

                if (!isInitial) {
                    //Whenever a character joins a new faction, add Transitioning status, so that if his new faction is hostile with his previous faction he will not be attacked immediately by the people of his previous faction
                    //Also, cancel all needs recovery job because it might not be applicable upon changing factions, example: if a character has founded a village, it should no longer it in the previous home village
                    character.traitContainer.AddTrait(character, "Transitioning");
                    character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT,
                        JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT, JOB_TYPE.HAPPINESS_RECOVERY);
                }

                //Every time ratman changes faction, behaviour set should update to know if he will use the resident behaviour or the ratmana behaviour
                //Every time a minion changes faction, behaviour set should update to know if he will use the resident behaviour or the minion behaviour
                //Reference: https://trello.com/c/8LQpoNGp/3036-when-a-demon-is-recruited-by-a-major-faction-its-behavior-will-be-replaced-by-the-villager-set
                if (character.race == RACE.RATMAN || character.minion != null) {
                    character.behaviourComponent.UpdateDefaultBehaviourSet();
                }

                if (factionType.type == FACTION_TYPE.Undead && character.necromancerTrait != null) {
                    //Every time a necromancer is added to the Undead Faction, check if it can be the faction leader
                    if (!HasAliveNecromancerLeaderExcept(character)) {
                        FactionManager.Instance.undeadFaction.OnlySetLeader(character);
                        //We only call the Become Faction Leader Log so that there will be a log if necromancer becomes the faction leader of undead
                        //So since the log is our only purpose, we just need to log it, not call another interrupt, because calling another interrupt for the sole purpose of log is bad and might cause problems
                        //actor.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, actor);
                        Log becomeLeaderLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Become Faction Leader", "became_leader", null, LOG_TAG.Major);
                        becomeLeaderLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        becomeLeaderLog.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                        becomeLeaderLog.AddLogToDatabase();
                        PlayerManager.Instance.player.ShowNotificationFrom(character, becomeLeaderLog, true);
                    }
                }

                if (broadcastSignal) {
                    Messenger.Broadcast(FactionSignals.CHARACTER_ADDED_TO_FACTION, character, this);
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
            Messenger.Broadcast(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, character, this);
            if (characters.All(c => c.isDead) && factionType.type != FACTION_TYPE.Undead && factionType.type != FACTION_TYPE.Vagrants && factionType.type != FACTION_TYPE.Demons && factionType.type != FACTION_TYPE.Ratmen) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "disband", providedTags: LOG_TAG.Major);
                log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                // Messenger.Broadcast(FactionSignals.FACTION_DISBANDED, this);
                DisbandFaction();
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// Function used for making a character leave this faction because we want to disband it
    /// aka. Removing all characters from the faction. Usually because all members are dead.
    /// </summary>
    /// <param name="character">The character to remove.</param>
    public bool LeaveFactionForDisband(Character character) {
        if (characters.Remove(character)) {
            if (leader == character) {
                SetLeader(null); //so a new leader can be set if the leader is ever removed from the list of characters of this faction
            }
            character.SetFaction(null);
            Messenger.Broadcast(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, character, this);
            return true;
        }
        return false;
    }
    public bool AddCharacter(Character character) {
        if (!characters.Contains(character)) {
            characters.Add(character);
            character.SetFaction(this);
            factionType.ProcessNewMember(character);
            //if (isPlayerFaction && character is Summon summon) {
            //    Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_SUMMON, summon);
            //}
            return true;
        }
        return false;
    }
    public void OnlySetLeader(ILeader newLeader) {
        if (leader != newLeader) {
            ILeader prevLeader = leader;
            leader = newLeader;
            if (prevLeader != null && prevLeader is Character prevCharacterLeader) {
                if (isMajorNonPlayer) {
                    prevCharacterLeader.behaviourComponent.RemoveBehaviourComponent(typeof(FactionLeaderBehaviour));
                    if (!prevCharacterLeader.isSettlementRuler) {
                        prevCharacterLeader.jobComponent.RemoveAbleJob(JOB_TYPE.JUDGE_PRISONER);
                        prevCharacterLeader.jobComponent.RemoveAbleJob(JOB_TYPE.PLACE_BLUEPRINT);
                    }
                }
            }
            Character newCharacterLeader = leader as Character;
            if (leader != null) {
                if (newCharacterLeader != null) {
                    if (isMajorNonPlayer) {
                        newCharacterLeader.behaviourComponent.AddBehaviourComponent(typeof(FactionLeaderBehaviour));
                        newCharacterLeader.jobComponent.AddAbleJob(JOB_TYPE.JUDGE_PRISONER);
                        newCharacterLeader.jobComponent.AddAbleJob(JOB_TYPE.PLACE_BLUEPRINT);
                    }
                }
            }
            if (newCharacterLeader != null) {
                Messenger.Broadcast(CharacterSignals.ON_SET_AS_FACTION_LEADER, newCharacterLeader, prevLeader);
            } else if (newLeader == null) {
                Messenger.Broadcast(CharacterSignals.ON_FACTION_LEADER_REMOVED, this, prevLeader);
            }
            factionEventDispatcher.ExecuteFactionLeaderChangedEvent(newLeader);

            ProcessFactionLeaderAsSettlementRuler();
        }
    }
    public void ProcessFactionLeaderAsSettlementRuler() {
        //https://trello.com/c/KDgydAWd/3005-faction-leader-also-doubles-as-the-settlement-ruler-of-its-current-settlement
        if (leader != null && leader is Character newCharacterLeader) {
            NPCSettlement homeSettlement = newCharacterLeader.homeSettlement;
            if (homeSettlement != null && homeSettlement.locationType == LOCATION_TYPE.VILLAGE && homeSettlement.owner == this) {
                //Only do this for villages since special structures does not have settlement rulers
                Character previousRuler = homeSettlement.ruler;
                if (previousRuler != newCharacterLeader) {
                    if (GameManager.Instance.gameHasStarted) {
                        if (previousRuler == null) {
                            newCharacterLeader.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Settlement_Ruler, newCharacterLeader);
                        } else {
                            //Do not trigger Become_Settlement_Ruler because we have a special log for this
                            homeSettlement.SetRuler(newCharacterLeader);

                            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "replace_ruler", null, LOG_TAG.Life_Changes);
                            log.AddToFillers(newCharacterLeader, newCharacterLeader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddToFillers(previousRuler, previousRuler.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                            log.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
                            log.AddLogToDatabase(true);
                        }
                    } else {
                        //If the game has not yet started yet, just set the ruler and do not log it because this is the first state of the world/game
                        homeSettlement.SetRuler(newCharacterLeader);
                    }
                }
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
        if (character.faction == this && !character.isDead && !ideologyComponent.DoesCharacterFitCurrentIdeologies(character) && character.race.IsSapient()) {
            //Character will only leave faction here if they are sapients
            if (willLog) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_not_fit");
            } else {
                Faction targetFaction;
                if (character is Summon summon) {
                    targetFaction = summon.defaultFaction;
                } else {
                    targetFaction = FactionManager.Instance.vagrantFaction;
                }
                character.ChangeFactionTo(targetFaction);
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
        if (character.faction == this) {
            AddBannedCharacter(character);

            character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "kick_out_faction_character");
            //character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
            //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "kick_out_faction_character");
            //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
            //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
    }
    private void OnCharacterPresumedDead(Character missingCharacter) {
        //Per Marvin, remove this, so it means that when a leader becomes presumed dead, he will still be the faction leader of the faction
        //if (leader != null && missingCharacter == leader) {
        //    SetLeader(null);
        //}
    }
    private void OnCharacterDied(Character deadCharacter) {
        if (leader != null && deadCharacter == leader) {
            SetLeader(null);
        }
        successionComponent.OnCharacterDied(deadCharacter);
        UpdateFactionCount();
        if (!isDisbanded && isMajorNonPlayer) {
            if (characters.All(c => c.isDead)) {
                //all characters are dead
                DisbandFaction();
            }    
        }
    }
    /// <summary>
    /// Disband this faction. This is used to clear out all faction data,
    /// and remove all current members of the faction.
    /// </summary>
    private void DisbandFaction() {
#if DEBUG_LOG
        Debug.Log($"Disbanded faction {name} because it has no more living members");
#endif
        isDisbanded = true;
        SetFactionActiveState(false);
        ClearOutReservedFactionData();
        List<Character> currentMembers = RuinarchListPool<Character>.Claim();
        currentMembers.AddRange(characters);
        for (int i = 0; i < currentMembers.Count; i++) {
            Character character = currentMembers[i];
            LeaveFactionForDisband(character);
        }
        RuinarchListPool<Character>.Release(currentMembers);
        RemoveListeners();
        FactionManager.Instance.RemoveRelationshipsWith(this);
        Messenger.Broadcast(FactionSignals.FACTION_DISBANDED, this);
    }
    private void ClearOutReservedFactionData() {
        FactionEmblemRandomizer.SetEmblemAsUnUsed(emblem);
        InnerMapManager.Instance.ReturnPathfindingPair(this);
    }

    private void OnCharacterReturnToLife(Character deadCharacter) {
        UpdateFactionCount();
    }

    private void OnFactionmemberChanges(Character newMember, Faction faction) {
        UpdateFactionCount();
    }

    private void OnNewVillagerMigrated(Character character) {
        UpdateFactionCount();
    }

    private void UpdateFactionCount() {
        FactionInfoHubUI.Instance.UpdateFactionItem(this);
    }
    public void SetLeader(ILeader newLeader) {
        if (!isMajorFaction && !isPlayerFaction) {
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
        int chance = Random.Range(0, 100);
#if DEBUG_LOG
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new faction leader designation for {name}";
        debugLog += $"\n-Chance: {newLeaderDesignationChance.ToString()}";
        debugLog += $"\n-Roll: {chance.ToString()}";
        Debug.Log(debugLog);
#endif
        // chance = 0;
        if (chance < newLeaderDesignationChance) {
            DesignateNewLeader();
        } else {
            newLeaderDesignationChance += 2;
        }
    }
    public void DesignateNewLeader(bool willLog = true) {
#if DEBUG_LOG
        string log = $"Designating a new npcSettlement faction leader for: {name}(chance it triggered: {newLeaderDesignationChance.ToString()})";
#endif

        Character chosenLeader = successionComponent.PickSuccessor();
        if (chosenLeader != null) {
#if DEBUG_LOG
            log += $"\nCHOSEN LEADER: {chosenLeader.name}";
#endif
            if (willLog) {
                chosenLeader.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, chosenLeader);
            } else {
                SetLeader(chosenLeader);
            }
        }
        //newLeaderDesignationWeights.Clear();
        //for (int i = 0; i < characters.Count; i++) {
        //    Character member = characters[i];
        //    log += $"\n\n-{member.name}";
        //    if (member.isDead /*|| member.isMissing*/ || member.isBeingSeized || member.isInLimbo) {
        //        log += "\nEither dead, missing, in limbo, seized or enslaved, will not be part of candidates for faction leader";
        //        continue;
        //    }
        //    if (member.crimeComponent.IsWantedBy(this)) {
        //        log += "\nMember is wanted by this faction, skipping...";
        //        continue;
        //    }

        //    bool isInHome = member.IsAtHome();
        //    bool isInAnActiveParty = member.partyComponent.isMemberThatJoinedQuest;

        //    if (!isInHome && !isInAnActiveParty) {
        //        log += "\nMember is not inside home and not in active party, skipping...";
        //        continue;
        //    }

        //    int weight = 50;
        //    log += "\n  -Base Weight: +50";
        //    if (factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires)) {
        //        Vampire vampire = member.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        //        if (vampire != null && vampire.DoesFactionKnowThisVampire(this)) {
        //            weight += 100;
        //            log += "\n  -Faction reveres vampires and member is a known vampire: +100";
        //        }
        //    }
        //    if (factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
        //        if (member.isLycanthrope && member.lycanData.DoesFactionKnowThisLycan(this)) {
        //            weight += 100;
        //            log += "\n  -Faction reveres werewolves and member is a known Lycanthrope: +100";
        //        }
        //    }
        //    if (member.isSettlementRuler) {
        //        weight += 30;
        //        log += "\n  -NPCSettlement Ruler: +30";
        //    }
        //    if (member.characterClass.className == "Noble") {
        //        weight += 40;
        //        log += "\n  -Noble: +40";
        //    }
        //    int numberOfFriends = 0;
        //    int numberOfEnemies = 0;
        //    for (int j = 0; j < member.relationshipContainer.charactersWithOpinion.Count; j++) {
        //        Character otherCharacter = member.relationshipContainer.charactersWithOpinion[j];
        //        if (otherCharacter.faction == this) {
        //            if (otherCharacter.relationshipContainer.IsFriendsWith(member)) {
        //                numberOfFriends++;
        //            } else if (otherCharacter.relationshipContainer.IsEnemiesWith(member)) {
        //                numberOfEnemies++;
        //            }
        //        }
        //    }
        //    if (numberOfFriends > 0) {
        //        int weightToAdd = 0;
        //        if (member.traitContainer.HasTrait("Worker")) {
        //            weightToAdd = Mathf.FloorToInt((numberOfFriends * 20) * 0.2f);
        //        } else {
        //            weightToAdd = (numberOfFriends * 20);    
        //        }
        //        weight += weightToAdd;
        //        log += $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{weightToAdd}";
        //    }
        //    if (member.traitContainer.HasTrait("Inspiring")) {
        //        weight += 25;
        //        log += "\n  -Inspiring: +25";
        //    }
        //    if (member.traitContainer.HasTrait("Authoritative")) {
        //        weight += 50;
        //        log += "\n  -Authoritative: +50";
        //    }


        //    if (numberOfEnemies > 0) {
        //        weight += (numberOfEnemies * -10);
        //        log += $"\n  -Num of Enemies/Rivals in the NPCSettlement: {numberOfEnemies}, +{(numberOfEnemies * -10)}";
        //    }
        //    if (member.traitContainer.HasTrait("Unattractive")) {
        //        weight += -20;
        //        log += "\n  -Unattractive: -20";
        //    }
        //    if (member.hasUnresolvedCrime) {
        //        weight += -50;
        //        log += "\n  -Has Unresolved Crime: -50";
        //    }
        //    if (member.traitContainer.HasTrait("Worker")) {
        //        weight += -40;
        //        log += "\n  -Civilian: -40";
        //    }
        //    if (member.traitContainer.HasTrait("Ambitious")) {
        //        weight = Mathf.RoundToInt(weight * 1.5f);
        //        log += "\n  -Ambitious: x1.5";
        //    }
        //    if(weight < 1) {
        //        weight = 1;
        //        log += "\n  -Weight cannot be less than 1, setting weight to 1";
        //    }
        //    if (member.traitContainer.HasTrait("Ambitious")) {
        //        weight = Mathf.RoundToInt(weight * 1.5f);
        //        log += "\n  -Ambitious: x1.5";
        //    }
        //    if (member is Summon || member.characterClass.IsZombie()) {
        //        if (HasMemberThatMeetCriteria(c => c.race.IsSapient() && (c.IsAtHome() || c.partyComponent.isMemberThatJoinedQuest))) {
        //            weight *= 0;
        //            log += "\n  -Member is a Summon and there is atleast 1 Sapient resident inside home settlement or in active party: x0";
        //        }
        //    }
        //    if (member.traitContainer.HasTrait("Enslaved")) {
        //        weight *= 0;
        //        log += "\n  -Enslaved: x0";
        //    }
        //    log += $"\n  -TOTAL WEIGHT: {weight}";
        //    if (weight > 0) {
        //        newLeaderDesignationWeights.AddElement(member, weight);
        //    }
        //}
        //if (newLeaderDesignationWeights.Count > 0) {
        //    Character chosenLeader = newLeaderDesignationWeights.PickRandomElementGivenWeights();
        //    if (chosenLeader != null) {
        //        log += $"\nCHOSEN LEADER: {chosenLeader.name}";
        //        if (willLog) {
        //            chosenLeader.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, chosenLeader);
        //        } else {
        //            SetLeader(chosenLeader);
        //        }
        //    } else {
        //        log += "\nCHOSEN LEADER: NONE";
        //    }
        //} else {
        //    log += "\nCHOSEN LEADER: NONE";
        //}
        ResetNewLeaderDesignationChance();
#if DEBUG_LOG
        Debug.Log(GameManager.Instance.TodayLogString() + log);
#endif
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
                if (character1 != character2) {
                    character1.relationshipContainer.AdjustOpinion(character1, character2, "Base", 0);
                }
            }
        }
    }
    public bool HasMemberThatIsNotDeadHasHomeSettlementUnoccupiedDwelling() {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead && m.homeSettlement != null && m.homeSettlement.GetFirstStructureThatIsUnoccupiedDwelling() != null) {
                return true;
            }
        }
        return false;
    }
    public bool HasMemberThatIsNotDeadCultLeader() {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead && m.characterClass.className == "Cult Leader") {
                return true;
            }
        }
        return false;
    }
    public bool HasMemberThatIsNotDeadAndIsFamilyOrLoverAndNotEnemyRivalWith(Character p_character) {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead && (p_character.relationshipContainer.IsFamilyMember(m) || p_character.relationshipContainer.HasRelationshipWith(m, RELATIONSHIP_TYPE.LOVER)) && !p_character.relationshipContainer.IsEnemiesWith(m)) {
                return true;
            }
        }
        return false;
    }
    public bool HasMemberThatIsNotDeadAndIsCloseFriendWith(Character p_character) {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead && p_character.relationshipContainer.GetOpinionLabel(m) == RelationshipManager.Close_Friend) {
                return true;
            }
        }
        return false;
    }
    public bool HasMemberThatIsNotDeadAndIsRivalWith(Character p_character) {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead && p_character.relationshipContainer.GetOpinionLabel(m) == RelationshipManager.Rival) {
                return true;
            }
        }
        return false;
    }
    public bool HasMemberThatIsNotDeadAndIsFriendWith(Character p_character) {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead && p_character.relationshipContainer.GetOpinionLabel(m) == RelationshipManager.Friend) {
                return true;
            }
        }
        return false;
    }
    public bool HasMemberThatIsSapientAndIsAtHomeOrHasJoinedQuest() {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (m.race.IsSapient() && (m.IsAtHome() || m.partyComponent.isMemberThatJoinedQuest)) {
                return true;
            }
        }
        return false;
    }
    public bool HasMemberThatIsNotDead() {
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead) {
                return true;
            }
        }
        return false;
    }
    public int GetAliveMembersCount() {
        int count = 0;
        for (int i = 0; i < characters.Count; i++) {
            Character m = characters[i];
            if (!m.isDead) {
                count++;
            }
        }
        return count;
    }

    public bool HasAliveMember() {
        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (!character.isDead) {
                return true;
            }
        }
        return false;
    }
    public bool HasAliveNecromancerLeaderExcept(Character p_character) {
        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (character != p_character && !character.isDead && character.necromancerTrait != null && leader == character) {
                return true;
            }
        }
        return false;
    }
#endregion

#region Utilities
    private void AddListeners() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnToLife);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnFactionmemberChanges);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnFactionmemberChanges);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerMigrated);
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);

        successionComponent.AddListeners();
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnToLife);
        Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnFactionmemberChanges);
        Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnFactionmemberChanges);
        Messenger.RemoveListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerMigrated);
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);

        successionComponent.RemoveListeners();
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
        if (faction == null) {
            return false;
        }
        if (faction == this) {
            return true;
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
        Messenger.Broadcast(FactionSignals.FACTION_ACTIVE_CHANGED, this);
    }
    public string GetRaceText() {
        return $"{UtilityScripts.GameUtilities.GetNormalizedRaceAdjective(race)} Faction";
    }
    private void OnTickEnded() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Faction On Tick Ended");
#endif
        ProcessForcedCancelJobsOnTickEnded();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void OnDayStarted() {
        ClearAllBlacklistToAllExistingJobs();
        successionComponent.OnDayStarted();
    }
    private string GetNameWithColor() {
        if (FactionManager.Instance != null) {
            string color = FactionManager.Instance.GetFactionNameColorHex();
            return $"<color=#{color}>{name}</color>";
        }
        return name;
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
        Faction chosenFaction = null;
        List<Faction> factions = RuinarchListPool<Faction>.Claim();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (kvp.Key.isActive && kvp.Key.isMajorNonPlayer && kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
                factions.Add(kvp.Key);
            }
        }
        if(factions.Count > 0) {
            chosenFaction = factions[GameUtilities.RandomBetweenTwoNumbers(0, factions.Count - 1)];
        }
        RuinarchListPool<Faction>.Release(factions);
        return chosenFaction;
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
            Messenger.Broadcast(FactionSignals.FACTION_OWNED_SETTLEMENT_ADDED, this, settlement);
        }
    }
    public void RemoveFromOwnedSettlements(BaseSettlement settlement) {
        if (ownedSettlements.Remove(settlement)) {
            Messenger.Broadcast(FactionSignals.FACTION_OWNED_SETTLEMENT_REMOVED, this, settlement);
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
    public bool HasOwnedVillages() {
        for (int i = 0; i < ownedSettlements.Count; i++) {
            if (ownedSettlements[i].locationType == LOCATION_TYPE.VILLAGE) {
                return true;
            }
        }
        return false;
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
    public bool HasOwnedSettlementThatHasAliveResidentAndIsNotHomeOf(Character p_character) {
        for (int i = 0; i < ownedSettlements.Count; i++) {
            BaseSettlement s = ownedSettlements[i];
            if (s != p_character.homeSettlement && s.HasResidentThatIsNotDead()) {
                return true;
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
    public BaseSettlement GetRandomOwnedVillage() {
        BaseSettlement chosenVillage = null;
        List<BaseSettlement> villages = RuinarchListPool<BaseSettlement>.Claim();
        for (int i = 0; i < ownedSettlements.Count; i++) {
            BaseSettlement s = ownedSettlements[i];
            if (s.locationType == LOCATION_TYPE.VILLAGE) {
                villages.Add(s);
            }
        }
        if (villages.Count > 0) {
            chosenVillage = villages[GameUtilities.RandomBetweenTwoNumbers(0, villages.Count - 1)];
        }
        RuinarchListPool<BaseSettlement>.Release(villages);
        return chosenVillage;
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
        if (emblem != null) {
            emblemName = emblem.name;
        } else {
            emblemName = string.Empty;
        }
    }
    #endregion

    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job, int position = -1) {
        if (position == -1) {
            availableJobs.Add(job);    
        } else {
            availableJobs.Insert(position, job);
        }
#if DEBUG_LOG
        if (job is GoapPlanJob goapJob) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI} was added to {name}'s available jobs");
        } else {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was added to {name}'s available jobs");    
        }
#endif
        
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
#if DEBUG_LOG
                Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI?.name} was removed from {name}'s available jobs");
#endif
            } else {
#if DEBUG_LOG
                Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was removed from {name}'s available jobs");
#endif
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
                OtherData[] otherData = goapJob.GetOtherDataSpecific(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
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
                forcedCancelJobsOnTickEnded[i].ForceCancelJob();
            }
            forcedCancelJobsOnTickEnded.Clear();
        }
    }
#endregion

#region War Declaration
    public void CheckForWar(Faction targetFaction, CRIME_SEVERITY crimeSeverity, Character crimeCommitter, Character crimeTarget, ActualGoapNode crime) {
        if (targetFaction != this && targetFaction != null) {
#if DEBUG_LOG
            string debugLog = $"Checking for war {name} against {targetFaction.name}";
#endif
            if (!factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful)) {
#if DEBUG_LOG
                debugLog += $"\n{name} is not a peaceful faction.";
#endif
                bool isTargetPartOfFaction = crimeTarget != null && crimeTarget.faction == this;
#if DEBUG_LOG
                debugLog += $"\nTarget of committed crime is part of faction {name}: {isTargetPartOfFaction.ToString()}";
                debugLog += $"\nSeverity of committed crime is {crimeSeverity.ToString()}.";
#endif
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
#if DEBUG_LOG
                        debugLog += $"\n{name} is a warmonger faction.";
#endif
                        chance *= 1.5f;
                    }
                } else {
#if DEBUG_LOG
                    debugLog += $"\nTarget is not part of faction.";
#endif
                    //target is not part of faction
                    if (crimeSeverity == CRIME_SEVERITY.Heinous && (crimeCommitter.isFactionLeader || crimeCommitter.isSettlementRuler)) {
#if DEBUG_LOG
                        debugLog += $"\nCrime severity Heinous and {crimeCommitter.name} is Faction Leader or Settlement Ruler";
#endif
                        chance = 50f;
                    }
                    if (factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
#if DEBUG_LOG
                        debugLog += $"\n{name} is a warmonger faction.";
#endif
                        chance *= 1.5f;
                    }
                }

                float roll = Random.Range(0f, 100f);
#if DEBUG_LOG
                debugLog += $"\nChance for war is {chance.ToString()}. Roll is {roll.ToString()}";
#endif
                if (roll < chance) {
#if DEBUG_LOG
                    debugLog += $"\nChance for war met, setting {name} and {targetFaction.name} as Hostile.";
#endif
                    if (SetRelationshipFor(targetFaction, FACTION_RELATIONSHIP_STATUS.Hostile)) {
#if DEBUG_LOG
                        debugLog += $"\nSuccessfully set {name} and {targetFaction.name} as Hostile.";
#endif
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_war", providedTags: LOG_TAG.Life_Changes);
                        log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                        log.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
                        log.AddToFillers(crime.descriptionLog.fillers);
                        log.AddToFillers(null, crime.descriptionLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                        log.AddLogToDatabase();    
                        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                    } else {
#if DEBUG_LOG
                        debugLog += $"\nCould not set {name} and {targetFaction.name} as Hostile.";
#endif
                    }
                }
            } else {
#if DEBUG_LOG
                debugLog += $"\n{name} is a peaceful faction.";
#endif
            }
#if DEBUG_LOG
            Debug.Log(debugLog);
#endif
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
            if(factionHeirloom.gridTileLocation != null && !factionHeirloom.IsInStructureSpot() && 
               !partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Heirloom_Hunt) && !HasJob(JOB_TYPE.HUNT_HEIRLOOM)) {
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
            if (character != null) {
                bannedCharacters.Add(character);
            }
        }

        if (!isDisbanded) {
            //only load relationships if this faction is not yet disbanded since we remove relations with disbanded factions, 
            //this is currently a problem because we still load disbanded factions, since some things might still reference it.
            //And it is currently unsafe to completely remove disbanded factions from the all factions list. Change added June 29, 2021
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
        }
        

        // for (int i = 0; i < data.history.Count; i++) {
        //     Log log = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.history[i]);
        //     history.Add(log);
        // }

        for (int i = 0; i < data.ownedSettlementIDs.Count; i++) {
            BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.ownedSettlementIDs[i]);
            ownedSettlements.Add(settlement);
        }
        if (isMajorNonPlayer && isActive && !isDisbanded) {
            PathfindingTagPair pair = new PathfindingTagPair(data.pathfindingTag, data.pathfindingDoorTag);
            InnerMapManager.Instance.SetPathfindingTagPairAsClaimed(pair); 
        }
        if (isActive && !isDisbanded) {
            FactionEmblemRandomizer.SetEmblemAsUsed(emblem);
        }
        partyQuestBoard.LoadReferences(data.partyQuestBoard);
        successionComponent.LoadReferences(data.successionComponent);

    }
#endregion

    
}