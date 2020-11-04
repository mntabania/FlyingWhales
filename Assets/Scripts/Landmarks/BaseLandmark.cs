/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

//[System.Serializable]
public class BaseLandmark : ILocation, TaskCreator {
    protected int _id;
    protected HexTile _location;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected List<BaseLandmark> _connections;
    protected bool _canBeOccupied; //can the landmark be occupied?
    protected bool _isOccupied;
    protected string _landmarkName;
    protected Faction _owner;
    protected List<Character> _charactersWithHomeOnLandmark;
    //protected Dictionary<RACE, int> _civiliansByRace;
    //protected int _currDurability;
    //protected int _totalDurability;
    protected List<TECHNOLOGY> _technologiesOnLandmark;
    protected Dictionary<TECHNOLOGY, bool> _technologies; //list of technologies and whether or not the landmark has that type of technology
    protected LandmarkObject _landmarkObject;
    protected List<Character> _prisoners; //list of prisoners on landmark
    protected List<Log> _history;
    protected int _combatHistoryID;
    protected Dictionary<int, Combat> _combatHistory;
    protected List<Character> _charactersAtLocation;
    protected List<Item> _itemsInLandmark;
    protected Dictionary<Character, GameDate> _characterTraces; //Lasts for 60 days
    protected List<LANDMARK_TAG> _landmarkTags;
    protected StructureObj _landmarkObj;
    //protected List<IObject> _objects;
    private bool _hasScheduledCombatCheck = false;
    private Dictionary<RESOURCE, int> _resourceInventory;
    private List<HexTile> _nextCorruptedTilesToCheck;
    private bool _hasBeenCorrupted;
    protected bool _isAttackingAnotherLandmark;
    //private List<HexTile> _diagonalRightTiles;
    //private List<HexTile> _diagonalLeftTiles;
    //private List<HexTile> _horizontalTiles;
    private List<HexTile> _wallTiles;
    public bool hasAdjacentCorruptedLandmark;
    //private int _diagonalLeftBlocked;
    //private int _diagonalRightBlocked;
    //private int _horizontalBlocked;
    //private Dictionary<BaseLandmark, string> _blockedLandmarkDirection;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public string locationName {
        get { return landmarkName; }
    }
    public string landmarkName {
		get { return _landmarkName; }
	}
	public string urlName {
		get { return "<link=" + '"' + this._id.ToString() + "_landmark" + '"' + ">" + _landmarkName + "</link>"; }
	}
    public LANDMARK_TYPE specificLandmarkType {
        get { return _specificLandmarkType; }
    }
    public List<BaseLandmark> connections {
        get { return _connections; }
    }
    public bool canBeOccupied {
        get { return _canBeOccupied; }
    }
    public bool isOccupied {
        get { return _isOccupied; }
    }
    public Faction owner {
        get { return _owner; }
    }
  //  public virtual int totalPopulation {
		//get { return civilians + CharactersCount(); }
  //  }
	//public int civilians {
	//	get { return _civiliansByRace.Sum(x => x.Value); }
 //   }
 //   public Dictionary<RACE, int> civiliansByRace {
 //       get { return _civiliansByRace; }
 //   }
    public Dictionary<TECHNOLOGY, bool> technologies {
        get { return _technologies; }
    }
    public LandmarkObject landmarkObject {
        get { return _landmarkObject; }
    }
	public List<Character> prisoners {
		get { return _prisoners; }
	}
	public List<Log> history{
		get { return this._history; }
	}
	public Dictionary<int, Combat> combatHistory {
		get { return _combatHistory; }
	}
    public List<Character> charactersAtLocation {
        get { return _charactersAtLocation; }
    }
	public HexTile tileLocation{
		get { return _location; }
	}
	public LOCATION_IDENTIFIER locIdentifier{
		get { return LOCATION_IDENTIFIER.LANDMARK; }
	}
	public List<Item> itemsInLandmark {
		get { return _itemsInLandmark; }
	}
    public int currDurability {
        get { return _landmarkObj.currentHP; }
    }
    public int totalDurability {
		get { return _landmarkObj.maxHP; }
    }
	public Dictionary<Character, GameDate> characterTraces {
		get { return _characterTraces; }
	}
    public StructureObj landmarkObj {
        get { return _landmarkObj; }
    }
    public List<HexTile> wallTiles {
        get { return _wallTiles; }
    }
    public bool isAttackingAnotherLandmark {
        get { return _isAttackingAnotherLandmark; }
    }
    //public List<IObject> objects {
    //    get { return _objects; }
    //}
    //public int diagonalLeftBlocked {
    //    get { return _diagonalLeftBlocked; }
    //}
    //public int diagonalRightBlocked {
    //    get { return _diagonalRightBlocked; }
    //}
    //public int horizontalBlocked {
    //    get { return _horizontalBlocked; }
    //}
    #endregion

    public BaseLandmark() {
        _connections = new List<BaseLandmark>();
        _owner = null; //landmark has no owner yet
        _charactersWithHomeOnLandmark = new List<Character>();
        _prisoners = new List<Character>();
        _history = new List<Log>();
        _combatHistory = new Dictionary<int, Combat>();
        _combatHistoryID = 0;
        _charactersAtLocation = new List<Character>();
        _itemsInLandmark = new List<Item>();
        _characterTraces = new Dictionary<Character, GameDate>();
        //_totalDurability = landmarkData.hitPoints;
        //_currDurability = _totalDurability;
        //_objects = new List<IObject>();
        _nextCorruptedTilesToCheck = new List<HexTile>();
        _hasBeenCorrupted = false;
        //_diagonalLeftTiles = new List<HexTile>();
        //_diagonalRightTiles = new List<HexTile>();
        //_horizontalTiles = new List<HexTile>();
        _wallTiles = new List<HexTile>();
        hasAdjacentCorruptedLandmark = false;
        //_diagonalLeftBlocked = 0;
        //_diagonalRightBlocked = 0;
        //_horizontalBlocked = 0;
        //_blockedLandmarkDirection = new Dictionary<BaseLandmark, string>();
        //Messenger.AddListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);
        //Messenger.AddListener<BaseLandmark>("StopCorruption", ALandmarkHasStoppedCorruption);

        //ConstructResourceInventory();
    }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this(){
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        _landmarkName = RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType);
        ConstructTags(landmarkData);
        ConstructTechnologiesDictionary();
        //ConstructCiviliansDictionary();
        //GenerateCivilians();
        SpawnInitialLandmarkItems();
    }
    public BaseLandmark(HexTile location, LandmarkSaveData data) : this(){
        _id = Utilities.SetID(this, data.landmarkID);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        _landmarkName = data.landmarkName;

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        ConstructTechnologiesDictionary();
        //ConstructCiviliansDictionary();
        //GenerateCivilians();
        SpawnInitialLandmarkItems();
    }

    #region Virtuals
    public virtual void Initialize() {}
	public virtual void DestroyLandmark(bool putRuinStructure){}
    /*
     What should happen when a character searches this landmark
         */
    public virtual void SearchLandmark(Character character) { }
	#endregion

    #region Connections
    public void AddConnection(BaseLandmark connection) {
        if (!_connections.Contains(connection)) {
            _connections.Add(connection);
        }
    }
    public bool IsConnectedTo(Region region) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.tileLocation.region.id == region.id) {
                return true;
            }
        }
        return false;
    }
    public bool IsConnectedTo(BaseLandmark landmark) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.id == landmark.id) {
                return true;
            }
        }
        return false;
    }
    public bool IsIndirectlyConnectedTo(Region region) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.IsConnectedTo(region)) {
                return true;
            }
        }
        return false;
    }
    public bool IsIndirectlyConnectedTo(BaseLandmark landmark) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.IsConnectedTo(landmark)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Ownership
    public virtual void OccupyLandmark(Faction faction) {
        _owner = faction;
        _isOccupied = true;
        _location.Occupy();
        EnableInitialTechnologies(faction);
        _owner.OwnLandmark(this);
    }
    public virtual void UnoccupyLandmark() {
        if(_owner == null) {
            throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
        }
        _isOccupied = false;
        _location.Unoccupy();
        DisableInititalTechnologies(_owner);
        _owner = null;
    }
	public void ChangeOwner(Faction newOwner){
		_owner = newOwner;
		_isOccupied = true;
		_location.Occupy();
		EnableInitialTechnologies(newOwner);
	}
    #endregion

    #region Technologies
    /*
     Initialize the technologies dictionary with all the available technologies
     and set them as disabled.
         */
    private void ConstructTechnologiesDictionary() {
        TECHNOLOGY[] allTechnologies = Utilities.GetEnumValues<TECHNOLOGY>();
        _technologies = new Dictionary<TECHNOLOGY, bool>();
        for (int i = 0; i < allTechnologies.Length; i++) {
            TECHNOLOGY currTech = allTechnologies[i];
            _technologies.Add(currTech, false);
        }
    }
    /*
     Set the initial technologies of a faction as enabled on this landmark.
         */
    private void EnableInitialTechnologies(Faction faction) {
        SetTechnologyState(faction.initialTechnologies, true);
    }
    /*
     Set the initital technologies of a faction as disabled on this landmark.
         */
    private void DisableInititalTechnologies(Faction faction) {
        SetTechnologyState(faction.initialTechnologies, false);
    }
    /*
     Enable/Disable technologies in a landmark.
         */
    public void SetTechnologyState(TECHNOLOGY technology, bool state) {
        if (!state) {
            if (!_technologiesOnLandmark.Contains(technology)) {
                //technology is not inherent to the landmark, so allow action
                _technologies[technology] = state;
            }
        } else {
            _technologies[technology] = state;
        }
    }
    /*
     Set multiple technologies states.
         */
    public void SetTechnologyState(List<TECHNOLOGY> technology, bool state) {
        for (int i = 0; i < technology.Count; i++) {
            TECHNOLOGY currTech = technology[i];
            SetTechnologyState(currTech, state);
        }
    }
    /*
     Add a technology that is inherent to the current landmark.
         */
    public void AddTechnologyOnLandmark(TECHNOLOGY technology) {
        if (!_technologiesOnLandmark.Contains(technology)) {
            _technologiesOnLandmark.Add(technology);
            SetTechnologyState(technology, true);
        }
    }
    /*
     Remove a technology that is inherent to the current landmark.
         */
    public void RemoveTechnologyOnLandmark(TECHNOLOGY technology) {
        if (_technologiesOnLandmark.Contains(technology)) {
            _technologiesOnLandmark.Remove(technology);
            if(_owner != null && _owner.initialTechnologies.Contains(technology)) {
                //Do not disable technology, since the owner of the landmark has that technology inherent to itself
            } else {
                SetTechnologyState(technology, false);
            }
        }
    }
    /*
     Does this landmark have a specific technology?
         */
    public bool HasTechnology(TECHNOLOGY technology) {
        return technologies[technology];
    }
    #endregion

    #region Population
    //   private void ConstructCiviliansDictionary() {
    //       _civiliansByRace = new Dictionary<RACE, int>();
    //       RACE[] allRaces = Utilities.GetEnumValues<RACE>();
    //       for (int i = 0; i < allRaces.Length; i++) {
    //           RACE currRace = allRaces[i];
    //           if(currRace != RACE.NONE) {
    //               _civiliansByRace.Add(currRace, 0);
    //           }
    //       }
    //   }
    //   private void GenerateCivilians() {
    //       Faction ownerOfRegion = tileLocation.region.owner;
    //       LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
    //       //int civilians = Random.Range(landmarkData.minCivilians, landmarkData.maxCivilians);
    //       RACE civiliansRace = RACE.NONE;
    //       //if (specificLandmarkType == LANDMARK_TYPE.GOBLIN_CAMP) {
    //       //    civiliansRace = RACE.GOBLIN;
    //       //} else {
    //           if (ownerOfRegion != null) {
    //               civiliansRace = ownerOfRegion.race;
    //           } else {
    //               civiliansRace = RACE.HUMANS;
    //               if (Random.Range(0, 2) == 1) {
    //                   civiliansRace = RACE.ELVES;
    //               }
    //           }
    //       //}
    //       AdjustCivilians(civiliansRace, civilians);
    //   }
    //   public void AdjustCivilians(RACE race, int amount, Character culprit = null) {
    //       _civiliansByRace[race] += amount;
    //       _civiliansByRace[race] = Mathf.Max(0, _civiliansByRace[race]);
    //	if(culprit != null){
    //		QuestManager.Instance.CreateHuntQuest (culprit);
    //	}
    //   }
    //   public void AdjustCivilians(Dictionary<RACE, int> civilians) {
    //       foreach (KeyValuePair<RACE, int> kvp in civilians) {
    //           AdjustCivilians(kvp.Key, kvp.Value);
    //       }
    //   }
    //   public Dictionary<RACE, int> ReduceCivilians(int amount) {
    //       Dictionary<RACE, int> reducedCivilians = new Dictionary<RACE, int>();
    //       for (int i = 0; i < Mathf.Abs(amount); i++) {
    //           RACE chosenRace = GetRaceBasedOnProportion();
    //           AdjustCivilians(chosenRace, -1);
    //           if (reducedCivilians.ContainsKey(chosenRace)) {
    //               reducedCivilians[chosenRace] += 1;
    //           } else {
    //               reducedCivilians.Add(chosenRace, 1);
    //           }
    //       }
    //       return reducedCivilians;
    //   }
    //public void KillAllCivilians(){
    //	RACE[] races = _civiliansByRace.Keys.ToArray ();
    //	for (int i = 0; i < races.Length; i++) {
    //		_civiliansByRace [races [i]] = 0;
    //	}
    //}
    //   protected RACE GetRaceBasedOnProportion() {
    //       WeightedDictionary<RACE> raceDict = new WeightedDictionary<RACE>(_civiliansByRace);
    //       if (raceDict.GetTotalOfWeights() > 0) {
    //           return raceDict.PickRandomElementGivenWeights();
    //       }
    //       throw new System.Exception("Cannot get race to produce!");
    //   }
    #endregion

    #region Characters
    /*
     Create a new character, given a role and class.
     This will also subtract from the civilian population.
         */
    public Character CreateNewCharacter(RACE raceOfChar, CHARACTER_ROLE charRole, string className, bool determineAction = true) {
        //RACE raceOfChar = GetRaceBasedOnProportion();
        Character newCharacter = CharacterManager.Instance.CreateNewCharacter(charRole, className, raceOfChar, _owner);
        newCharacter.SetHome(this);
        //if (reduceCivilians) {
        //    AdjustCivilians(raceOfChar, -1);
        //}
        newCharacter.CreateIcon();
        this.owner.AddNewCharacter(newCharacter);
        this.AddCharacterToLocation(newCharacter);
        this.AddCharacterHomeOnLandmark(newCharacter);
        newCharacter.icon.SetPosition(this.tileLocation.transform.position);
        if (charRole != CHARACTER_ROLE.FOLLOWER) {
            //newCharacter.CreateNewParty(); //Automatically create a new party lead by this new character.
            if (determineAction) {
                newCharacter.DetermineAction();
            }
        }
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    /*
     Create a new character, given a role and class.
     This will also subtract from the civilian population.
         */
    //public Character CreateNewCharacter(RACE raceOfChar, CHARACTER_ROLE charRole, string className, bool determineAction = true) {
    //    Character newCharacter = CharacterManager.Instance.CreateNewCharacter(charRole, className, raceOfChar);
        
    //    newCharacter.SetHome(this);
    //    //if (reduceCivilians) {
    //    //    AdjustCivilians(raceOfChar, -1);
    //    //}
    //    if (owner != null) {
    //        newCharacter.SetFaction(owner);
    //        owner.AddNewCharacter(newCharacter);
    //    }
    //    newCharacter.CreateIcon();
    //    AddCharacterToLocation(newCharacter);
    //    AddCharacterHomeOnLandmark(newCharacter);
    //    newCharacter.icon.SetPosition(this.tileLocation.transform.position);
    //    if (determineAction) {
    //        newCharacter.DetermineAction();
    //    }
    //    Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
    //    return newCharacter;
    //}

    /*
     Create a new character, given a character setup name.
         */
    public Character CreateNewCharacter(RACE raceOfChar, string setupName, bool reduceCivilians = true, bool determineAction = true) {
        Character newCharacter = CharacterManager.Instance.CreateNewCharacter(setupName, _owner);
        //newCharacter.AssignRole(charRole);
        //newCharacter.SetFaction(_owner);
        newCharacter.SetHome(this);
        //if (reduceCivilians) {
        //    AdjustCivilians(raceOfChar, -1);
        //}
        if (_owner != null) {
            _owner.AddNewCharacter(newCharacter);
        }
        newCharacter.CreateIcon();
        this.AddCharacterToLocation(newCharacter);
        this.AddCharacterHomeOnLandmark(newCharacter);
        newCharacter.icon.SetPosition(this.tileLocation.transform.position);
        if (newCharacter.role.roleType != CHARACTER_ROLE.FOLLOWER) {
            //newCharacter.CreateNewParty(); //Automatically create a new party lead by this new character.
            if (determineAction) {
                newCharacter.DetermineAction();
            }
        }
        //UIManager.Instance.UpdateFactionSummary();
        return newCharacter;
    }
    /*
     Make a character consider this landmark as it's home.
         */
    public virtual void AddCharacterHomeOnLandmark(Character character) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
        }
    }
    public void RemoveCharacterHomeOnLandmark(Character character) {
        _charactersWithHomeOnLandmark.Remove(character);
    }
	public Character GetCharacterAtLocationByID(int id, bool includeTraces = false){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
            if (_charactersAtLocation[i].id == id) {
                return _charactersAtLocation[i];
            }
            //if (_charactersAtLocation[i]	is Character){
			//	if(((Character)_charactersAtLocation[i]).id == id){
			//		return (Character)_charactersAtLocation [i];
			//	}
			//}else if(_charactersAtLocation[i] is Party){
			//	Party party = (Party)_charactersAtLocation [i];
			//	for (int j = 0; j < party.partyMembers.Count; j++) {
			//		if(party.partyMembers[j].id == id){
			//			return party.partyMembers [j];
			//		}
			//	}
			//}
		}
		if(includeTraces){
			foreach (Character character in _characterTraces.Keys) {
				if(character.id == id){
					return character;
				}	
			}
		}
		return null;
	}
	//public Party GetPartyAtLocationByLeaderID(int id){
	//	for (int i = 0; i < _charactersAtLocation.Count; i++) {
	//		if(_charactersAtLocation[i]	is Party){
	//			if(((Party)_charactersAtLocation[i]).partyLeader.id == id){
	//				return (Party)_charactersAtLocation [i];
	//			}
	//		}
	//	}
	//	return null;
	//}
	public Character GetPrisonerByID(int id){
		for (int i = 0; i < _prisoners.Count; i++) {
			if (_prisoners [i].id == id){
				return _prisoners [i];
			}
		}
		return null;
	}
    /*
     Does the landmark have the required technology
     to produce a class?
         */
    public bool CanProduceClass(CHARACTER_CLASS charClass) {
        //if (_owner == null) {
        //    return false;
        //}
        TECHNOLOGY neededTech = Utilities.GetTechnologyForCharacterClass(charClass);
        if (neededTech == TECHNOLOGY.NONE) {
            return true;
        } else {
            return _technologies[neededTech];
        }
    }
    #endregion

   // #region Party
   // public List<Party> GetPartiesOnLandmark() {
   //     List<Party> parties = new List<Party>();
   //     for (int i = 0; i < _location.charactersAtLocation.Count; i++) {
			//if(_location.charactersAtLocation[i] is Party){
			//	parties.Add((Party)_location.charactersAtLocation[i]);
			//}
   //     }
   //     return parties;
   // }
   // #endregion

    #region Location
    public void AddCharacterToLocation(Character character) {
        if (!_charactersAtLocation.Contains(character)) {
            _charactersAtLocation.Add(character);
            character.SetSpecificLocation(this);
    //        if (character is Character) {
    //            Character currChar = character as Character;
				//this.tileLocation.RemoveCharacterFromLocation(currChar);
    //            currChar.SetSpecificLocation(this);
    //        } else if (character is Party) {
    //            Party currParty = character as Party;
				//this.tileLocation.RemoveCharacterFromLocation(currParty);
    //            currParty.SetSpecificLocation(this);
    //        }
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        _charactersAtLocation.Remove(character);
        character.SetSpecificLocation(null);
   //     if (character is Character) {
   //         Character currChar = character as Character;
			//currChar.SetSpecificLocation(null);
   //     } else if (character is Party) {
   //         Party currParty = character as Party;
			//currParty.SetSpecificLocation(null);
   //     }
        if (_charactersAtLocation.Count == 0 && _hasScheduledCombatCheck) {
            UnScheduleCombatCheck();
        }
    }
    public void ReplaceCharacterAtLocation(Character characterToReplace, Character characterToAdd) {
        if (_charactersAtLocation.Contains(characterToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(characterToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, characterToAdd);
            _charactersAtLocation.Remove(characterToReplace);
            characterToAdd.SetSpecificLocation(this);
    //        if (characterToAdd is Character) {
    //            Character currChar = characterToAdd as Character;
				//this.tileLocation.RemoveCharacterFromLocation(currChar);
    //            currChar.SetSpecificLocation(this);
    //        } else if (characterToAdd is Party) {
    //            Party currParty = characterToAdd as Party;
				//this.tileLocation.RemoveCharacterFromLocation(currParty);
    //            currParty.SetSpecificLocation(this);
    //        }
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
    }
   // public int CharactersCount(bool includeHostile = false) {
   //     int count = 0;
   //     for (int i = 0; i < _charactersAtLocation.Count; i++) {
			//if (includeHostile && this._owner != null) {
			//	if(_charactersAtLocation[i].faction == null){
			//		continue;
			//	}else{
			//		FactionRelationship fr = this._owner.GetRelationshipWith (_charactersAtLocation [i].faction);
			//		if(fr != null && fr.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
			//			continue;
			//		}
			//	}
			//}
   //         if (_charactersAtLocation[i] is Party) {
   //             count += ((Party)_charactersAtLocation[i]).partyMembers.Count;
   //         } else {
   //             count += 1;
   //         }
   //     }
   //     return count;
   // }
    #endregion

    #region Combat
    public void ScheduleCombatCheck() {
        _hasScheduledCombatCheck = true;
        Messenger.AddListener(Signals.HOUR_STARTED, CheckForCombat);
    }
    public void UnScheduleCombatCheck() {
        _hasScheduledCombatCheck = false;
        Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForCombat);
    }
    /*
     Check this location for encounters, start if any.
     Mechanics can be found at https://trello.com/c/PgK25YvC/837-encounter-mechanics.
         */
    public void CheckForCombat() {
        //At the start of each day:
        if (HasHostilities() && HasCombatInitializers()) {
            ////1. Attacking characters will attempt to initiate combat:
            //CheckAttackingGroupsCombat();
            ////2. Patrolling characters will attempt to initiate combat:
            //CheckPatrollingGroupsCombat();
            PairUpCombats();
        }
        //3. Pillaging and Hunting characters will perform their daily action if they havent been engaged in combat
        //4. Exploring and Stealing characters will perform their daily action if they havent been engaged in combat
        //5. Resting and Hibernating characters will recover HP if they havent been engaged in combat
        ContinueDailyActions();
    }
    public void PairUpCombats() {
        List<Character> combatInitializers = GetCharactersByCombatPriority();
        if (combatInitializers != null) {
            for (int i = 0; i < combatInitializers.Count; i++) {
                Character currInitializer = combatInitializers[i];
                Debug.Log("Finding combat pair for " + currInitializer.name);
                if (currInitializer.isInCombat) {
                    continue; //this current group is already in combat, skip it
                }
                //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<Character> combatGroups = new List<Character>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (combatGroups.Count > 0) {
                    Character chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<Character> neutralGroups = new List<Character>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (neutralGroups.Count > 0) {
                    Character chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
                List<Character> stealthGroups = new List<Character>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (stealthGroups.Count > 0) {
                    //The chance of initiating combat is 35%
                    if (Random.Range(0, 100) < 35) {
                        Character chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                        StartCombatBetween(currInitializer, chosenEnemy);
                        continue; //the attacking group has found an enemy! skip to the next group
                    }
                }
            }
        }
    }
    public List<Character> GetCharactersByCombatPriority() {
        //if (_charactersAtLocation.Count <= 0) {
        //    return null;
        //}
        //return _charactersAtLocation.Where(x => x.currentAction.combatPriority > 0).OrderByDescending(x => x.currentAction.combatPriority).ToList();
        return null;
    }
    public bool HasCombatInitializers() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currChar = _charactersAtLocation[i];
            //if (currChar.currentAction != null && currChar.currentAction.combatPriority > 0) {
            //    return true;
            //}
        }
        return false;
    }
    public bool HasHostilities() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currItem = _charactersAtLocation[i];
            for (int j = 0; j < _charactersAtLocation.Count; j++) {
                Character otherItem = _charactersAtLocation[j];
                if (currItem != otherItem) {
                    if (currItem.IsHostileWith(otherItem)) {
                        return true; //there are characters with hostilities
                    }
                }
            }
        }
        return false;
    }
    public bool HasHostileCharactersWith(Character character) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currItem = _charactersAtLocation[i];
            if (currItem == character) {
                continue; //skip
            }
            Faction factionOfItem = currItem.faction;
            //if (currItem is Character) {
            //    factionOfItem = (currItem as Character).faction;
            //} else if (currItem is Party) {
            //    factionOfItem = (currItem as Party).faction;
            //}
            if (factionOfItem == null || character.faction == null) {
                return true;
            } else {
                if (factionOfItem.id == character.faction.id) {
                    continue; //skip this item, since it has the same faction as the other faction
                }
                FactionRelationship rel = character.faction.GetRelationshipWith(factionOfItem);
                if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false) {
        if (faction == null) {
            if(this.owner != null) {
                return true; //the passed faction is null (factionless), if this landmark is owned, the factionless are considered as hostile
            }
        } else {
            //the passed faction is not null, check if this landmark is owned
            if(this.owner != null) {
                //if this is owned, check if the 2 factions are not the same
                if(faction.id != this.owner.id) {
                    //if they are not the same, check if the relationship of the factions are hostile
                    FactionRelationship rel = faction.GetRelationshipWith(this.owner);
                    if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                        return true; //the passed faction is hostile with the owner of this landmark
                    }
                }
            }
        }
        if (!withFactionOnly) {
            for (int i = 0; i < _charactersAtLocation.Count; i++) {
                Character currItem = _charactersAtLocation[i];
                Faction factionOfItem = currItem.faction;
                //if (currItem is Character) {
                //    factionOfItem = (currItem as Character).faction;
                //} else if (currItem is Party) {
                //    factionOfItem = (currItem as Party).faction;
                //}
                if (factionOfItem == null || faction == null) {
                    return true;
                } else {
                    if (factionOfItem.id == faction.id) {
                        continue; //skip this item, since it has the same faction as the other faction
                    }
                    FactionRelationship rel = faction.GetRelationshipWith(factionOfItem);
                    if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public List<Character> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, Character except = null) {
        List<Character> groups = new List<Character>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currGroup = _charactersAtLocation[i];
            if (notInCombatOnly) {
                if (currGroup.isInCombat) {
                    continue; //skip
                }
            }
            if (currGroup.GetCurrentStance() == stance) {
                if (except != null && currGroup == except) {
                    continue; //skip
                }
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public void StartCombatBetween(Character combatant1, Character combatant2) {
        Combat combat = new Combat(this);
        combatant1.SetIsInCombat(true);
        combatant2.SetIsInCombat(true);
        string combatant1Name = combatant1.name;
        string combatant2Name = combatant2.name;
        combat.AddCharacter(SIDES.A, combatant1);
        combat.AddCharacter(SIDES.B, combatant2);

        //if (combatant1 is Party) {
        //    combatant1Name = (combatant1 as Party).name;
        //    combat.AddCharacters(SIDES.A, (combatant1 as Party).partyMembers);
        //} else {
        //    combatant1Name = (combatant1 as Character).name;
        //    combat.AddCharacter(SIDES.A, combatant1 as Character);
        //}
        //if (combatant2 is Party) {
        //    combatant2Name = (combatant2 as Party).name;
        //    combat.AddCharacters(SIDES.B, (combatant2 as Party).partyMembers);
        //} else {
        //    combatant2Name = (combatant2 as Character).name;
        //    combat.AddCharacter(SIDES.B, combatant2 as Character);
        //}
        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(combatant1, combatant1Name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
        combatLog.AddToFillers(combatant2, combatant2Name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddHistory(combatLog);
        combatant1.AddHistory(combatLog);
        combatant2.AddHistory(combatLog);
        Debug.Log("Starting combat between " + combatant1Name + " and  " + combatant2Name);

        //this.specificLocation.SetCurrentCombat(combat);
        MultiThreadPool.Instance.AddToThreadPool(combat);
    }
    public void ContinueDailyActions() {
        //for (int i = 0; i < _charactersAtLocation.Count; i++) {
        //    Character currItem = _charactersAtLocation[i];
        //    if (!currItem.isInCombat) {
        //        currItem.ContinueDailyAction();
        //    }
        //}
    }
    #endregion

    #region Utilities
    public void SetLandmarkObject(LandmarkObject obj) {
        _landmarkObject = obj;
        _landmarkObject.SetLandmark(this);
    }
    internal int GetTechnologyCount() {
        int count = 0;
        foreach (bool isTrue in _technologies.Values) {
            if (isTrue) {
                count += 1;
            }
        }
        return count;
    }
    internal int GetMinimumCivilianRequirement() {
        if (this is ResourceLandmark) {
            return 5;
        }else if(this is Settlement) {
            return 20;
        }
        return 0;
    }
	internal void ChangeLandmarkType(LANDMARK_TYPE newLandmarkType){
		_specificLandmarkType = newLandmarkType;
		Initialize ();
	}
    public void CenterOnLandmark() {
		CameraMove.Instance.CenterCameraOn(this.tileLocation.gameObject);
    }
    public override string ToString() {
        return this.landmarkName;
    }
    public void SetIsAttackingAnotherLandmarkState(bool state) {
        _isAttackingAnotherLandmark = state;
    }
    #endregion

    #region Prisoner
    internal void AddPrisoner(Character character){
		character.SetPrisoner (true, this);
		_prisoners.Add (character);
	}
	internal void RemovePrisoner(Character character){
		_prisoners.Remove (character);
		character.SetPrisoner (false, null);
	}
	#endregion

	#region History
    internal void AddHistory(Log log) {
        ////check if the new log is a duplicate of the latest log
        //Log latestLog = history.ElementAtOrDefault(history.Count - 1);
        //if (latestLog != null) {
        //    if (Utilities.AreLogsTheSame(log, latestLog)) {
        //        string text = landmarkName + " has duplicate logs!";
        //        text += "\n" + log.id + Utilities.LogReplacer(log) + " ST:" + log.logCallStack;
        //        text += "\n" + latestLog.id + Utilities.LogReplacer(latestLog) + " ST:" + latestLog.logCallStack;
        //        throw new System.Exception(text);
        //    }
        //}

        _history.Add(log);
        if (this._history.Count > 20) {
            this._history.RemoveAt(0);
        }
        Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
    }
    #endregion

    #region Materials
    public void AdjustDurability(int amount){
        _landmarkObj.AdjustHP(amount);
		//_currDurability += amount;
		//_currDurability = Mathf.Clamp (_currDurability, 0, _totalDurability);
	}
    #endregion

    #region Items
    private void SpawnInitialLandmarkItems() {
        //LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_specificLandmarkType);
        //for (int i = 0; i < data.itemData.Length; i++) {
        //    LandmarkItemData currItemData = data.itemData[i];
        //    Item createdItem = ItemManager.Instance.CreateNewItemInstance(currItemData.itemName);
        //    if (ItemManager.Instance.IsLootChest(createdItem)) {
        //        //chosen item is a loot crate, generate a random item
        //        string[] words = createdItem.itemName.Split(' ');
        //        int tier = System.Int32.Parse(words[1]);
        //        if (createdItem.itemName.Contains("Armor")) {
        //            createdItem = ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.ARMOR);
        //        } else if (createdItem.itemName.Contains("Weapon")) {
        //            createdItem = ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.WEAPON);
        //        }
        //    } else {
        //        //only set as unlimited if not from loot chest, since gear from loot chests are not unlimited
        //        createdItem.SetIsUnlimited(currItemData.isUnlimited);
        //    }
        //    //createdItem.SetExploreWeight(currItemData.exploreWeight);
        //    AddItemInLandmark(createdItem);
        //}
    }
    private QUALITY GetEquipmentQuality() {
        int crudeChance = 30;
        int exceptionalChance = crudeChance + 20;
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < crudeChance) {
            return QUALITY.CRUDE;
        } else if (chance >= crudeChance && chance < exceptionalChance) {
            return QUALITY.EXCEPTIONAL;
        }
        return QUALITY.NORMAL;
    }
    public void AddItemInLandmark(Item item){
        if (_itemsInLandmark.Contains(item)) {
            throw new System.Exception(this.landmarkName + " already has an instance of " + item.itemName);
        }
		_itemsInLandmark.Add (item);
		item.SetPossessor (this);
        item.OnItemPlacedOnLandmark(this);
	}
	public void AddItemsInLandmark(List<Item> item){
		_itemsInLandmark.AddRange (item);
	}
	public void RemoveItemInLandmark(Item item){
		if(!item.isUnlimited){
			_itemsInLandmark.Remove (item);
			item.SetPossessor (null);
		}
	}
    public void RemoveItemInLandmark(string itemName) {
        for (int i = 0; i < itemsInLandmark.Count; i++) {
            ECS.Item currItem = itemsInLandmark[i];
            if (currItem.itemName.Equals(itemName)) {
                RemoveItemInLandmark(currItem);
                break;
            }
        }
    }
    //private WeightedDictionary<Item> GetExploreItemWeights() {
    //    WeightedDictionary<Item> itemWeights = new WeightedDictionary<Item>();
    //    for (int i = 0; i < _itemsInLandmark.Count; i++) {
    //        Item currItem = _itemsInLandmark[i];
    //        itemWeights.AddElement(currItem, currItem.exploreWeight);
    //    }
    //    return itemWeights;
    //}

	//public void SpawnItemInLandmark(string itemName, int exploreWeight, bool isUnlimited){
	//	Item item = ItemManager.Instance.CreateNewItemInstance (itemName);
	//	item.exploreWeight = exploreWeight;
	//	item.isUnlimited = isUnlimited;
	//	AddItemInLandmark (item);
	//}

	//public void SpawnItemInLandmark(Item item, int exploreWeight, bool isUnlimited){
	//	Item newItem = item.CreateNewCopy();
    //  newItem.exploreWeight = exploreWeight;
	//	newItem.isUnlimited = isUnlimited;
	//	AddItemInLandmark (newItem);
	//}
	public Item SpawnItemInLandmark(string itemName){
		Item item = ItemManager.Instance.CreateNewItemInstance (itemName);
		AddItemInLandmark (item);
		return item;
	}
	public Item SpawnItemInLandmark(Item item){
		Item newItem = item.CreateNewCopy();
		AddItemInLandmark (newItem);
		return item;
	}
	public bool HasItem(string itemName){
		for (int i = 0; i < _itemsInLandmark.Count; i++) {
			if (_itemsInLandmark [i].itemName == itemName) {
				return true;
			}
		}
		return false;
	}
    #endregion

	#region Traces
	public void AddTrace(Character character){
		GameDate expDate = GameManager.Instance.Today ();
		expDate.AddDays (90);
		if(!_characterTraces.ContainsKey(character)){
			_characterTraces.Add (character, expDate);
		}else{
			SchedulingManager.Instance.RemoveSpecificEntry (_characterTraces[character], () => RemoveTrace (character));
			_characterTraces [character] = expDate;
		}
		SchedulingManager.Instance.AddEntry (expDate, () => RemoveTrace (character));
	}
	public void RemoveTrace(Character character){
		if(_characterTraces.ContainsKey(character)){
			if(GameManager.Instance.Today().IsSameDate(_characterTraces[character])){
				_characterTraces.Remove (character);
			}
		}
	}
    #endregion

    #region Tags
    private void ConstructTags(LandmarkData landmarkData) {
        _landmarkTags = new List<LANDMARK_TAG>(landmarkData.uniqueTags); //add unique tags
        //add common tags from base landmark type
        BaseLandmarkData baseLandmarkData = LandmarkManager.Instance.GetBaseLandmarkData(landmarkData.baseLandmarkType);
        _landmarkTags.AddRange(baseLandmarkData.baseLandmarkTags);
    }
    #endregion

    #region Objects
    public void SetObject(StructureObj obj) {
        _landmarkObj = obj;
        obj.OnAddToLandmark(this);
    }
    //public bool AddObject(IObject obj) {
    //    if (!_objects.Contains(obj)) {
    //        _objects.Add(obj);
    //        obj.OnAddToLandmark(this);
    //        return true;
    //    }
    //    return false;
    //}
    //public void RemoveObject(IObject obj) {
    //    _objects.Remove(obj);
    //    obj.SetObjectLocation(null);
    //}
    //public IObject GetObject(OBJECT_TYPE objectType, string name) {
    //    for (int i = 0; i < _objects.Count; i++) {
    //        if(_objects[i].objectType == objectType && _objects[i].objectName == name) {
    //            return _objects[i];
    //        }
    //    }
    //    return null;
    //}
    //public IObject GetObject(string name) {
    //    for (int i = 0; i < _objects.Count; i++) {
    //        if (_objects[i].objectName == name) {
    //            return _objects[i];
    //        }
    //    }
    //    return null;
    //}
    #endregion

    #region Resource Inventory
    //public void ConstructResourceInventory() {
    //    _resourceInventory = new Dictionary<RESOURCE, int>();
    //    RESOURCE[] allResources = Utilities.GetEnumValues<RESOURCE>();
    //    for (int i = 0; i < allResources.Length; i++) {
    //        if (allResources[i] != RESOURCE.NONE) {
    //            _resourceInventory.Add(allResources[i], 0);
    //        }
    //    }
    //}
    //public void AdjustResource(RESOURCE resource, int amount) {
    //    _resourceInventory[resource] += amount;
    //}
    //public void TransferResourceTo(RESOURCE resource, int amount, BaseLandmark target) {
    //    AdjustResource(resource, -amount);
    //    target.AdjustResource(resource, amount);
    //}
    //public void TransferResourceTo(RESOURCE resource, int amount, CharacterObj target) {
    //    AdjustResource(resource, -amount);
    //    target.AdjustResource(resource, amount);
    //}
    #endregion

    #region Corruption
    public void ToggleCorruption(bool state) {
        if (state) {
            LandmarkManager.Instance.corruptedLandmarksCount++;
            if (!_hasBeenCorrupted) {
                _hasBeenCorrupted = true;
                _nextCorruptedTilesToCheck.Add(tileLocation);
            }
            //_diagonalLeftBlocked = 0;
            //_diagonalRightBlocked = 0;
            //_horizontalBlocked = 0;
            //tileLocation.region.LandmarkStartedCorruption(this);
            PutWallDown();
            Messenger.AddListener(Signals.HOUR_ENDED, DoCorruption);
            //if (Messenger.eventTable.ContainsKey("StartCorruption")) {
            //    Messenger.RemoveListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);
            //    Messenger.Broadcast<BaseLandmark>("StartCorruption", this);
            //}
        } else {
            LandmarkManager.Instance.corruptedLandmarksCount--;
            StopSpreadCorruption();
        }
    }
    private void StopSpreadCorruption() {
        if (!hasAdjacentCorruptedLandmark && LandmarkManager.Instance.corruptedLandmarksCount > 1) {
            HexTile chosenTile = null;
            int range = 3;
            while(chosenTile == null) {
                List<HexTile> tilesToCheck = tileLocation.GetTilesInRange(range, true);
                for (int i = 0; i < tilesToCheck.Count; i++) {
                    if (tilesToCheck[i].corruptedLandmark != null && tilesToCheck[i].corruptedLandmark.id != this.id) {
                        chosenTile = tilesToCheck[i];
                        break;
                    }
                }
                range++;
            }
            PathGenerator.Instance.CreatePath(this, this.tileLocation, chosenTile, PATHFINDING_MODE.UNRESTRICTED);
        }
        //tileLocation.region.LandmarkStoppedCorruption(this);
        Messenger.RemoveListener(Signals.HOUR_ENDED, DoCorruption);
        //Messenger.Broadcast<BaseLandmark>("StopCorruption", this);
    }
    private void DoCorruption() {
        if(_nextCorruptedTilesToCheck.Count > 0) {
            int index = UnityEngine.Random.Range(0, _nextCorruptedTilesToCheck.Count);
            HexTile currentCorruptedTileToCheck = _nextCorruptedTilesToCheck[index];
            _nextCorruptedTilesToCheck.RemoveAt(index);
            SpreadCorruption(currentCorruptedTileToCheck);
        } else {
            StopSpreadCorruption();
        }
    }
    private void SpreadCorruption(HexTile originTile) {
        if (!originTile.CanThisTileBeCorrupted()) {
            return;
        }
        for (int i = 0; i < originTile.AllNeighbours.Count; i++) {
            HexTile neighbor = originTile.AllNeighbours[i];
            if (neighbor.uncorruptibleLandmarkNeighbors <= 0) {
                if (!neighbor.isCorrupted) { //neighbor.region.id == originTile.region.id && neighbor.CanThisTileBeCorrupted()
                    neighbor.SetCorruption(true, this);
                    _nextCorruptedTilesToCheck.Add(neighbor);
                }
                //if(neighbor.landmarkNeighbor != null && !neighbor.landmarkNeighbor.tileLocation.isCorrupted) {
                //    neighbor.landmarkNeighbor.CreateWall();
                //}
                if (originTile.corruptedLandmark.id != neighbor.corruptedLandmark.id) {
                    originTile.corruptedLandmark.hasAdjacentCorruptedLandmark = true;
                }
            }
            //else {
                //if cannot be corrupted it means that it has a landmark still owned by a kingdom
                //neighbor.landmarkOnTile.CreateWall();
            //}
        }
    }
    //public void AdjustDiagonalLeftBlocked(int amount) {
    //    _diagonalLeftBlocked += amount;
    //    if(_diagonalLeftBlocked < 0) {
    //        _diagonalLeftBlocked = 0;
    //    }
    //}
    //public void AdjustDiagonalRightBlocked(int amount) {
    //    _diagonalRightBlocked += amount;
    //    if (_diagonalRightBlocked < 0) {
    //        _diagonalRightBlocked = 0;
    //    }
    //}
    //public void AdjustHorizontalBlocked(int amount) {
    //    _horizontalBlocked += amount;
    //    if (_horizontalBlocked < 0) {
    //        _horizontalBlocked = 0;
    //    }
    //}
    //public void AdjustDirectionBlocked(string direction, int amount) {
    //    if(direction == "diagonalleft") {
    //        AdjustDiagonalLeftBlocked(amount);
    //    }else if (direction == "diagonalright") {
    //        AdjustDiagonalRightBlocked(amount);
    //    }else if (direction == "horizontal") {
    //        AdjustHorizontalBlocked(amount);
    //    }
    //}
    public void ALandmarkHasStartedCorruption(BaseLandmark corruptedLandmark) {
        //Messenger.RemoveListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);

        //int corruptedX = corruptedLandmark.tileLocation.xCoordinate;
        //int corruptedY = corruptedLandmark.tileLocation.yCoordinate;

        //string direction = "horizontal";
        ////if same column, the wall is automatically horizontal, if not, enter here
        //if (tileLocation.xCoordinate != corruptedX) {
        //    if (tileLocation.yCoordinate == corruptedY) {
        //        int chance = UnityEngine.Random.Range(0, 2);
        //        if (chance == 0) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    } else if (tileLocation.yCoordinate < corruptedY) {
        //        if (tileLocation.xCoordinate < corruptedX) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    } else {
        //        if (tileLocation.xCoordinate < corruptedX) {
        //            direction = "diagonalright";
        //        } else {
        //            direction = "diagonalleft";
        //        }
        //    }
        //}
        //int chance = UnityEngine.Random.Range(0, 3);
        //if (chance == 0) {
        //    direction = "diagonalleft";
        //} else {
        //    direction = "diagonalright";
        //}
        PutWallUp();
        // if (tileLocation.xCoordinate != corruptedX) {
        //    if (tileLocation.xCoordinate < corruptedX) {
        //        if(tileLocation.yCoordinate == corruptedY) {
        //            if(_diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //                direction = "diagonalright";
        //            }else if (_diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //                direction = "diagonalleft";
        //            } else {
        //                if (chance == 0) {
        //                    direction = "diagonalleft";
        //                } else {
        //                    direction = "diagonalright";
        //                }
        //            }
        //        } else {
        //            if(tileLocation.yCoordinate < corruptedY) {
        //                if (_diagonalLeftBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalleft";
        //                } else if (_diagonalLeftBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalleft";
        //                    }
        //                }
        //            } else {
        //                if (_diagonalRightBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalright";
        //                } else if (_diagonalRightBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalright";
        //                    }
        //                }
        //            }
        //        }
        //    } else {
        //        if (tileLocation.yCoordinate == corruptedY) {
        //            if (_diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //                direction = "diagonalright";
        //            } else if (_diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //                direction = "diagonalleft";
        //            } else {
        //                if (chance == 0) {
        //                    direction = "diagonalleft";
        //                } else {
        //                    direction = "diagonalright";
        //                }
        //            }
        //        } else {
        //            if (tileLocation.yCoordinate < corruptedY) {
        //                if (_diagonalRightBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalright";
        //                } else if (_diagonalRightBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalright";
        //                    }
        //                }
        //            } else {
        //                if (_diagonalLeftBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalleft";
        //                } else if (_diagonalLeftBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalleft";
        //                    }
        //                }
        //            }
        //        }
        //    }
        //} else {
        //    if (_horizontalBlocked > 0 && _diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //        direction = "diagonalright";
        //    } else if (_horizontalBlocked > 0 && _diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //        direction = "diagonalleft";
        //    } else if (_horizontalBlocked <= 0 && _diagonalLeftBlocked > 0 && _diagonalRightBlocked > 0) {
        //        direction = "horizontal";
        //    } else {
        //        if (chance == 0) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    }
        //}
        //AdjustDirectionBlocked(direction, 1);
        //_blockedLandmarkDirection.Add(corruptedLandmark, direction);
    }
    //public void ALandmarkHasStoppedCorruption(BaseLandmark corruptedLandmark) {
    //    string direction = _blockedLandmarkDirection[corruptedLandmark];
    //    AdjustDirectionBlocked(direction, -1);
    //    _blockedLandmarkDirection.Remove(corruptedLandmark);
    //}

    public void PutWallUp() {
        //_wallDirection = direction;
        //List<HexTile> wallTiles = _horizontalTiles;
        //if(direction == "diagonalleft") {
        //    wallTiles = _diagonalLeftTiles;
        //} else if (direction == "diagonalright") {
        //    wallTiles = _diagonalRightTiles;
        //}
        //for (int i = 0; i < wallTiles.Count; i++) {
        //    wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(1);
        //}
        for (int i = 0; i < _wallTiles.Count; i++) {
            _wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(1);
        }
    }
    private void PutWallDown() {
        for (int i = 0; i < _wallTiles.Count; i++) {
            _wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        }
        //for (int i = 0; i < tileLocation.AllNeighbours.Count; i++) {
        //    tileLocation.AllNeighbours[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        //}
        //if(_wallDirection != string.Empty) {
        //    List<HexTile> wallTiles = _horizontalTiles;
        //    if (_wallDirection == "diagonalleft") {
        //        wallTiles = _diagonalLeftTiles;
        //    } else if (_wallDirection == "diagonalright") {
        //        wallTiles = _diagonalRightTiles;
        //    }
        //    for (int i = 0; i < wallTiles.Count; i++) {
        //        wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        //    }
        //    _wallDirection = string.Empty;
        //}
    }
    public void ReceivePath(List<HexTile> pathTiles) {
        if(pathTiles != null) {
            ConnectCorruption(pathTiles);
        }
    }
    private void ConnectCorruption(List<HexTile> pathTiles) {
        for (int i = 0; i < pathTiles.Count; i++) {
            pathTiles[i].SetUncorruptibleLandmarkNeighbors(0);
            pathTiles[i].SetCorruption(true, this);
        }
    }
    #endregion

    #region Hextiles
    public void GenerateDiagonalLeftTiles() {
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.NORTH_WEST, tileLocation);
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.SOUTH_EAST, tileLocation);
    }
    public void GenerateDiagonalRightTiles() {
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.NORTH_EAST, tileLocation);
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.SOUTH_WEST, tileLocation);
    }
    public void GenerateHorizontalTiles() {
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.EAST, tileLocation);
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.WEST, tileLocation);
    }
    public void GenerateWallTiles() {
        //_wallTiles.AddRange(_diagonalLeftTiles);
        //_wallTiles.AddRange(_diagonalRightTiles);
        //_wallTiles.AddRange(_horizontalTiles);
        //_wallTiles.AddRange(tileLocation.AllNeighbours);
        _wallTiles = tileLocation.GetTilesInRange(2);
    }
    private void AddTileRecursivelyByDirection(HEXTILE_DIRECTION direction, HexTile originTile) {
        if (originTile.tileLocation.neighbourDirections.ContainsKey(direction)) {
            if (!originTile.tileLocation.neighbourDirections[direction].neighbourDirections.ContainsKey(direction)) {
                return;
            }
            HexTile directionTile = originTile.tileLocation.neighbourDirections[direction].neighbourDirections[direction];
            if(directionTile.landmarkOnTile == null) { //directionTile.region.id == originTile.region.id
                //string strDirection = "diagonalleft";
                //if (direction == HEXTILE_DIRECTION.NORTH_WEST || direction == HEXTILE_DIRECTION.SOUTH_EAST) {
                //    _diagonalLeftTiles.Add(directionTile);
                //} else if (direction == HEXTILE_DIRECTION.NORTH_EAST || direction == HEXTILE_DIRECTION.SOUTH_WEST) {
                //    _diagonalRightTiles.Add(directionTile);
                //    //strDirection = "diagonalright";
                //} else if (direction == HEXTILE_DIRECTION.EAST || direction == HEXTILE_DIRECTION.WEST) {
                //    _horizontalTiles.Add(directionTile);
                //    //strDirection = "horizontal";
                //}
                //directionTile.landmarkDirection.Add(this, strDirection);
                //AddTileRecursivelyByDirection(direction, directionTile);
            }
        }
    }
    //public bool IsDirectionBlocked(string direction) {
    //    if (!tileLocation.isCorrupted) {
    //        if(direction == "diagonalleft") {
    //            return _diagonalLeftBlocked > 0;
    //        }else if (direction == "diagonalright") {
    //            return _diagonalRightBlocked > 0;
    //        } else if (direction == "horizontal") {
    //            return _horizontalBlocked > 0;
    //        }
    //    }
    //    return false;
    //}
    #endregion
}
