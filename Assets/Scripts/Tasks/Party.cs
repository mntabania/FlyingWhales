﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Party: IEncounterable, ICombatInitializer {

    //public delegate void OnPartyFull(Party party);
    //public OnPartyFull onPartyFull;

    protected string _name;

    protected bool _isOpen; //is this party open to new members?
    protected bool _isDisbanded;

    protected ECS.Character _partyLeader;
    protected List<ECS.Character> _partyMembers; //Contains all party members including the party leader
    //protected List<ECS.Character> _partyMembersOnTheWay; //Party members that just joined, but are on the way to the party leaders location
	protected List<ECS.Character> _prisoners; //TODO: remove this, move to party leader
	protected List<ECS.Character> _followers;

    //protected CharacterTask _currentTask;

    protected ILocation _specificLocation;
	protected Region _currentRegion;

	protected bool _isDefeated;
    protected Dictionary<RACE, int> _civiliansByRace;

    private const int MAX_PARTY_MEMBERS = 5;

    //private Dictionary<MATERIAL, int> _materialInventory;

	private Action _currentFunction;
	private bool _isInCombat;

    #region getters/setters
    public string encounterName {
		get { return _name; }
    }
    public string name {
        get { return _name; }
    }
    public string nameWithRole {
        get {
            if (_partyLeader.role != null) {
                return Utilities.NormalizeString(_partyLeader.role.roleType.ToString()) + " " + _name;
            }
            return _name;
        }
    }
    public string urlName {
		get { return "[url=" + _partyLeader.id.ToString() + "_party]" + name + "[/url]"; }
	}
    public string urlNameWithRole {
        get { return "[url=" + _partyLeader.id.ToString() + "_party]" + nameWithRole + "[/url]"; }
    }
    public bool isFull {
        get { return partyMembers.Count >= MAX_PARTY_MEMBERS; }
    }
    public bool isOpen {
        get { return _isOpen; }
    }
    public bool isDisbanded {
        get { return _isDisbanded; }
    }
    public ECS.Character partyLeader {
        get { return _partyLeader; }
    }
    public List<ECS.Character> partyMembers {
        get { return _partyMembers; }
    }
    public List<ECS.Character> followers {
        get { return _followers; }
    }
    public CharacterTask currentTask {
        get { return _partyLeader.currentTask; }
	}
	public List<ECS.Character> prisoners {
		get { return _prisoners; }
	}
    public ILocation specificLocation {
        get { return _specificLocation; }
    }
    public HexTile currLocation {
		get { return this.specificLocation.tileLocation; }
    }
	public bool isDefeated {
		get { return _isDefeated; }
	}
	public int civilians{
		get { 
			if(_civiliansByRace == null){
				return 0;
			}
			return _civiliansByRace.Sum(x => x.Value); 
		}
	}
    public Dictionary<RACE, int> civiliansByRace {
        get { return _civiliansByRace; }
    }
    public Faction faction {
        get { return _partyLeader.faction; }
    }
    //public Dictionary<MATERIAL, int> materialInventory {
    //    get { return _materialInventory; }
    //}
    public CharacterAvatar avatar {
        get { return _partyLeader.avatar; }
    }
	public bool isInCombat{
		get { return _isInCombat; }
	}
	public Action currentFunction{
		get { return _currentFunction; }
	}
	public ECS.Character mainCharacter{
		get { return this._partyLeader; }
	}
	public int numOfCharacters{
		get { return _partyMembers.Count; }
	}
	public bool doesNotTakePrisoners{
		get { return _partyLeader.characterDoesNotTakePrisoners; }
	}
	public Region currentRegion{
		get { return _currentRegion; }
	}
    #endregion

    public Party(ECS.Character partyLeader, bool mustBeAddedToPartyList = true) {
		SetName(partyLeader.firstName + "'s Party");
        _partyLeader = partyLeader;
        _partyMembers = new List<ECS.Character>();
        //_partyMembersOnTheWay = new List<ECS.Character>();
		_prisoners = new List<ECS.Character> ();
		_followers = new List<ECS.Character> ();
		_isDefeated = false;
        _civiliansByRace = new Dictionary<RACE, int>();

        //Debug.Log(partyLeader.name + " has created " + _name);
        partyLeader.specificLocation.ReplaceCharacterAtLocation(partyLeader, this);

        //partyLeader.specificLocation.AddCharacterToLocation (this);
        //partyLeader.specificLocation.RemoveCharacterFromLocation(partyLeader);

        AddPartyMember(_partyLeader);
        //ConstructMaterialInventory();
		if(mustBeAddedToPartyList){
			PartyManager.Instance.AddParty(this);
		}
    }

	public void SetName(string name){
		_name = name;
	}
    public void SetSpecificLocation(ILocation specificLocation) {
        _specificLocation = specificLocation;
		if (_specificLocation != null) {
			_currentRegion = _specificLocation.tileLocation.region;
		}
    }
	public void SetIsDefeated(bool state){
		_isDefeated = state;
		for (int i = 0; i < _partyMembers.Count; i++) {
			_partyMembers [i].SetIsDefeated (state);
		}
	}
    #region Party Management
    /*
     Add a new party member.
         */
    public virtual void AddPartyMember(ECS.Character member) {
        if (!_partyMembers.Contains(member)) {
			if(member.party != null){
				member.party.DisbandParty ();
				Debug.Log ("DISBANDING " + member.party.name + " before adding " + member.name + " to " + _name);
			}else{
				if (member.avatar != null) {
					member.avatar.RemoveCharacter(member);
				}
			}
            _partyMembers.Add(member);
            //CreateRelationshipsForNewMember(member);
			if(member.id != _partyLeader.id){
				_followers.Add (member);
				if(_partyLeader.avatar != null){
					_partyLeader.avatar.AddNewCharacter (member);
				}
			}
            member.specificLocation.RemoveCharacterFromLocation(member);//Remove member from specific location, since it is already included in the party
            member.SetParty(this);

            if (!IsCharacterLeaderOfParty(member)) {
                member.SetCurrentTask(currentTask);
                member.SetFollowerState(true);
            }
        }
    }
    //public void AddPartyMemberAsOnTheWay(ECS.Character member) {
    //    _partyMembersOnTheWay.Add(member);
    //}
    //public void PartyMemberHasArrived(ECS.Character member) {
    //    _partyMembersOnTheWay.Remove(member);
    //}
    /*
     Remove a character from this party.
         */
    public virtual void RemovePartyMember(ECS.Character member, bool forDeath = false) {
        _partyMembers.Remove(member);
		_followers.Remove (member);
        if(member.avatar != null) {
			member.avatar.RemoveCharacter(member);
        }
		////If party is unaligned, change party leader immediately if party leader died
		//if(faction == null && member.id == _partyLeader.id && _partyMembers.Count > 0){
		//	_partyLeader = _partyMembers[0];
		//}

        if (!forDeath) { //If the member was removed from party, but did not die
			this.specificLocation.AddCharacterToLocation(member);
            Debug.Log(member.name + " has left the party of " + partyLeader.name);
            //if (currentTask != null && _currentTask.taskType == TASK_TYPE.QUEST) {
            //    ((OldQuest.Quest)currentTask).AddNewLog(member.name + " has left the party");
            //}
		}
        
        member.SetParty(null);
		member.SetCurrentTask (null);
		if(member.isFollower){
			member.SetFollowerState (false);
//			member.isFollowerOf.RemoveFollower (member);
			Settlement settlement = member.GetNearestSettlementFromFaction();
			if (settlement == null) {
				//TODO: This will always throw with monter parties, since monsters don't have factions. Handle that.
				throw new Exception(member.name + " cannot find a settlement from his/her faction!");
			}
			//will go back to the nearest settlement of their faction
			settlement.AdjustCivilians(member.raceSetting.race, 1);
		}
		if (_partyMembers.Count <= 0 || member.id == _partyLeader.id) {
            //JustDisbandParty ();
            DisbandParty();
        }
    }
	public void AddPrisoner(ECS.Character character){
		character.SetPrisoner (true, this);
		_prisoners.Add (character);
	}
	public void RemovePrisoner(ECS.Character character){
		_prisoners.Remove (character);
	}
    //public void CheckLeavePartyAfterQuest() {
    //    //Check which party members will leave
    //    List<ECS.Character> charactersToLeave = new List<ECS.Character>();
    //    Faction factionOfLeader = _partyLeader.faction;
    //    for (int i = 0; i < _partyMembers.Count; i++) {
    //        ECS.Character currMember = _partyMembers[i];
    //        Faction factionOfMember = currMember.faction;
    //        if (!IsCharacterLeaderOfParty(currMember)) {
				//if((currMember.role == null || currMember.role.roleType != CHARACTER_ROLE.TAMED_BEAST) && factionOfMember != null && factionOfLeader.id != factionOfMember.id) {//if the faction of the member is different from the faction of the leader
    //                FactionRelationship factionRel = FactionManager.Instance.GetRelationshipBetween(factionOfLeader, factionOfMember);
    //                if(factionRel != null && factionRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
    //                    //- if hostile, characters from both factions must leave party led by a character from the other faction after completing a quest
    //                    charactersToLeave.Add(currMember);
    //                    continue;
    //                }
    //            }
                
    //            WeightedDictionary<PARTY_ACTION> partyActionWeights = GetPartyActionWeightsForCharacter(currMember);
    //            if (partyActionWeights.PickRandomElementGivenWeights() == PARTY_ACTION.LEAVE) {
    //                charactersToLeave.Add(currMember);
    //            }
    //        }
    //    }

    //    for (int i = 0; i < charactersToLeave.Count; i++) {
    //        ECS.Character characterToLeave = charactersToLeave[i];
    //        RemovePartyMember(characterToLeave);
    //        characterToLeave.GoToNearestNonHostileSettlement(() => characterToLeave.OnReachNonHostileSettlementAfterQuest()); //Make the character that left, go home then decide a new action
    //    }
    //}
    /*
     This will disband this party.
     The Party will only disband if the party leader becomes imprisoned or dies.
     When a party is disbanded, all it's followers will become civilians again
     of the nearest settlement of it's faction.
         */
    public void DisbandParty() {
		if(_isDisbanded){
			return;
		}
        _isDisbanded = true;
        Debug.Log("Disbanded " + this.name);
        PartyManager.Instance.RemoveParty(this);
        if (!partyLeader.isDead) {
            RemovePartyMember(partyLeader);
        }
		//if (_currentTask != null) {
		//	if (!_currentTask.isDone) {
		//		_currentTask.EndTask(TASK_STATUS.CANCEL); //Cancel OldQuest.Quest if party is currently on a quest
		//	}
		//}
		//SetCurrentTask (null);

        //if this party has any prisoners
        for (int i = 0; i < _prisoners.Count; i++) {
            //check each of them
            ECS.Character currPrisoner = _prisoners[i];
            if (currPrisoner.isFollower) {
                //if they are just followers
                Settlement settlement = currPrisoner.GetNearestSettlementFromFaction();
                if (settlement == null) {
                    //TODO: This will always throw with monter parties, since monsters don't have factions. Handle that.
                    throw new Exception(currPrisoner.name + " cannot find a settlement from his/her faction!");
                }
                //convert them to civilians of the nearest settlement of their faction
                settlement.AdjustCivilians(currPrisoner.raceSetting.race, 1);
            } else {
                //if they are not followers
                //let them decide again, since they were set free
                currPrisoner.SetSpecificLocation(specificLocation);
                currPrisoner.DetermineAction();
            }
        }
            
		//_partyLeader.AdjustCivilians (_civiliansByRace);

        //all the remaining followers of the party
//        for (int i = 0; i < followers.Count; i++) {
//            ECS.Character currFollower = followers[i];
//            Settlement settlement = currFollower.GetNearestSettlementFromFaction();
//            if (settlement == null) {
//                //TODO: This will always throw with monter parties, since monsters don't have factions. Handle that.
//                throw new Exception(currFollower.name + " cannot find a settlement from his/her faction!");
//            }
//            //will go back to the nearest settlement of their faction
//            settlement.AdjustCivilians(currFollower.raceSetting.race, 1);
//        }

		while(_partyMembers.Count > 0){
			RemovePartyMember (_partyMembers [0]);
		}

		this.specificLocation.RemoveCharacterFromLocation (this);
    }
    
    public void SetOpenStatus(bool isOpen) {
        _isOpen = isOpen;
        //Do Nothing adventurers within the same city will be informed whenever a new character is registering for a Party. They will have first choice to join the party.

    }
    #endregion

    #region Quest
    ///*
    // Set the current task the party is on.
    // This will also set the current task of all
    // the characters in the party.
    //     */
    //public void SetCurrentTask(CharacterTask task) {
    //    _currentTask = task;
    //    for (int i = 0; i < _partyMembers.Count; i++) {
    //        ECS.Character currMember = _partyMembers[i];
    //        currMember.SetCurrentTask(task);
    //    }
    //    if(task == null) {
    //        Debug.Log("Set current quest of " + name + " to nothing");
    //    } else {
    //        if(task.taskType == TASK_TYPE.QUEST) {
    //            Debug.Log("Set current quest of " + name + " to " + ((OldQuest.Quest)task).questType.ToString());
    //        }
    //    }
        
    //}
    ///*
    // Make the party leader decide the next action for the party.
    //     */
    //public void DetermineNextAction() {
    //    _partyLeader.DetermineAction();
    //}
    ///*
    // This is called when the quest assigned to this party ends.
    //     */
    //public void OnQuestEnd(TASK_RESULT result) {
    //    AdjustRelationshipBasedOnQuestResult(result);
    //}
    #endregion

    #region Relationships
    public void CreateRelationshipsForNewMember(ECS.Character newMember) {
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currPartyMember = _partyMembers[i];
            if(newMember.GetRelationshipWith(currPartyMember) == null) {
                CharacterManager.Instance.CreateNewRelationshipBetween(currPartyMember, newMember);
            }
        }
    }
    /*
     Adjust the relationship of each party member with each other by an amount
         */
    public void AdjustPartyRelationships(int adjustment) {
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currPartyMember = _partyMembers[i];
            for (int j = 0; j < _partyMembers.Count; j++) {
                ECS.Character otherPartyMember = _partyMembers[j];
                if (currPartyMember.id != otherPartyMember.id) {
                    currPartyMember.GetRelationshipWith(otherPartyMember).AdjustValue(adjustment);
                }
            }
        }
    }
    private void AdjustRelationshipBasedOnQuestResult(TASK_STATUS result) {
        switch (result) {
            case TASK_STATUS.SUCCESS:
                AdjustPartyRelationships(5); //Succeeded in a OldQuest.Quest Together: +5 (cumulative)
                break;
            case TASK_STATUS.FAIL:
                AdjustPartyRelationships(-5); //Failed in a OldQuest.Quest Together: -5 (cumulative)
                break;
            default:
                break;
        }
    }
    #endregion

    #region Utilities
    internal bool IsCharacterLeaderOfParty(ECS.Character character) {
        return character.id == _partyLeader.id;
    }
    internal bool IsCharacterFollowerOfParty(ECS.Character character) {
        return followers.Contains(character);
    }
  //  public void GoBackToQuestGiver(TASK_STATUS taskResult) {
		//if(isInCombat){
		//	SetCurrentFunction (() => GoBackToQuestGiver (taskResult));
		//	return;
		//}
  //      if(currentTask == null || currentTask.taskType != TASK_TYPE.QUEST) {
  //          throw new Exception(this.name + " cannot go back to quest giver because the party has no quest!");
  //      }
  //      OldQuest.Quest currentQuest = (OldQuest.Quest)currentTask;
  //      if(_avatar == null) {
  //          _partyLeader.CreateNewAvatar();
  //      }
  //      if(currentQuest.postedAt == null) {
  //          throw new Exception("Posted at of quest " + currentQuest.questName + " is null!");
  //      }
		//if(_avatar.specificLocation.tileLocation.id == currentQuest.postedAt.tileLocation.id){
		//	currentQuest.TurnInQuest (taskResult);
		//}else{
		//	PATHFINDING_MODE pathMode = PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP;
		//	if(this.specificLocation is Settlement) {
		//		pathMode = PATHFINDING_MODE.MAJOR_ROADS; //if this party is at a settlement, use major roads
		//	}

		//	_avatar.SetTarget(currentQuest.postedAt);
		//	_avatar.StartPath(pathMode, () => currentQuest.TurnInQuest(taskResult));
		//}
  //  }
    ///*
    // This is the default action to be done when a 
    // party returns to the quest giver settlement after a quest.
    //     */
    //internal void OnQuestEnd() {
    //    AdjustRelationshipBasedOnQuestResult(currentTask.taskStatus);
    //    FactionManager.Instance.RemoveQuest((OldQuest.Quest)currentTask);
    //    if (_partyLeader.isDead) {
    //        //party leader is already dead!
    //        SetCurrentTask(null);
    //        DisbandParty();
    //    } else {
    //        //CheckLeavePartyAfterQuest();
    //        _partyLeader.DestroyAvatar();
    //        _partyLeader.DetermineAction();
    //    }
    //    //_currLocation.AddCharacterOnTile(this);
    //}
    public bool CanJoinParty(ECS.Character candidate) {
        if(isFull || !_isOpen) {
            return false; //cannot join party because it is already full or party is not open
        }
        Faction factionOfParty = _partyLeader.faction;
        Faction factionOfCandidate = candidate.faction;
        if(factionOfCandidate == null || factionOfParty == null) {
            return true; //one of the characters are factionless, allow join
        } else {
            if(factionOfCandidate.id == factionOfParty.id) {
                return true; //faction of party is the same as faction of candidate
            }
            FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(factionOfParty, factionOfCandidate);
            if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                return false; //candidate cannot join party, because faction of leader and his/her faction are in hostile relations
            }
            return true;
        }
    }
    public bool IsPartyWounded() {
        for (int i = 0; i < _partyMembers.Count; i++) {
            if (_partyMembers[i].currentHP < _partyMembers[i].maxHP) {
                return true;
            }
        }
        return false;
    }
    #endregion

    public ECS.Character GetCharacterByID(int id){
		if(_partyLeader.id == id){
			return _partyLeader;
		}
		for (int i = 0; i < _partyMembers.Count; i++) {
			if (_partyMembers [i].id == id){
				return _partyMembers [i];
			}
		}
		return null;
	}

	public ECS.Character GetPrisonerByID(int id){
		for (int i = 0; i < _prisoners.Count; i++) {
			if (_prisoners [i].id == id){
				return _prisoners [i];
			}
		}
		return null;
	}

	#region Virtuals
    public virtual void StartEncounter(ECS.Character encounteredBy){ }
	public virtual void StartEncounter(Party encounteredBy){}
    #endregion

    #region ICombatInitializer
    public virtual void ReturnResults(object result) { }
    //public virtual bool InitializeCombat(){
    //	if(isDefeated){
    //		return false;
    //	}
    //	if(_partyLeader.faction == null){
    //		ICombatInitializer enemy = this.specificLocation.GetCombatEnemy (this);
    //		if(enemy != null){
    //			ECS.CombatPrototype combat = new ECS.CombatPrototype (this, enemy, this.specificLocation);
    //			combat.AddCharacters (ECS.SIDES.A, this._partyMembers);
    //			if(enemy is Party){
    //				combat.AddCharacters (ECS.SIDES.B, ((Party)enemy).partyMembers);
    //			}else{
    //				combat.AddCharacters (ECS.SIDES.B, new List<ECS.Character>(){((ECS.Character)enemy)});
    //			}
    //			this.specificLocation.SetCurrentCombat (combat);
    //			CombatThreadPool.Instance.AddToThreadPool (combat);
    //			return true;
    //		}
    //		return false;
    //	}else{
    //		if(_partyLeader.role != null && _partyLeader.role.roleType == CHARACTER_ROLE.WARLORD){
    //			ICombatInitializer enemy = this.specificLocation.GetCombatEnemy (this);
    //			if(enemy != null){
    //				ECS.CombatPrototype combat = new ECS.CombatPrototype (this, enemy, this.specificLocation);
    //				combat.AddCharacters (ECS.SIDES.A, this._partyMembers);
    //				if(enemy is Party){
    //					combat.AddCharacters (ECS.SIDES.B, ((Party)enemy).partyMembers);
    //				}else{
    //					combat.AddCharacters (ECS.SIDES.B, new List<ECS.Character>(){((ECS.Character)enemy)});
    //				}
    //				this.specificLocation.SetCurrentCombat (combat);
    //				CombatThreadPool.Instance.AddToThreadPool (combat);
    //				return true;
    //			}
    //			return false;
    //		}
    //		return false;
    //	}
    //}
    public virtual bool IsHostileWith(ICombatInitializer combatInitializer) {
        if (this.faction == null) {
            return true; //this party has no faction
        }
        //Check here if the combatInitializer is hostile with this character, if yes, return true
        Faction factionOfEnemy = null;
        if (combatInitializer is ECS.Character) {
            factionOfEnemy = (combatInitializer as ECS.Character).faction;
        } else if (combatInitializer is Party) {
            factionOfEnemy = (combatInitializer as Party).faction;
        }
        if (factionOfEnemy != null) {
            if (factionOfEnemy.id == this.faction.id) {
                return false; //characters are of same faction
            }
            FactionRelationship rel = this.faction.GetRelationshipWith(factionOfEnemy);
            if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                return true; //factions of combatants are hostile
            }
            return false;
        } else {
            return true;
        }
    }
    public virtual void ReturnCombatResults(ECS.CombatPrototype combat) {
        this.SetIsInCombat(false);
        if (this.isDisbanded) {
            return;
        }
        if (this.isDefeated) {
            //this party was defeated
            if (partyMembers.Count > 0) {
                //the party was defeated in combat, but there are still members that are alive,
                //Check if the party leader is dead
                if (partyLeader.isDead) {
                    //if he/she is dead, disband this party
                    DisbandParty();
                } else {
                    ////the party leader is still alive
                    //if (currentFunction != null) {
                    //    currentFunction();
                    //    SetCurrentFunction(null);
                    //}

                    //check if he/she is not a prisoner
                    if (partyLeader.isPrisonerOf == null) {
                        //if he/she is not dead and is not a prisoner, it means that this party chose to flee combat
                        //when a party chooses to flee, it's current task will be considered as cancelled, and it will return
                        //to it's nearest non hostile location, and determine it's next action there
                        if (currentTask != null) {
                            currentTask.EndTask(TASK_STATUS.CANCEL);
                        }
                        BaseLandmark targetLocation = partyLeader.GetNearestLandmarkWithoutHostiles();
                        if(targetLocation == null) {
                            throw new Exception(this.name + " could not find a non hostile location to run to!");
                        } else {
                            partyLeader.GoToLocation(targetLocation, PATHFINDING_MODE.USE_ROADS, () => partyLeader.DetermineAction());
                        }
                    }
                }
            } else {
                //The party was defeated in combat, and no one survived, Disband this party.
                //JustDisbandParty();
                DisbandParty();
            }
        } else {
            //this party was not defeated in combat, resume this party's current action, if any.
            if (currentFunction != null) {
                currentFunction();
                SetCurrentFunction(null);
            } 
        }
        SetIsDefeated(false);
    }
    //public void SetCivilians(int amount){
    //	_civilians = amount;
    //}
    //public void AdjustCivilians(int amount){
    //	_civilians += amount;
    //	if(_civilians < 0){
    //		_civilians = 0;
    //	}
    //}
    public void AdjustCivilians(Dictionary<RACE, int> civilians) {
        foreach (KeyValuePair<RACE, int> kvp in civilians) {
            AdjustCivilians(kvp.Key, kvp.Value);
        }
    }
    public void ReduceCivilians(Dictionary<RACE, int> civilians) {
        foreach (KeyValuePair<RACE, int> kvp in civilians) {
            AdjustCivilians(kvp.Key, -kvp.Value);
        }
    }
    public void AdjustCivilians(RACE race, int amount) {
        if (!_civiliansByRace.ContainsKey(race)) {
            _civiliansByRace.Add(race, 0);
        }
        _civiliansByRace[race] += amount;
        _civiliansByRace[race] = Mathf.Max(0, _civiliansByRace[race]);
    }
    public void TransferCivilians(BaseLandmark to, Dictionary<RACE, int> civilians) {
        ReduceCivilians(civilians);
        to.AdjustCivilians(civilians);
    }
    public STANCE GetCurrentStance() {
        //TODO: Make this more elegant! Add a stance variable per quest type maybe?
        if (currentTask != null) {
            return currentTask.stance;
            //if (avatar != null && avatar.isTravelling) {
            //    if (currentTask is Attack || currentTask is Defend || currentTask is Pillage || currentTask is HuntPrey) {
            //        return STANCE.COMBAT;
            //    }
            //    return STANCE.NEUTRAL;
            //}
            //if (currentTask is Attack || currentTask is Defend || currentTask is Pillage || currentTask is HuntPrey) {
            //    return STANCE.COMBAT;
            //} else if (currentTask is Rest || currentTask is Hibernate || (currentTask is OldQuest.Quest && !(currentTask as OldQuest.Quest).isExpired) /*Forming Party*/ || currentTask is DoNothing) {
            //    return STANCE.NEUTRAL;
            //} else if (currentTask is ExploreTile) {
            //    return STANCE.STEALTHY;
            //}
        }
        return STANCE.NEUTRAL;
    }
    public void ContinueDailyAction() {
        if (!isInCombat) {
            if (currentTask != null) {
                if (avatar != null && avatar.isTravelling) {
                    return;
                }
                currentTask.PerformTask();
            }
        }
    }
    #endregion

    #region Combat Handlers
    public void SetIsInCombat (bool state){
		_isInCombat = state;
	}
	public void SetCurrentFunction (Action function){
		_currentFunction = function;
	}
	#endregion
}
