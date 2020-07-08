using UnityEngine;
using System.Collections.Generic;
using Factions;
using Factions.Faction_Types;
using Locations.Settlements;

public class Faction {
    public int id { get; }
    public string name { get; private set; }
    public string description { get; private set; }
    public bool isPlayerFaction => factionType.type == FACTION_TYPE.Demons;
    public bool isMajorFaction { get; private set; }
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
                    return RACE.NONE;
            }
        }
    } 
    public ILeader leader { get; private set; }
    public Sprite emblem { get; private set; }
    public Color factionColor { get; private set; }
    public List<Character> characters { get; }//List of characters that are part of the faction
    public List<BaseSettlement> ownedSettlements { get; }
    public List<Character> bannedCharacters { get; }
    public Dictionary<Faction, FactionRelationship> relationships { get; }
    public FactionType factionType { get; }
    public bool isActive { get; private set; }
    public List<Log> history { get; }
    public FactionQuest activeFactionQuest { get; protected set; }
    public FactionIdeologyComponent ideologyComponent { get; }

    private int newLeaderDesignationChance;
    private WeightedDictionary<Character> newLeaderDesignationWeights;

    #region getters/setters
    public bool isDestroyed => characters.Count <= 0;
    public bool isMajorFriendlyNeutral => isMajorFaction || this == FactionManager.Instance.vagrantFaction;
    public bool isMajorNonPlayerFriendlyNeutral => isMajorNonPlayer || this == FactionManager.Instance.vagrantFaction;
    public bool isMajorNonPlayer => isMajorFaction && !isPlayerFaction;
    #endregion

    public Faction(FACTION_TYPE _factionType) {
        id = UtilityScripts.Utilities.SetID<Faction>(this);
        SetName(RandomNameGenerator.GenerateKingdomName());
        SetEmblem(FactionManager.Instance.GenerateFactionEmblem(this));
        SetFactionColor(UtilityScripts.Utilities.GetColorForFaction());
        SetFactionActiveState(true);
        factionType = FactionManager.Instance.CreateFactionType(_factionType);
        characters = new List<Character>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        history = new List<Log>();
        newLeaderDesignationWeights = new WeightedDictionary<Character>();
        ideologyComponent = new FactionIdeologyComponent(this);
        ResetNewLeaderDesignationChance();
        AddListeners();
    }
    public Faction(SaveDataFaction data) {
        id = UtilityScripts.Utilities.SetID(this, data.id);
        SetName(data.name);
        SetDescription(data.description);
        SetEmblem(FactionManager.Instance.GetFactionEmblem(data.emblemIndex));
        SetFactionColor(data.factionColor);
        SetFactionActiveState(data.isActive);

        characters = new List<Character>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedSettlements = new List<BaseSettlement>();
        bannedCharacters = new List<Character>();
        history = new List<Log>();
        newLeaderDesignationWeights = new WeightedDictionary<Character>();
        ideologyComponent = new FactionIdeologyComponent(this);
        ResetNewLeaderDesignationChance();
        AddListeners();
    }

    #region Characters
    public bool JoinFaction(Character character, bool broadcastSignal = true) {
        if (ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
            if (!characters.Contains(character)) {
                characters.Add(character);
                character.SetFaction(this);
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
            //Once a character leave a faction and that faction is the owner of the home settlement, leave the settlement also
            //Reason: One village = One faction, no other faction can co exist in a village, for simplification
            if (character.homeSettlement != null && character.homeSettlement.owner != null && character.homeSettlement.owner == this) {
                character.MigrateHomeStructureTo(null);
            }
            Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_FACTION, character, this);
            return true;
        }
        return false;
    }
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
                return character.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
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
    public void SetLeader(ILeader newLeader) {
        if(!isMajorFaction && !isPlayerFaction) {
            //Neutral, Friendly Neutral, Disguised Factions cannot have a leader
            return;
        }
        OnlySetLeader(newLeader);

        if (!isPlayerFaction) {
            if (newLeader != null) {
                if (newLeader is Character newRuler) {
                    newRuler.currentRegion.AddFactionHere(this);
                    Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
                }
            } else {
                //if no leader was set, then roll every hour for a chance to designate a new leader.
                Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
            }
        }
    }
    private void CheckForNewLeaderDesignation() {
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new faction leader designation for {name}";
        debugLog += $"\n-Chance: {newLeaderDesignationChance.ToString()}";
        int chance = Random.Range(0, 100);
        debugLog += $"\n-Roll: {chance.ToString()}";
        Debug.Log(debugLog);
        if (chance < newLeaderDesignationChance) {
            DesignateNewLeader();
        } else {
            newLeaderDesignationChance += 2;
        }
    }
    public void DesignateNewLeader(bool willLog = true) {
        string log =
            $"Designating a new npcSettlement faction leader for: {name}(chance it triggered: {newLeaderDesignationChance.ToString()})";
        newLeaderDesignationWeights.Clear();
        for (int i = 0; i < characters.Count; i++) {
            Character member = characters[i];
            log += $"\n\n-{member.name}";
            if (member.isDead /*|| member.isMissing*/ || member.isBeingSeized) {
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
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
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
        return rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile;
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
            if (kvp.Key.isActive && kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
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
        var typeName = $"{UtilityScripts.Utilities.RemoveAllWhiteSpace(name)}Quest, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
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
