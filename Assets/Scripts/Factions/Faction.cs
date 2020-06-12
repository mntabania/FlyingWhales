/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Factions;
using Locations.Settlements;
using Traits;


public class Faction {
    public int id { get; protected set; }
    public string name { get; protected set; }
    public string description { get; protected set; }
    //public string initialLeaderClass { get; protected set; }
    public int level { get; protected set; }
    public bool isPlayerFaction { get; protected set; }
    public bool isMajorFaction { get; protected set; }
    public GENDER initialLeaderGender { get; protected set; }
    public RACE initialLeaderRace { get; protected set; }
    public RACE race { get; protected set; }
    public ILeader leader { get; protected set; }
    public Sprite emblem { get; protected set; }
    public List<BaseLandmark> ownedLandmarks { get; protected set; }
    public Color factionColor { get; protected set; }
    public List<Character> characters { get; protected set; }//List of characters that are part of the faction
    public List<BaseSettlement> ownedSettlements { get; protected set; }
    //public List<RACE> recruitableRaces { get; protected set; }
    //public List<RACE> startingFollowers { get; protected set; }
    public List<Character> bannedCharacters { get; protected set; }
    public Dictionary<Faction, FactionRelationship> relationships { get; protected set; }

    public MORALITY morality { get; private set; }
    public FACTION_SIZE size { get; private set; }
    public FACTION_TYPE factionType { get; private set; }
    public bool isActive { get; private set; }
    public List<Log> history { get; private set; }
    public FactionQuest activeFactionQuest { get; protected set; }

    //Components
    public FactionIdeologyComponent ideologyComponent { get; protected set; }

    private int newLeaderDesignationChance;
    private WeightedDictionary<Character> newLeaderDesignationWeights;

    #region getters/setters
    public string urlName {
        get { return $"<link={'"'}{this.id}_faction{'"'}>{this.name}</link>"; }
    }
    public bool isDestroyed {
        get { return characters.Count <= 0; }
    }
    public bool isMajorFriendlyNeutral {
        get { return isMajorFaction || this == FactionManager.Instance.friendlyNeutralFaction; }
    }
    public bool isMajorNonPlayerFriendlyNeutral {
        get { return isMajorNonPlayer || this == FactionManager.Instance.friendlyNeutralFaction; }
    }
    public bool isMajorNonPlayer {
        get { return isMajorFaction && !isPlayerFaction; }
    }
    #endregion

    public Faction(RACE race, bool isPlayerFaction = false) {
        this.isPlayerFaction = isPlayerFaction;
        this.id = UtilityScripts.Utilities.SetID<Faction>(this);
        SetName(RandomNameGenerator.GenerateKingdomName());
        SetEmblem(FactionManager.Instance.GenerateFactionEmblem(this));
        SetFactionColor(UtilityScripts.Utilities.GetColorForFaction());
        SetRace(race);
        SetMorality(MORALITY.GOOD);
        SetSize(FACTION_SIZE.MAJOR);
        SetFactionActiveState(true);
        level = 1;
        factionType = UtilityScripts.Utilities.GetRandomEnumValue<FACTION_TYPE>();
        characters = new List<Character>();
        ownedLandmarks = new List<BaseLandmark>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        //recruitableRaces = new List<RACE>();
        //startingFollowers = new List<RACE>();
        history = new List<Log>();
        newLeaderDesignationWeights = new WeightedDictionary<Character>();
        ideologyComponent = new FactionIdeologyComponent(this);
        ResetNewLeaderDesignationChance();
        AddListeners();
    }
    public Faction(SaveDataFaction data) {
        this.isPlayerFaction = data.isPlayerFaction;
        this.id = UtilityScripts.Utilities.SetID(this, data.id);
        SetName(data.name);
        SetDescription(data.description);
        SetEmblem(FactionManager.Instance.GetFactionEmblem(data.emblemIndex));
        SetFactionColor(data.factionColor);
        SetRace(data.race);
        SetMorality(data.morality);
        SetSize(data.size);
        SetFactionActiveState(data.isActive);
        //initialLeaderClass = data.initialLeaderClass;
        initialLeaderRace = data.initialLeaderRace;
        initialLeaderGender = data.initialLeaderGender;
        level = data.level;
        factionType = data.factionType;

        characters = new List<Character>();
        ownedLandmarks = new List<BaseLandmark>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        //recruitableRaces = new List<RACE>();
        //startingFollowers = new List<RACE>();
        history = new List<Log>();
        newLeaderDesignationWeights = new WeightedDictionary<Character>();
        ideologyComponent = new FactionIdeologyComponent(this);
        ResetNewLeaderDesignationChance();
        AddListeners();
    }

    #region Landmarks
    public void OwnLandmark(BaseLandmark landmark) {
        if (!ownedLandmarks.Contains(landmark)) {
            ownedLandmarks.Add(landmark);
        }
    }
    public void UnownLandmark(BaseLandmark landmark) {
        ownedLandmarks.Remove(landmark);
    }
    #endregion

    #region Characters
    public bool JoinFaction(Character character, bool broadcastSignal = true) {
        if (ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
            if (!characters.Contains(character)) {
                characters.Add(character);
                character.SetFaction(this);
                // if (this != FactionManager.Instance.neutralFaction && character.role == CharacterRole.BANDIT) {
                //     if (UnityEngine.Random.Range(0, 2) == 0) {
                //         character.AssignRole(CharacterRole.SOLDIER);
                //     } else {
                //         character.AssignRole(CharacterRole.ADVENTURER);
                //     }
                // }
                if (broadcastSignal) {
                    Messenger.Broadcast(Signals.CHARACTER_ADDED_TO_FACTION, character, this);
                }
            }
            return true;
        }
        //else {
        //  if (GameManager.Instance.gameHasStarted) {
        //      Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cannot_join_faction");
        //      log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //      log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
        //      character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        //  }
        //}
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
        //if (_leader != null && character.id == _leader.id) {
        //    SetLeader(null);
        //}
    }
    public void ClearAllDeadCharactersFromFaction() {
        for (int i = 0; i < characters.Count; i++) {
            if (characters[i].isDead) {
                if (LeaveFaction(characters[i])) {
                    i--;
                }
            }
        }
    }

    // public List<Character> GetCharactersOfType(CHARACTER_ROLE role) {
    //     List<Character> chars = new List<Character>();
    //     for (int i = 0; i < characters.Count; i++) {
    //         Character currCharacter = characters[i];
    //         if (currCharacter.role.roleType == role) {
    //             chars.Add(currCharacter);
    //         }
    //     }
    //     return chars;
    // }
    // public List<Character> GetViableCharacters(GENDER gender, params CHARACTER_ROLE[] role) {
    //     List<Character> chars = new List<Character>();
    //     for (int i = 0; i < characters.Count; i++) {
    //         Character currCharacter = characters[i];
    //         if (currCharacter.gender == gender && !currCharacter.isDead && !currCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && !currCharacter.isCriminal) {
    //             for (int j = 0; j < role.Length; j++) {
    //                 if (currCharacter.role.roleType == role[j]) {
    //                     chars.Add(currCharacter);
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    //     return chars;
    // }
    public List<Character> GetViableCharacters(GENDER gender, params string[] classNames) {
        List<Character> chars = new List<Character>();
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            if (currCharacter.gender == gender && !currCharacter.isDead && currCharacter.canPerform && !currCharacter.traitContainer.HasTrait("Criminal")) { //!currCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                for (int j = 0; j < classNames.Length; j++) {
                    if (currCharacter.characterClass.className == classNames[j]) {
                        chars.Add(currCharacter);
                        break;
                    }
                }
            }
        }
        return chars;
    }
    //public void SetNewLeader() {
    //    if (leader != null) {
    //        Character previousRuler = leader as Character;
    //        Character newRuler = null;
    //        string log = GameManager.Instance.TodayLogString() + name + " faction is deciding a new leader...";

    //        log += "\nChecking male relatives of the king...";
    //        List<Character> maleRelatives = FactionManager.Instance.GetViableRulers(previousRuler, GENDER.MALE, RELATIONSHIP_TYPE.RELATIVE);
    //        if (maleRelatives.Count > 0) {
    //            newRuler = maleRelatives[UnityEngine.Random.Range(0, maleRelatives.Count)];
    //            log += "\nNew Ruler: " + newRuler.name;
    //        } else {
    //            log += "\nChecking male nobles...";
    //            List<Character> maleNobles = GetViableCharacters(GENDER.MALE, "Noble");
    //            if (maleNobles.Count > 0) {
    //                newRuler = maleNobles[UnityEngine.Random.Range(0, maleNobles.Count)];
    //                log += "\nNew Ruler: " + newRuler.name;
    //            } else {
    //                log += "\nChecking female relatives of the king...";
    //                List<Character> femaleRelatives = FactionManager.Instance.GetViableRulers(previousRuler, GENDER.FEMALE, RELATIONSHIP_TYPE.RELATIVE);
    //                if (femaleRelatives.Count > 0) {
    //                    newRuler = femaleRelatives[UnityEngine.Random.Range(0, femaleRelatives.Count)];
    //                    log += "\nNew Ruler: " + newRuler.name;
    //                } else {
    //                    log += "\nChecking female nobles...";
    //                    List<Character> femaleNobles = GetViableCharacters(GENDER.FEMALE, "Noble");
    //                    if (femaleNobles.Count > 0) {
    //                        newRuler = femaleNobles[UnityEngine.Random.Range(0, femaleNobles.Count)];
    //                        log += "\nNew Ruler: " + newRuler.name;
    //                    } else {
    //                        log += "\nChecking male soldiers and adventurers...";
    //                        List<Character> maleSoldiersAndAdventurers = GetViableCharacters(GENDER.MALE, CHARACTER_ROLE.SOLDIER, CHARACTER_ROLE.ADVENTURER);
    //                        if (maleSoldiersAndAdventurers.Count > 0) {
    //                            newRuler = maleSoldiersAndAdventurers[UnityEngine.Random.Range(0, maleSoldiersAndAdventurers.Count)];
    //                            log += "\nNew Ruler: " + newRuler.name;
    //                        } else {
    //                            log += "\nChecking female soldiers and adventurers...";
    //                            List<Character> femaleSoldiersAndAdventurers = GetViableCharacters(GENDER.FEMALE, CHARACTER_ROLE.SOLDIER, CHARACTER_ROLE.ADVENTURER);
    //                            if (femaleSoldiersAndAdventurers.Count > 0) {
    //                                newRuler = femaleSoldiersAndAdventurers[UnityEngine.Random.Range(0, femaleSoldiersAndAdventurers.Count)];
    //                                log += "\nNew Ruler: " + newRuler.name;
    //                            } else {
    //                                log += "\nChecking male civilians...";
    //                                List<Character> maleCivilians = GetViableCharacters(GENDER.MALE, CHARACTER_ROLE.CIVILIAN);
    //                                if (maleCivilians.Count > 0) {
    //                                    newRuler = maleCivilians[UnityEngine.Random.Range(0, maleCivilians.Count)];
    //                                    log += "\nNew Ruler: " + newRuler.name;
    //                                } else {
    //                                    log += "\nChecking female civilians...";
    //                                    List<Character> femaleCivilians = GetViableCharacters(GENDER.FEMALE, CHARACTER_ROLE.CIVILIAN);
    //                                    if (femaleCivilians.Count > 0) {
    //                                        newRuler = femaleCivilians[UnityEngine.Random.Range(0, femaleCivilians.Count)];
    //                                        log += "\nNew Ruler: " + newRuler.name;
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        previousRuler.PrintLogIfActive(log);
    //        if (newRuler != null) {
    //            SetLeader(newRuler);

    //            Log logNotif = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_faction_leader");
    //            logNotif.AddToFillers(newRuler, newRuler.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            logNotif.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
    //            newRuler.AddHistory(logNotif);
    //            PlayerManager.Instance.player.ShowNotification(logNotif);
    //        }
    //        //else {
    //        //    Debug.LogError(GameManager.Instance.TodayLogString() + name + " couldn't set a new leader replacing " + previousRuler.name);
    //        //}
    //    } else {
    //        Debug.LogError(GameManager.Instance.TodayLogString() + name + " cannot set new leader because there is no previous leader!");
    //    }
    //}
    public void OnlySetLeader(ILeader newLeader) {
        if(leader != newLeader) {
            ILeader prevLeader = leader;
            leader = newLeader;
            if(prevLeader != null && prevLeader is Character prevCharacterLeader) {
                if (!prevCharacterLeader.isSettlementRuler) {
                    prevCharacterLeader.jobComponent.RemovePriorityJob(JOB_TYPE.JUDGE_PRISONER);
                }
            }
            if(leader != null) {
                if(leader is Character characterLeader) {
                    characterLeader.jobComponent.AddPriorityJob(JOB_TYPE.JUDGE_PRISONER);
                }
            }
            if (newLeader is Character character) {
                Messenger.Broadcast(Signals.ON_SET_AS_FACTION_LEADER, character);
            }
        }
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        CheckIfCharacterStillFitsIdeology(character);
    }
    private void OnCharacterRaceChange(Character character) {
        CheckIfCharacterStillFitsIdeology(character);
    }
    private void OnCharacterRemoved(Character character) {
        LeaveFaction(character);
    }
    //Returns true if character left the faction, otherwise return false
    public bool CheckIfCharacterStillFitsIdeology(Character character, bool willLog = true) {
        if (character.faction == this && !ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
            if (willLog) {
                return character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_not_fit");
            } else {
                return character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
            }
            //character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
            //Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "left_faction_not_fit");
            //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
            //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
        return false;
    }
    public bool IsCharacterBannedFromJoining(Character character) {
        return HasCharacterBeenBanned(character);
    }
    public bool HasCharacterBeenBanned(Character character) {
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
            //Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "kick_out_faction_character");
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
    public void SetLeader(ILeader newLeader, bool setIdeology = true) {
        if(!isMajorFaction && !isPlayerFaction) {
            //Neutral, Friendly Neutral, Disguised Factions cannot have a leader
            return;
        }
        //if (this.leader != null && this.leader is Character) {
        //    Character previousRuler = this.leader as Character;
        //    //if (previousRuler.role.roleType != CHARACTER_ROLE.NOBLE) {
        //    //    previousRuler.AssignRole(CharacterRole.NOBLE);
        //    //    previousRuler.AssignClassByRole(previousRuler.role);
        //    //}
        //    //if (previousRuler.characterClass.className != previousRuler.GetClassForRole(previousRuler.role)) {
        //    //    previousRuler.AssignClassByRole(previousRuler.role);
        //    //}
        //    previousRuler.UnassignBuildStructureComponent();
        //}
        //if (newLeader != null && newLeader is Character) {
        //    Character newRuler = newLeader as Character;
        //    if (newRuler.role.roleType != CHARACTER_ROLE.LEADER) {
        //        newRuler.AssignRole(CharacterRole.LEADER);
        //        newRuler.AssignClassByRole(newRuler.role);
        //    }
        //    newRuler.AssignBuildStructureComponent();
        //    //if (newRuler.characterClass.className != initialLeaderClass) {
        //    //    newRuler.AssignClass(CharacterManager.Instance.CreateNewCharacterClass(initialLeaderClass));
        //    //}
        //}
        OnlySetLeader(newLeader);

        //Every time the leader changes, faction ideology changes
        if (!isPlayerFaction) {
            if (newLeader != null) {
                if (newLeader is Character) {
                    Character newRuler = newLeader as Character;
                    //newRuler.AssignBuildStructureComponent();
                    if (setIdeology) {
                        ideologyComponent.RerollIdeologies();
                    }
                    newRuler.currentRegion.AddFactionHere(this);
                    //ResetNewLeaderDesignationChance();
                    if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                        Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
                    }
                    //Messenger.Broadcast(Signals.ON_SET_AS_FACTION_LEADER, newRuler);
                }
                //if(characterLeader.currentRegion != null) {
                //    characterLeader.currentRegion.AddFactionHere(characterLeader.faction);
                //} else if (characterLeader.currentArea != null) {
                //    characterLeader.currentArea.region.AddFactionHere(characterLeader.faction);
                //}
            } else {
                Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
            }
        }
    }
    private void CheckForNewLeaderDesignation() {
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new faction leader designation for {name}";
        debugLog += $"\n-Chance: {newLeaderDesignationChance}";
        int chance = UnityEngine.Random.Range(0, 100);
        debugLog += $"\n-Roll: {chance}";
        Debug.Log(debugLog);
        if (chance < newLeaderDesignationChance) {
            DesignateNewLeader();
        } else {
            newLeaderDesignationChance += 2;
        }
    }
    public void DesignateNewLeader(bool willLog = true) {
        string log =
            $"Designating a new npcSettlement faction leader for: {name}(chance it triggered: {newLeaderDesignationChance})";
        newLeaderDesignationWeights.Clear();
        for (int i = 0; i < characters.Count; i++) {
            Character member = characters[i];
            log += $"\n\n-{member.name}";
            if (member.isDead || member.isMissing || member.isBeingSeized) {
                log += "\nEither dead or missing or seized, will not be part of candidates for faction leader";
                continue;
            }
            int weight = 50;
            log += "\n  -Base Weight: +50";
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
                weight += (numberOfFriends * 20);
                log +=
                    $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{(numberOfFriends * 20)}";
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
            if (member.traitContainer.HasTrait("Ugly")) {
                weight += -20;
                log += "\n  -Ugly: -20";
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
                    SetLeader(chosenLeader, false);
                    ideologyComponent.RerollIdeologies(willLog);
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
    #endregion

    #region Utilities
    private void AddListeners() {
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //if (!_isPlayerFaction) {
        //    Messenger.AddListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        //}
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //if (!_isPlayerFaction) {
        //    Messenger.RemoveListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        //}
    }
    public void SetRace(RACE race) {
        this.race = race;
    }
    public void SetSize(FACTION_SIZE size) {
        this.size = size;
    }
    public void SetFactionColor(Color color) {
        factionColor = color;
    }
    public void SetName(string name) {
        this.name = name;
    }
    public void SetDescription(string description) {
        this.description = description;
    }
    public void SetInitialFactionLeaderGender(GENDER gender) {
        initialLeaderGender = gender;
    }
    public void SetIsMajorFaction(bool state) {
        isMajorFaction = state;
    }
    //public void SetInitialFactionLeaderClass(string className) {
    //    initialLeaderClass = className;
    //}
    public void SetInitialFactionLeaderRace(RACE race) {
        initialLeaderRace = race;
    }
    public Character GetCharacterByID(int id) {
        for (int i = 0; i < characters.Count; i++) {
            if (characters[i].id == id) {
                return characters[i];
            }
        }
        return null;
    }
    public bool IsHostileWith(Faction faction) {
        if (faction.id == this.id) {
            return false;
        }
        FactionRelationship rel = GetRelationshipWith(faction);
        return rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE;
    }
    public bool HasLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return true;
            }
        }
        return false;
    }
    public override string ToString() {
        return name;
    }
    //private void OnCharacterDied(Character characterThatDied) {
    //    if (leader != null && leader is Character && leader.id == characterThatDied.id) {
    //        Debug.Log(this.name + "'s Leader that died was " + characterThatDied.name);
    //        OnLeaderDied();
    //    }
    //}
    //private void OnLeaderDied() {
    //    Debug.Log(this.name + "'s leader died");
    //    SetNewLeader();
    //    //Messenger.Broadcast(Signals.FACTION_LEADER_DIED, this);
    //}
    public void SetLevel(int amount) {
        level = amount;
    }
    public void LevelUp() {
        level++;
    }
    public void LevelUp(int amount) {
        level += amount;
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
    #endregion

    #region Relationships
    public void AddNewRelationship(Faction relWith, FactionRelationship relationship) {
        if (!relationships.ContainsKey(relWith)) {
            relationships.Add(relWith, relationship);
        } else {
            throw new System.Exception(
                $"{this.name} already has a relationship with {relWith.name}, but something is trying to create a new one!");
        }
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
    public void AdjustRelationshipFor(Faction otherFaction, int adjustment) {
        if (relationships.ContainsKey(otherFaction)) {
            relationships[otherFaction].AdjustRelationshipStatus(adjustment);
        } else {
            Debug.LogWarning($"There is no key for {otherFaction.name} in {this.name}'s relationship dictionary");
        }
    }
    public void SetRelationshipFor(Faction otherFaction, FACTION_RELATIONSHIP_STATUS status) {
        if (relationships.ContainsKey(otherFaction)) {
            relationships[otherFaction].SetRelationshipStatus(status);
        } else {
            Debug.LogWarning($"There is no key for {otherFaction.name} in {this.name}'s relationship dictionary");
        }
    }
    public bool IsAtWar() {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (kvp.Key.isActive && kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Death
    public void Death() {
        RemoveListeners();
        FactionManager.Instance.RemoveRelationshipsWith(this);
    }
    #endregion

    #region Landmarks
    public BaseLandmark GetOwnedLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return currLandmark;
            }
        }
        return null;
    }
    #endregion

    #region Areas
    public void AddToOwnedSettlements(BaseSettlement settlement) {
        if (!ownedSettlements.Contains(settlement)) {
            ownedSettlements.Add(settlement);
            Messenger.Broadcast(Signals.FACTION_OWNED_REGION_ADDED, this, settlement);
        }
    }
    public void RemoveFromOwnedSettlements(BaseSettlement settlement) {
        if (ownedSettlements.Remove(settlement)) {
            Messenger.Broadcast(Signals.FACTION_OWNED_REGION_REMOVED, this, settlement);
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
    #endregion

    #region Emblems
    public void SetEmblem(Sprite sprite) {
        emblem = sprite;
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Logs
    public void AddHistory(Log log) {
        if (!history.Contains(log)) {
            history.Add(log);
            if (this.history.Count > 60) {
                //if (this.history[0].node != null) {
                //    this.history[0].node.AdjustReferenceCount(-1);
                //}
                this.history.RemoveAt(0);
            }
            //if (log.node != null) {
            //    log.node.AdjustReferenceCount(1);
            //}
            //Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
    }
    #endregion

    #region Quests
    public void CreateAndSetActiveQuest(string name, Region region) {
        var typeName = $"{UtilityScripts.Utilities.RemoveAllWhiteSpace(name)}Quest";
        System.Type type = System.Type.GetType(typeName);
        FactionQuest factionQuest = null;
        if(type != null) {
            factionQuest = System.Activator.CreateInstance(type, this, region) as FactionQuest;
        } else {
            factionQuest = new FactionQuest(this, region);
        }
        SetActiveQuest(factionQuest);
    }
    public void SetActiveQuest(FactionQuest factionQuest) {
        if(activeFactionQuest != null) {
            activeFactionQuest.FinishQuest();
        }
        activeFactionQuest = factionQuest;
        if(activeFactionQuest != null) {
            activeFactionQuest.ActivateQuest();
        }
    }
    #endregion
}
public struct FactionTaskWeight {
    public int baseWeight; //Must not be changed by npcSettlement
    public int areaWeight;
    public int supplyCost;
    public bool areaCannotDoTask;
    public bool factionCannotDoTask;
}
