﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace ECS {
	[System.Serializable]
	public class Character : TaskCreator, ICombatInitializer {
        public delegate void OnCharacterDeath();
        public OnCharacterDeath onCharacterDeath;

        public delegate void OnTaskChanged();
        private OnTaskChanged onTaskChanged; //What should happen if a character chooses to change it's task

        [SerializeField] private string _name;
        private int _id;
		private GENDER _gender;
        [System.NonSerialized] private CharacterType _characterType; //Base Character Type(For Traits)
        [System.NonSerialized] private List<Trait>	_traits;
        private List<TRAIT> _allTraits;
		private List<CharacterTag>	_tags;
		internal List<STATUS_EFFECT> statusEffects;
        private Dictionary<Character, Relationship> _relationships;
		private const int MAX_FOLLOWERS = 4;

		//Stats
		[SerializeField] private int _currentHP;
		[SerializeField] private int _maxHP;
		[SerializeField] private int _strength;
		[SerializeField] private int _intelligence;
		[SerializeField] private int _agility;
		[SerializeField] private int _currentRow;
		private SIDES _currentSide;
		private int _baseMaxHP;
		private int _baseStrength;
		private int _baseIntelligence;
		private int _baseAgility;
		private int _fixedMaxHP;
		private StatsModifierPercentage _statsModifierPercentage;

		//Skills
		private List<Skill> _skills;

		private CharacterClass _characterClass;
		private RaceSetting _raceSetting;
		private CharacterRole _role;
		private Faction _faction;
		private Party _party;
        private QuestData _questData;
		private CharacterTask _currentTask;
        private ILocation _specificLocation;
		private Region _currentRegion;
		private CharacterAvatar _avatar;

		[SerializeField] private List<BodyPart> _bodyParts;
		[SerializeField] private List<Item> _equippedItems;
		[SerializeField] private List<Item> _inventory;

		private Color _characterColor;
		private string _characterColorCode;
		private bool _isDead;
		private bool _isFainted;
		private bool _isPrisoner;
        private bool _isFollower;
		private bool _isDefeated;
		private object _isPrisonerOf;
		private List<OldQuest.Quest> _activeQuests;
		private BaseLandmark _home;
        private BaseLandmark _lair;
		private List<Log> _history;
		private int _combatHistoryID;
		private List<Character> _prisoners;
		private List<Character> _followers;
		private Character _isFollowerOf;
		private bool _doesNotTakePrisoners;
		private bool _cannotBeTakenAsPrisoner;

        private Dictionary<RACE, int> _civiliansByRace;

		internal int actRate;
		internal CombatPrototype currentCombat;
		internal Dictionary<int, CombatPrototype> combatHistory;

		private float _equippedWeaponPower;
        private int _gold;
        private int _prestige;

		private Action _currentFunction;
		private bool _isInCombat;

        //When the character should have a next action it should do after it's current one.
        private CharacterTask nextTaskToDo;

//        private Dictionary<MATERIAL, int> _materialInventory;
		private List<BaseLandmark> _exploredLandmarks; //Currently only storing explored landmarks that were explored for the last 6 months

		private Dictionary<Character, List<string>> _traceInfo;

        public Dictionary<CharacterTask, string> previousActions; //For testing, list of all the characters previous actions. TODO: Remove this after testing

		#region getters / setters
		public string firstName {
            get { return name.Split(' ')[0]; }
        }
		public string name{
			get { return this._name; }
		}
		public string coloredName{
			get { return "[" + this._characterColorCode + "]" + this._name + "[-]"; }
		}
		public string urlName{
			get { return "[url=" + this._id.ToString() + "_character]" + this._name + "[/url]"; }
		}
		public string coloredUrlName{
			get { return "[url=" + this._id.ToString() + "_character]" + "[" + this._characterColorCode + "]" + this._name + "[-]" + "[/url]"; }
		}
		public string prisonerName{
			get { return "[url=" + this._id.ToString() + "_prisoner]" + this._name + "[/url]"; }
		}
		public int id {
            get { return _id; }
        }
		public GENDER gender{
			get { return _gender; }
		}
		public List<Trait> traits {
            get { return _traits; }
        }
		public List<CharacterTag> tags {
			get { return _tags; }
		}
		public Dictionary<Character, Relationship> relationships {
            get { return _relationships; }
        }
		public CharacterClass characterClass{
			get { return this._characterClass; }
		}
		public RaceSetting raceSetting {
			get { return _raceSetting; }
		}
		public CharacterRole role {
			get { return _role; }
		}
		public Faction faction {
			get { return _faction; }
		}
		public Party party {
			get { return _party; }
		}
        public QuestData questData {
            get { return _questData; }
        }
        public Quest currentQuest {
            get { return _questData.activeQuest; }
        }
        public QuestPhase currentQuestPhase {
            get { return _questData.GetQuestPhase(); }
        }
        public CharacterTask currentTask {
			get { return _currentTask; }
		}
        public ILocation specificLocation {
            get {
                ILocation loc = null;
				loc = (party == null ? ((_isFollowerOf == null || _isFollowerOf.isDead) ? _specificLocation : _isFollowerOf.specificLocation) : party.specificLocation);
                return loc;
            }
        }
		public HexTile currLocation{
			get { return (this.specificLocation != null ? this.specificLocation.tileLocation : null); }
		}
		public Region currentRegion{
			get { return (_party == null ? _currentRegion : _party.currentRegion); }
		}
		public CharacterAvatar avatar{
			get { return _avatar; }
		}
		public List<BodyPart> bodyParts{
			get { return this._bodyParts; }
		}
		public List<Item> equippedItems {
			get { return this._equippedItems; }
		}
		public List<Item> inventory{
			get { return this._inventory; }
		}
		public List<Skill> skills{
			get { return this._skills; }
		}
		public int currentRow{
			get { return this._currentRow; }
		}
		public SIDES currentSide{
			get { return this._currentSide; }
		}
		public bool isDead{
			get { return this._isDead; }
		}
		public bool isFainted{
			get { return this._isFainted; }
		}
		public bool isPrisoner{
			get { return this._isPrisoner; }
		}
		public bool isFollower {
            get { return _isFollower; }
        }
		public List<OldQuest.Quest> activeQuests {
			get { return _activeQuests; }
		}
		public int strength {
			get { return _strength + (int)((float)_strength * _statsModifierPercentage.strPercentage); }
		}
		public int baseStrength {
			get { return _baseStrength; }
		}
		public int intelligence {
			get { return _intelligence + (int)((float)_intelligence * _statsModifierPercentage.intPercentage); }
		}
		public int baseIntelligence {
			get { return _baseIntelligence; }
		}
		public int agility {
			get { return _agility + (int)((float)_agility * _statsModifierPercentage.agiPercentage); }
		}
		public int baseAgility {
			get { return _baseAgility; }
		}
		public int currentHP{
			get { return this._currentHP; }
		}
		public int maxHP {
			get { return this._maxHP; }
		}
		public int baseMaxHP {
			get { return _baseMaxHP; }
		}
		public StatsModifierPercentage statsModifierPercentage {
			get { return _statsModifierPercentage; }
		}
		public int dodgeRate {
			get { return characterClass.dodgeRate + _equippedItems.Sum(x => x.bonusDodgeRate); }
		}
		public int parryRate {
			get { return characterClass.parryRate + _equippedItems.Sum(x => x.bonusParryRate); }
		}
		public int blockRate {
			get { return characterClass.blockRate + _equippedItems.Sum(x => x.bonusBlockRate); }
		}
		public Color characterColor {
			get { return _characterColor; }
		}
		public string characterColorCode {
			get { return _characterColorCode; }
		}
		public BaseLandmark home {
            get { return _home; }
        }
		public BaseLandmark lair {
            get { return _lair; }
        }
		public float remainingHP { //Percentage of remaining HP this character has
            get { return (float)currentHP / (float)maxHP; }
        }
		public int remainingHPPercent {
            get { return (int)(remainingHP * 100); }
        }
		public List<Log> history{
			get { return this._history; }
		}
		public float characterPower{
			get { return (float)_currentHP + (float)((_strength + _intelligence + _agility) * 2) + _equippedWeaponPower;}
		}
		public float equippedWeaponPower{
			get { return _equippedWeaponPower; }
		}
		public List<Character> prisoners{
			get { return this._prisoners; }
		}
		public List<Character> followers{
			get { return this._followers; }
		}
		public object isPrisonerOf{
			get { return this._isPrisonerOf; }
		}
		public int gold {
            get { return _gold; }
        }
		public int prestige {
            get { return _prestige; }
		}
		public bool isDefeated {
			get {
//				if(_party != null){
//					return _party.isDefeated;
//				}
				return _isDefeated; 
			}
		}
        public int civilians {
            get { 
				if(_civiliansByRace == null){
					return 0;
				}
				return _civiliansByRace.Sum(x => x.Value); 
			}
        }
		public Dictionary<RACE, int> civiliansByRace{
			get { return _civiliansByRace; }
		}
//      public Dictionary<MATERIAL, int> materialInventory {
//          get { return _materialInventory; }
//      }
		public List<BaseLandmark> exploredLandmarks {
			get { return _exploredLandmarks; }
		}
		public bool isInCombat{
			get {
				if(_party != null){
					return _party.isInCombat;
				}
				return _isInCombat; 
			}
		}
		public Action currentFunction{
			get { 
//				if(_party != null){
//					return _party.currentFunction;
//				}
				return _currentFunction; 
			}
		}
        public bool isFactionless {
            get { return faction == null; }
        }
		internal bool isFollowersFull{
			get { return followers.Count >= MAX_FOLLOWERS; }
		}
        public int missingFollowers {
            get { return Mathf.Max(0, MAX_FOLLOWERS - followers.Count); }
        }
		public Character isFollowerOf{
			get { return _isFollowerOf; }
		}
		public Character mainCharacter{
			get { return this; }
		}
		public int numOfCharacters{
			get { return 1; }
		}
		public bool doesNotTakePrisoners{
			get { return (_party == null ? characterDoesNotTakePrisoners : _party.doesNotTakePrisoners); }
		}
		public bool characterDoesNotTakePrisoners{
			get { return _doesNotTakePrisoners; }
		}
		public bool cannotBeTakenAsPrisoner{
			get { return _cannotBeTakenAsPrisoner; }
		}
		public Dictionary<Character, List<string>> traceInfo{
			get { return _traceInfo; }
		}
        #endregion

        public Character(CharacterSetup baseSetup, int statAllocationBonus = 0) {
            _id = Utilities.SetID(this);
			_characterClass = baseSetup.characterClass.CreateNewCopy();
			_raceSetting = baseSetup.raceSetting.CreateNewCopy();
			_gender = Utilities.GetRandomGender();
            _name = RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender);
            _traits = new List<Trait> ();
			_tags = new List<CharacterTag> ();
            _relationships = new Dictionary<Character, Relationship>();
			_exploredLandmarks = new List<BaseLandmark> ();
			statusEffects = new List<STATUS_EFFECT>();
			_isDead = false;
			_isFainted = false;
			_isPrisoner = false;
			_isDefeated = false;
			_doesNotTakePrisoners = false;
			_cannotBeTakenAsPrisoner = false;
			_traceInfo = new Dictionary<Character, List<string>> ();
			_isPrisonerOf = null;
			_prisoners = new List<Character> ();
			_history = new List<Log> ();
			_followers = new List<Character> ();
			_isFollowerOf = null;
			_statsModifierPercentage = new StatsModifierPercentage ();
            _questData = new QuestData(this);
			previousActions = new Dictionary<CharacterTask, string> ();

			GenerateRaceTags ();

			AllocateStatPoints (statAllocationBonus);

            //GenerateTraits();

			_strength = _baseStrength;
			_agility = _baseAgility;
			_intelligence = _baseIntelligence;
			_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);

			_equippedItems = new List<Item> ();
			_inventory = new List<Item> ();
			_skills = GetGeneralSkills();
			_skills.AddRange (GetBodyPartSkills ());

			AdjustMaxHP (_baseMaxHP);

            EquipPreEquippedItems (baseSetup);
			GetRandomCharacterColor ();

//          _maxHP = _baseMaxHP;
//          _currentHP = maxHP;

            _activeQuests = new List<OldQuest.Quest>();
			currentCombat = null;
			combatHistory = new Dictionary<int, CombatPrototype> ();
			_combatHistoryID = 0;

			Messenger.AddListener<Region> ("RegionDeath", RegionDeath);
			Messenger.AddListener<List<Region>> ("RegionPsytoxin", RegionPsytoxin);

            //ConstructMaterialInventory();
		}

		private void AllocateStatPoints(int statAllocationBonus){
			_baseMaxHP = _raceSetting.baseHP;
			_baseStrength = _raceSetting.baseStr;
			_baseAgility = _raceSetting.baseAgi;
			_baseIntelligence = _raceSetting.baseInt;

			WeightedDictionary<string> statWeights = new WeightedDictionary<string> ();
			statWeights.AddElement ("strength", _raceSetting.strWeightAllocation);
			statWeights.AddElement ("intelligence", _raceSetting.intWeightAllocation);
			statWeights.AddElement ("agility", _raceSetting.agiWeightAllocation);
			statWeights.AddElement ("hp", _raceSetting.hpWeightAllocation);

			if(statWeights.GetTotalOfWeights() > 0){
				string chosenStat = string.Empty;
				int totalStatAllocation = _raceSetting.statAllocationPoints + statAllocationBonus;
				for (int i = 0; i < totalStatAllocation; i++) {
					chosenStat = statWeights.PickRandomElementGivenWeights ();
					if (chosenStat == "strength") {
						_baseStrength += 5;
					}else if (chosenStat == "intelligence") {
						_baseIntelligence += 5;
					}else if (chosenStat == "agility") {
						_baseAgility += 5;
					}else if (chosenStat == "hp") {
						_baseMaxHP += 50;
					}
				}
			}

			_baseMaxHP += (int)((float)_baseMaxHP * (_characterClass.hpPercentage / 100f));
			_baseStrength += (int)((float)_baseStrength * (_characterClass.strPercentage / 100f));
			_baseAgility += (int)((float)_baseAgility * (_characterClass.agiPercentage / 100f));
			_baseIntelligence += (int)((float)_baseIntelligence * (_characterClass.intPercentage / 100f));
		}
		//Check if the body parts of this character has the attribute necessary and quantity
		internal bool HasAttribute(IBodyPart.ATTRIBUTE attribute, int quantity){
			int count = 0;
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];

				if(bodyPart.statusEffects.Count <= 0){
					if(bodyPart.HasAttribute(attribute)){
						count += 1;
						if(count >= quantity){
							return true;
						}
					}
				}

				for (int j = 0; j < bodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = bodyPart.secondaryBodyParts [j];
					if (secondaryBodyPart.statusEffects.Count <= 0) {
						if (secondaryBodyPart.HasAttribute(attribute)) {
							count += 1;
							if (count >= quantity) {
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		internal bool HasBodyPart(string bodyPartType){
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];

				if(bodyPart.name == bodyPartType){
					return true;
				}

				for (int j = 0; j < bodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = bodyPart.secondaryBodyParts [j];
					if(secondaryBodyPart.name == bodyPartType){
						return true;
					}
				}
			}
			return false;
		}


		//Enables or Disables skills based on skill requirements
		internal void EnableDisableSkills(CombatPrototype combat){
			bool isAllAttacksInRange = true;
			bool isAttackInRange = false;
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				skill.isEnabled = true;

				for (int j = 0; j < skill.skillRequirements.Length; j++) {
					SkillRequirement skillRequirement = skill.skillRequirements [j];
					if (!HasAttribute(skillRequirement.attributeRequired, skillRequirement.itemQuantity)) {
						skill.isEnabled = false;
						break;
					}
				}
				if(!skill.isEnabled){
					continue;
				}
				if(skill is AttackSkill){
					isAttackInRange = combat.HasTargetInRangeForSkill (skill, this);
					if(!isAttackInRange){
						isAllAttacksInRange = false;
						skill.isEnabled = false;
						continue;
					}
				}else if (skill is FleeSkill){
//					skill.isEnabled = false;
//					continue;
					if(this.currentHP >= (this.maxHP / 2)){
						skill.isEnabled = false;
						continue;
					}
				}
			}


			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				if(skill is MoveSkill){
					skill.isEnabled = true;
					if(isAllAttacksInRange){
						skill.isEnabled = false;
						continue;
					}
					if(skill.skillName == "MoveLeft"){
						if (this._currentRow == 1) {
							skill.isEnabled = false;
							continue;
						} else {
							bool hasEnemyOnLeft = false;
							if(combat.charactersSideA.Contains(this)){
								for (int j = 0; j < combat.charactersSideB.Count; j++) {
									Character enemy = combat.charactersSideB [j];
									if(enemy.currentRow < this._currentRow){
										hasEnemyOnLeft = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < combat.charactersSideA.Count; j++) {
									Character enemy = combat.charactersSideA [j];
									if(enemy.currentRow < this._currentRow){
										hasEnemyOnLeft = true;
										break;
									}
								}
							}
							if(!hasEnemyOnLeft){
								skill.isEnabled = false;
								continue;
							}
						}
					}else if(skill.skillName == "MoveRight"){
						if (this._currentRow == 5) {
							skill.isEnabled = false;
						} else {
							bool hasEnemyOnRight = false;
							if(combat.charactersSideA.Contains(this)){
								for (int j = 0; j < combat.charactersSideB.Count; j++) {
									Character enemy = combat.charactersSideB [j];
									if(enemy.currentRow > this._currentRow){
										hasEnemyOnRight = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < combat.charactersSideA.Count; j++) {
									Character enemy = combat.charactersSideA [j];
									if(enemy.currentRow > this._currentRow){
										hasEnemyOnRight = true;
										break;
									}
								}
							}
							if(!hasEnemyOnRight){
								skill.isEnabled = false;
								continue;
							}
						}
					}
				}
			}
		}

		//Changes row number of this character
		internal void SetRowNumber(int rowNumber){
			this._currentRow = rowNumber;
		}

		//Changes character's side
		internal void SetSide(SIDES side){
			this._currentSide = side;
		}

		//Adjust current HP based on specified paramater, but HP must not go below 0
		internal void AdjustHP(int amount){
			this._currentHP += amount;
			this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
			if(this._currentHP == 0){
				FaintOrDeath ();
			}
		}
		internal void SetHP(int amount){
			this._currentHP = amount;
		}
		private string GetFaintOrDeath(){
			WeightedDictionary<string> faintDieDict = new WeightedDictionary<string> ();
			int faintWeight = 100;
			int dieWeight = 50;
			if(HasTrait(TRAIT.GRITTY)){
				faintWeight += 50;
			}
			if(HasTrait(TRAIT.ROBUST)){
				faintWeight += 50;
			}
			if(HasTrait(TRAIT.FRAGILE)){
				dieWeight += 50;
			}
			faintDieDict.AddElement ("faint", 100);
			faintDieDict.AddElement ("die", 50);

			return faintDieDict.PickRandomElementGivenWeights ();
		}
		internal void FaintOrDeath(){
			string pickedWeight = GetFaintOrDeath ();
			if(pickedWeight == "faint"){
				if(this.currentCombat == null){
					Faint ();
				}else{
                    this.currentCombat.CharacterFainted(this);
                }
			}else if(pickedWeight == "die"){
				if(this.currentCombat == null){
					Death ();
				}else{
					this.currentCombat.CharacterDeath (this);
				}
			}
		}

		//When character will faint
		internal void Faint(){
			if(!_isFainted){
				_isFainted = true;
				SetHP (1);
				if (this._party != null) {
					this._party.RemovePartyMember(this);
				}
//				if (this._isFollowerOf != null) {
//					this._isFollowerOf.RemoveFollower(this);
//				}
				if (_isFollower) {
					SetFollowerState (false);
				}
			}
		}

		internal void Unfaint(){
			if (_isFainted) {
				_isFainted = false;
				SetHP (1);
			}
		}

		//Character's death
		internal void Death(){
			if(!_isDead){
				_isDead = true;
				Messenger.RemoveListener<Region> ("RegionDeath", RegionDeath);
				Messenger.RemoveListener<List<Region>> ("RegionPsytoxin", RegionPsytoxin);

				CombatPrototypeManager.Instance.ReturnCharacterColorToPool (_characterColor);

				if(specificLocation is BaseLandmark){
                    Log deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death");
                    deathLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    AddHistory(deathLog);
                    (specificLocation as BaseLandmark).AddHistory(deathLog);
				}

				//Drop all Items
				while (_equippedItems.Count > 0) {
					ThrowItem (_equippedItems [0]);
				}
				while (_inventory.Count > 0) {
					ThrowItem (_inventory [0]);
				}

				if(_home != null){
                    //Remove character home on landmark
					_home.RemoveCharacterHomeOnLandmark (this);
				}

				if(this._faction != null){
					if(this._faction.leader != null && this._faction.leader.id == this.id) {
						//If this character is the leader of a faction, set that factions leader as null
						this._faction.SetLeader(null);
					}
					this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
				}

                //CheckForInternationalIncident();

                if (this._party != null) {
					Party party = this._party;
					party.RemovePartyMember(this, true);
					if(party.partyLeader.id == this._id){
						party.DisbandParty ();
                    }
				}else{
					this.specificLocation.RemoveCharacterFromLocation(this);
				}

//				if(this._isFollowerOf != null){
//					this._isFollowerOf.RemoveFollower (this);
//					if(this.specificLocation != null){
//						this.specificLocation.RemoveCharacterFromLocation(this);
//					}
//				}
				if(_isFollower){
					SetFollowerState (false);
				}
                if (_avatar != null) {
                    _avatar.RemoveCharacter(this); //if the character has an avatar, remove it from the list of characters
                }

                if (_isPrisoner){
					PrisonerDeath ();
				}
				if(_role != null){
					_role.DeathRole ();
				}
				while(_tags.Count > 0){
					RemoveCharacterTag (_tags [0]);
				}
//				if(Messenger.eventTable.ContainsKey("CharacterDeath")){
//					Messenger.Broadcast ("CharacterDeath", this);
//				}
				if(onCharacterDeath != null){
					onCharacterDeath();
				}
                onCharacterDeath = null;
                Messenger.Broadcast(Signals.CHARACTER_DEATH, this);
                Debug.Log(this.name + " died!");
            }
		}
        internal void AddActionOnDeath(OnCharacterDeath onDeathAction) {
            onCharacterDeath += onDeathAction;
        }
        internal void RemoveActionOnDeath(OnCharacterDeath onDeathAction) {
            onCharacterDeath -= onDeathAction;
        }
        private void CheckForInternationalIncident() {
            //a non-Adventurer character from a tribe dies while in a region owned by another tribe
			if(this._role == null){
				return;
			}
            if (this.faction == null) {
                return;
            }
            Faction ownerOfCurrLocation = this.currLocation.region.owner;
			if (ownerOfCurrLocation != null && ownerOfCurrLocation.id != this.faction.id) {
                if(currentTask != null && currentTask.taskType == TASK_TYPE.QUEST) { //if this character is in a quest when he/she died
                    OldQuest.Quest currentQuest = (OldQuest.Quest)currentTask;
                    if (FactionManager.Instance.IsQuestHarmful(currentQuest.questType)) { //check if the quest is meant to negatively impact a faction
                        //if it is, check if this character died on a region owned by the faction he/she means to negatively impact
                        //if he/she is, do not count this character's death as an international incident
                        if (currentQuest.targetFaction.id != ownerOfCurrLocation.id) {
                            //otherwise, this is an international incident
                            FactionManager.Instance.InternationalIncidentOccured(this.faction, this.currLocation.region.owner, INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH, this);
                        }
                    } else {
                        FactionManager.Instance.InternationalIncidentOccured(this.faction, this.currLocation.region.owner, INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH, this);
                    }
                } else {
                    FactionManager.Instance.InternationalIncidentOccured(this.faction, this.currLocation.region.owner, INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH, this);
                }
            }
        }

		#region Body Parts
		/*
     * Add new body parts here.
     * */
		internal void AddBodyPart(BodyPart bodyPart) {
			bodyParts.Add(bodyPart);
		}
		internal IBodyPart GetBodyPartForWeapon(Weapon weapon) {
			List<IBodyPart> allBodyParts = new List<IBodyPart>();
			for (int i = 0; i < bodyParts.Count; i++) {
				BodyPart currBodyPart = bodyParts[i];
				allBodyParts.Add(currBodyPart);
				for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
					allBodyParts.Add(currBodyPart.secondaryBodyParts[j]);
				}
			}
			for (int i = 0; i < allBodyParts.Count; i++) {
				IBodyPart currBodyPart = allBodyParts[i];
				bool meetsRequirements = true;
				//check if currBodyPart meets the weapons requirements
				for (int j = 0; j < weapon.equipRequirements.Count; j++) {
					IBodyPart.ATTRIBUTE currReq = weapon.equipRequirements[j];
					if (!currBodyPart.HasUnusedAttribute(currReq)) {
						meetsRequirements = false;
						break;
					}
				}
				if (meetsRequirements) {
					return currBodyPart;
				}
			}
			return null;
		}
		internal IBodyPart GetBodyPartForArmor(Armor armor) {
			IBodyPart.ATTRIBUTE neededAttribute = Utilities.GetNeededAttributeForArmor(armor);
			for (int i = 0; i < bodyParts.Count; i++) {
				BodyPart currBodyPart = bodyParts[i];
				//check if currBodyPart can equip the armor
				if (currBodyPart.HasUnusedAttribute(neededAttribute)) {
					return currBodyPart;
				}
				for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
					//check if currBodyPart can equip the armor
					if (currBodyPart.secondaryBodyParts[j].HasUnusedAttribute(neededAttribute)) {
						return currBodyPart.secondaryBodyParts[j];
					}
				}
			}
			return null;
		}
		internal bool HasActivatableBodyPartSkill(){
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.BODY_PART){
					return true;
				}
			}
			return false;
		}
		internal bool HasActivatableWeaponSkill(){
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.WEAPON){
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Items
		//If a character picks up an item, it is automatically added to his/her inventory
		internal void PickupItem(Item item){
			Item newItem = item;
			if(item.isUnlimited){
				newItem = item.CreateNewCopy ();
				newItem.isUnlimited = false;
			}
            if (_inventory.Contains(newItem)) {
                throw new Exception(this.name + " already has an instance of " + newItem.itemName);
            }
			this._inventory.Add (newItem);
			if(newItem.owner == null){
				OwnItem (newItem);
			}
            Log obtainLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "obtain_item");
            obtainLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            obtainLog.AddToFillers(null, item.nameWithQuality, LOG_IDENTIFIER.ITEM_1);
            AddHistory(obtainLog);
            if (specificLocation is BaseLandmark) {
                (specificLocation as BaseLandmark).AddHistory(obtainLog);
            }

            Messenger.Broadcast(Signals.OBTAIN_ITEM, this, newItem);
            newItem.OnItemPutInInventory(this);
        }

		internal void ThrowItem(Item item, bool addInLandmark = true){
			if(item.isEquipped){
				UnequipItem (item);
			}
			this._inventory.Remove (item);
			item.exploreWeight = 15;
			if(addInLandmark){
				ILocation location = specificLocation;
				if(location != null && location is BaseLandmark){
					BaseLandmark landmark = (BaseLandmark)location;
					landmark.AddItemInLandmark(item);
				}
			}

		}

        internal void DropItem(Item item) {
            ThrowItem(item);
            ILocation location = specificLocation;
            if (location != null && location is BaseLandmark) {
                BaseLandmark landmark = (BaseLandmark)location;
                Log dropLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "drop_item");
                dropLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                dropLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
                dropLog.AddToFillers(this, location.locationName, LOG_IDENTIFIER.LANDMARK_1);
                AddHistory(dropLog);
                (location as BaseLandmark).AddHistory(dropLog);
            }

        }
        internal void CheckForItemDrop() {
            if (specificLocation != null && specificLocation is BaseLandmark) {
                if (UnityEngine.Random.Range(0, 100) < 3) {
                    Dictionary<Item, Character> itemPool = new Dictionary<Item, Character>();
                    List<Character> charactersToCheck = new List<Character>();
					if(_party == null){
						charactersToCheck.Add(this);
					}else{
						charactersToCheck.AddRange(_party.partyMembers);
					}
                    for (int i = 0; i < charactersToCheck.Count; i++) {
                        ECS.Character currCharacter = charactersToCheck[i];
                        for (int j = 0; j < currCharacter.inventory.Count; j++) {
                            itemPool.Add(currCharacter.inventory[j], currCharacter);
                        }
                    }

                    //Drop a random item from the pool
                    if (itemPool.Count > 0) {
                        Item chosenItem = itemPool.Keys.ElementAt(UnityEngine.Random.Range(0, itemPool.Count));
                        Character owner = itemPool[chosenItem];
                        owner.DropItem(chosenItem);
                    }
                }
            }
        }

        //If character set up has pre equipped items, equip it here evey time a character is made
        internal void EquipPreEquippedItems(CharacterSetup charSetup){
			if(charSetup.preEquippedItems.Count > 0){
				for (int i = 0; i < charSetup.preEquippedItems.Count; i++) {
					EquipItem (charSetup.preEquippedItems [i].itemType, charSetup.preEquippedItems [i].itemName);
				}
			}
		}

		//For prototype only, in reality, an instance of an Item must be passed as parameter
		//Equip generic item, can be a weapon, armor, etc.
		public void EquipItem(ITEM_TYPE itemType, string itemName) {
			string strItemType = itemType.ToString();
			string path = "Assets/CombatPrototype/Data/Items/" + strItemType + "/" + itemName + ".json";
			if(System.IO.File.Exists(path)){
				string dataAsJson = System.IO.File.ReadAllText(path);
				if (strItemType.Contains("WEAPON")) {
					Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
					weapon.ConstructSkillsList ();
					TryEquipWeapon(weapon);
				} else if (strItemType.Contains("ARMOR")) {
					Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
					TryEquipArmor(armor);
				}
			}
		}
		public void EquipItem(string itemType, string itemName) {
			string path = "Assets/CombatPrototype/Data/Items/" + itemType + "/" + itemName + ".json";
			string dataAsJson = System.IO.File.ReadAllText(path);
			if (itemType.Contains("WEAPON")) {
				Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
				weapon.ConstructSkillsList ();
				TryEquipWeapon(weapon);
			} else if (itemType.Contains("ARMOR")) {
				Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
				TryEquipArmor(armor);
			}
		}

        /*
         this is the real way to equip an item
         this will return a boolean whether the character successfully equipped
         the item or not.
             */
        internal bool EquipItem(Item item){
			bool hasEquipped = false;
			if (item is Weapon) {
				Weapon weapon = (Weapon)item;
				hasEquipped = TryEquipWeapon(weapon);
			} else if (item is Armor) {
				Armor armor = (Armor)item;
				hasEquipped = TryEquipArmor(armor);
			}
            if (hasEquipped) {
                Log equipLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "equip_item");
                equipLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                equipLog.AddToFillers(null, item.nameWithQuality, LOG_IDENTIFIER.ITEM_1);
                AddHistory(equipLog);
            }
            return hasEquipped;
		}

		//Unequips an item of a character, whether it's a weapon, armor, etc.
		internal void UnequipItem(Item item){
			if(item is Weapon){
				UnequipWeapon ((Weapon)item);
			}else if(item is Armor){
				UnequipArmor ((Armor)item);
			}
			RemoveEquippedItem(item);
		}

		//Unown an item making the owner of it null, if successfully unowned, return true, otherwise, return false
		internal bool UnownItem(Item item){
			if(item.owner.id == this._id){
				item.SetOwner (null);
				return true;
			}
			return false;
		}

		//Own an Item
		internal void OwnItem(Item item){
			item.SetOwner (this);
		}

		//Transfer item ownership
		internal void TransferItemOwnership(Item item, Character newOwner){
			newOwner.OwnItem (item);
		}

		//Try to equip a weapon to a body part of this character and add it to the list of items this character have
		internal bool TryEquipWeapon(Weapon weapon){
			for (int j = 0; j < weapon.equipRequirements.Count; j++) {
				IBodyPart.ATTRIBUTE currReq = weapon.equipRequirements[j];
				if(!AttachWeaponToBodyPart(weapon, currReq)){
					DetachWeaponFromBodyParts (weapon);
					return false;
				}
			}
			Weapon newWeapon = weapon;
			if(weapon.isUnlimited){
				newWeapon = (Weapon)weapon.CreateNewCopy ();
				newWeapon.isUnlimited = false;
			}
			AddEquippedItem(newWeapon);
			newWeapon.ResetDurability();
//			weapon.SetOwner(this);
			if(newWeapon.owner == null){
				OwnItem (newWeapon);
			}
			_equippedWeaponPower += newWeapon.weaponPower;

			for (int i = 0; i < newWeapon.skills.Count; i++) {
				this._skills.Add (newWeapon.skills [i]);
			}

			//          Debug.Log(this.name + " equipped " + weapon.itemName + " to " + bodyPart.bodyPart.ToString());
			//CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
			return true;
		}
		private bool AttachWeaponToBodyPart(Weapon weapon, IBodyPart.ATTRIBUTE req){
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart currBodyPart = this._bodyParts[i];
				if(currBodyPart.AttachItem(weapon, req)){
					return true;
				}
				for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
					if(currBodyPart.secondaryBodyParts[j].AttachItem(weapon, req)){
						return true;
					}
				}
			}
			return false;
		}
		private void DetachWeaponFromBodyParts(Weapon weapon){
			for (int i = 0; i < weapon.equipRequirements.Count; i++) {
				IBodyPart.ATTRIBUTE req = weapon.equipRequirements [i];
				for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
					if(weapon.bodyPartsAttached[j].DettachItem(weapon, req)){
						break;
					}
				}
			}
		}
		//Unequips weapon of a character
		private void UnequipWeapon(Weapon weapon) {
			DetachWeaponFromBodyParts (weapon);
			for (int i = 0; i < weapon.skills.Count; i++) {
				this._skills.Remove (weapon.skills [i]);
			}
			_equippedWeaponPower -= weapon.weaponPower;
		}

		//Try to equip an armor to a body part of this character and add it to the list of items this character have
		internal bool TryEquipArmor(Armor armor){
			IBodyPart bodyPartToEquip = GetBodyPartForArmor(armor);
			if(bodyPartToEquip == null){
				return false;
			}
			Armor newArmor = armor;
			if(armor.isUnlimited){
				newArmor = (Armor)armor.CreateNewCopy ();
				newArmor.isUnlimited = false;
			}
			bodyPartToEquip.AttachItem(newArmor, Utilities.GetNeededAttributeForArmor(newArmor));
			//			armor.bodyPartAttached = bodyPart;
			AddEquippedItem(newArmor);
			newArmor.ResetDurability();
//			armor.SetOwner(this);
			if(newArmor.owner == null){
				OwnItem (newArmor);
			}
			//Debug.Log(this.name + " equipped " + newArmor.itemName + " to " + bodyPartToEquip.name);
            if(CombatPrototypeUI.Instance != null) {
                CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
            }
			return true;
		}
		//Unequips armor of a character
		private void UnequipArmor(Armor armor) {
			armor.bodyPartAttached.DettachItem(armor, Utilities.GetNeededAttributeForArmor(armor));
		}
		internal void AddEquippedItem(Item newItem){
			this._inventory.Remove (newItem);
			this._equippedItems.Add (newItem);
			newItem.SetEquipped (true);
			AddItemBonuses (newItem);
		}
		internal void RemoveEquippedItem(Item newItem){
            if (_inventory.Contains(newItem)) {
                throw new Exception(this.name + " already has an instance of " + newItem.itemName);
            }
            this._inventory.Add (newItem);
			this._equippedItems.Remove (newItem);
			newItem.SetEquipped (false);
			RemoveItemBonuses (newItem);
		}
		internal bool HasWeaponEquipped(){
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if(currItem.itemType == ITEM_TYPE.WEAPON) {
					return true;
				}
			}
			return false;
		}
		internal bool HasArmorEquipped(){
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if(currItem.itemType == ITEM_TYPE.ARMOR) {
					return true;
				}
			}
			return false;
		}
		internal List<Weapon> GetAllAttachedWeapons() {
			List<Weapon> weapons = new List<Weapon>();
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if(currItem.itemType == ITEM_TYPE.WEAPON) {
					weapons.Add((Weapon)currItem);
				}
			}
			return weapons;
		}
		internal List<Armor> GetAllAttachedArmor() {
			List<Armor> weapons = new List<Armor>();
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if (currItem.itemType == ITEM_TYPE.ARMOR) {
					weapons.Add((Armor)currItem);
				}
			}
			return weapons;
		}
		internal bool HasEquipmentOfType(EQUIPMENT_TYPE equipmentType, IBodyPart.ATTRIBUTE attribute) {
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if(currItem.itemType == ITEM_TYPE.ARMOR) {
					Armor armor = (Armor)currItem;
					if ((EQUIPMENT_TYPE)armor.armorType == equipmentType && (armor.attributes.Contains(attribute) || attribute == IBodyPart.ATTRIBUTE.NONE)) {
						return true;
					}
				} else if (currItem.itemType == ITEM_TYPE.WEAPON) {
					Weapon weapon = (Weapon)currItem;
					if ((EQUIPMENT_TYPE)weapon.weaponType == equipmentType && (weapon.attributes.Contains(attribute) || attribute == IBodyPart.ATTRIBUTE.NONE)) {
						return true;
					}
				}
			}
			return false;
		}
        internal bool HasEquipmentOfType(EQUIPMENT_TYPE equipmentType) {
            for (int i = 0; i < equippedItems.Count; i++) {
                Item currItem = equippedItems[i];
                if (currItem.itemType == ITEM_TYPE.ARMOR) {
                    Armor armor = (Armor)currItem;
                    if ((EQUIPMENT_TYPE)armor.armorType == equipmentType) {
                        return true;
                    }
                } else if (currItem.itemType == ITEM_TYPE.WEAPON) {
                    Weapon weapon = (Weapon)currItem;
                    if ((EQUIPMENT_TYPE)weapon.weaponType == equipmentType) {
                        return true;
                    }
                }
            }
            return false;
        }
        /*
         Get the most needed armor type of this character,
         if this returns NONE. That means that this character already
         has all types of armor equipped.
             */
        internal ARMOR_TYPE GetNeededArmorType() {
            for (int i = 0; i < Utilities.orderedArmorTypes.Count; i++) {
                ARMOR_TYPE currArmorType = Utilities.orderedArmorTypes[i];
                //Get the armor type that the character doesn't currently have
                if (!this.HasEquipmentOfType((EQUIPMENT_TYPE)currArmorType)) {
                    return currArmorType; //character has not yet equipped an armor of this type
                }
            }
            return ARMOR_TYPE.NONE;
        }
        /*
         Get armor types that this character has not equipped yet.
             */
        internal List<EQUIPMENT_TYPE> GetMissingArmorTypes() {
            List<EQUIPMENT_TYPE> missingArmor = new List<EQUIPMENT_TYPE>();
            for (int i = 0; i < Utilities.orderedArmorTypes.Count; i++) {
                EQUIPMENT_TYPE currArmorType = (EQUIPMENT_TYPE)Utilities.orderedArmorTypes[i];
                //Get the armor type that the character doesn't currently have
                if (!this.HasEquipmentOfType((EQUIPMENT_TYPE)currArmorType)) {
                    missingArmor.Add(currArmorType); //character has not yet equipped an armor of this type
                }
            }
            return missingArmor;
        }
        /*
        Get a random weapon type for a character
        given its allowed weapon types.
             */
        internal WEAPON_TYPE GetWeaponTypeForCharacter() {
            if (_characterClass.className.Equals("Classless")) {
                return WEAPON_TYPE.NONE;
            }
            return this.characterClass.allowedWeaponTypes[UnityEngine.Random.Range(0, this.characterClass.allowedWeaponTypes.Count)];
        }
        internal void AdjustGold(int amount) {
            _gold += amount;
            _gold = Mathf.Max(0, _gold);
        }
        internal void AdjustPrestige(int amount) {
            _prestige += amount;
            _prestige = Mathf.Max(0, _prestige);
        }
        /*
         Get the Item type that this character needs,
         this is mainly used for upgrading gear at a settlement.
             */
        internal ITEM_TYPE GetNeededItemType() {
            if (!HasWeaponEquipped()) {
                return ITEM_TYPE.WEAPON;
            }
            return ITEM_TYPE.ARMOR;
        }
        /*
         Get the list of equipment that
         this character needs
             */
        internal List<EQUIPMENT_TYPE> GetNeededEquipmentTypes() {
            List<EQUIPMENT_TYPE> neededEquipment = new List<EQUIPMENT_TYPE>();
            if (!_characterClass.className.Equals("Classless") && !HasWeaponEquipped()) {
                neededEquipment.Add((EQUIPMENT_TYPE)GetWeaponTypeForCharacter());
            }
            neededEquipment.AddRange(GetMissingArmorTypes());
            return neededEquipment;
        }
        internal bool HasItem(string itemName) {
			for (int i = 0; i < _equippedItems.Count; i++) {
				Item currItem = _equippedItems[i];
				if (currItem.itemName.Equals(itemName)) {
					return true;
				}
			}
            for (int i = 0; i < _inventory.Count; i++) {
                Item currItem = _inventory[i];
                if (currItem.itemName.Equals(itemName)) {
                    return true;
                }
            }
            return false;
        }
        internal Item GetItemInInventory(string itemName){
			for (int i = 0; i < _inventory.Count; i++) {
				Item currItem = _inventory[i];
				if (currItem.itemName.Equals(itemName)) {
					return currItem;
				}
			}
			return null;
		}
		internal Item GetItemInEquipped(string itemName){
			for (int i = 0; i < _equippedItems.Count; i++) {
				Item currItem = _equippedItems[i];
				if (currItem.itemName.Equals(itemName)) {
					return currItem;
				}
			}
			return null;
		}
		public Item GetItemInAll(string itemName){
			if(_equippedItems.Count > 0){
				for (int i = 0; i < _equippedItems.Count; i++) {
					if(_equippedItems[i].itemName == itemName){
						return _equippedItems [i];
					}
				}
			}
			if(_inventory.Count > 0){
				for (int i = 0; i < _inventory.Count; i++) {
					if(_inventory[i].itemName == itemName){
						return _inventory [i];
					}
				}
			}
			return null;
		}
        #endregion

		#region Status Effects
		internal void AddStatusEffect(STATUS_EFFECT statusEffect){
			this.statusEffects.Add (statusEffect);
		}

		internal void RemoveStatusEffect(STATUS_EFFECT statusEffect){
			this.statusEffects.Remove (statusEffect);
		}

        internal void CureStatusEffects(){
			for (int i = 0; i < statusEffects.Count; i++) {
				STATUS_EFFECT statusEffect = statusEffects [i];
				int chance = Utilities.rng.Next (0, 100);
				if (chance < 15) {
					CombatPrototypeManager.Instance.combat.AddCombatLog(this.name + " is cured from " + statusEffect.ToString ().ToLower () + ".", this.currentSide);
					RemoveStatusEffect (statusEffect);
					i--;
				}
			}
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];
				if(bodyPart.statusEffects.Count > 0){
					for (int j = 0; j < bodyPart.statusEffects.Count; j++) {
						STATUS_EFFECT statusEffect = bodyPart.statusEffects [j];
						if(statusEffect != STATUS_EFFECT.DECAPITATED){
							int chance = Utilities.rng.Next (0, 100);
							if(chance < 15){
								CombatPrototypeManager.Instance.combat.AddCombatLog(this.name + "'s " + bodyPart.name.ToLower () + " is cured from " + statusEffect.ToString ().ToLower () + ".", this.currentSide);
								bodyPart.RemoveStatusEffectOnSecondaryBodyParts (statusEffect);
								bodyPart.statusEffects.RemoveAt (j);
								j--;
							}
						}
					}
				}
			}
		}

        internal bool HasStatusEffect(STATUS_EFFECT statusEffect) {
			if (statusEffects.Contains(statusEffect)) {
				return true;
			}
            for (int i = 0; i < this._bodyParts.Count; i++) {
                BodyPart bodyPart = this._bodyParts[i];
                if (bodyPart.statusEffects.Contains(statusEffect)) {
                    return true;
                }
            }
            return false;
        }
		#endregion

		#region Skills
		private List<Skill> GetGeneralSkills(){
			List<Skill> generalSkills = new List<Skill> ();
			string mainPath = "Assets/CombatPrototype/Data/Skills/GENERAL/";
			string[] folders = System.IO.Directory.GetDirectories (mainPath);
			for (int i = 0; i < folders.Length; i++) {
				string path = folders[i] + "/";
				DirectoryInfo di = new DirectoryInfo (path);
				if (di.Name == "ATTACK") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (attackSkill);
					}
				}else if (di.Name == "HEAL") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						HealSkill healSkill = JsonUtility.FromJson<HealSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (healSkill);
					}
				}else if (di.Name == "FLEE") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (fleeSkill);
					}
				}else if (di.Name == "MOVE") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (moveSkill);
					}
				}else if (di.Name == "OBTAIN_ITEM") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (obtainSkill);
					}
				}
			}
			return generalSkills;
		}
		private List<Skill> GetBodyPartSkills(){
			List<Skill> allBodyPartSkills = new List<Skill>();
			foreach (string skillName in SkillManager.Instance.bodyPartSkills.Keys) {
				bool requirementsPassed = true;
				Skill skill	= SkillManager.Instance.bodyPartSkills [skillName];
				for (int j = 0; j < skill.skillRequirements.Length; j++) {
					if(!HasAttribute(skill.skillRequirements[j].attributeRequired, skill.skillRequirements[j].itemQuantity)){
						requirementsPassed = false;
						break;
					}
				}
				if(requirementsPassed){
					allBodyPartSkills.Add (skill.CreateNewCopy ());
				}
			}
//			for (int i = 0; i < SkillManager.Instance.allSkills.Count; i++) {
//				bool requirementsPassed = true;
//				for (int j = 0; j < SkillManager.Instance.attributeSkills[i].requirements.Length; j++) {
//					if(!HasAttribute(SkillManager.Instance.attributeSkills[i].requirements[j].attributeRequired, SkillManager.Instance.attributeSkills[i].requirements[j].itemQuantity)){
//						requirementsPassed = false;
//						break;
//					}
//				}
//				if(requirementsPassed){
//					for (int j = 0; j < SkillManager.Instance.attributeSkills[i].skills.Count; j++) {
//						allBodyPartSkills.Add (SkillManager.Instance.attributeSkills [i].skills [j].CreateNewCopy());
//					}
//				}
//			}
			return allBodyPartSkills;
		}
		#endregion

		#region Stats
		internal void SetAgility (int amount){
			this._agility = amount;
		}
		internal void AdjustAgility (int amount){
			this._agility += amount;
		}
		#endregion

		private void AddItemBonuses(Item item){
			AdjustMaxHP (item.bonusMaxHP);
			this._strength += item.bonusStrength;
			this._intelligence += item.bonusIntelligence;
			this._agility += item.bonusAgility;
		}
		private void RemoveItemBonuses(Item item){
			AdjustMaxHP (-item.bonusMaxHP);
			this._strength -= item.bonusStrength;
			this._intelligence -= item.bonusIntelligence;
			this._agility -= item.bonusAgility;
		}
		internal void AdjustMaxHP(int amount){
			this._fixedMaxHP += amount;
//			this._maxHP = this._maxHP + (int)((float)this._maxHP * _statsModifierPercentage.hpPercentage);
			RecomputeMaxHP();
		}
		private void RecomputeMaxHP(){
			int previousMaxHP = this._maxHP;
			this._maxHP = this._fixedMaxHP + (int)((float)this._fixedMaxHP * _statsModifierPercentage.hpPercentage);
			if(this._currentHP > this._maxHP || this._currentHP == previousMaxHP){
				this._currentHP = this._maxHP;
			}
		}

		private void GetRandomCharacterColor(){
			_characterColor = CombatPrototypeManager.Instance.UseRandomCharacterColor ();
			_characterColorCode = ColorUtility.ToHtmlStringRGBA (_characterColor).Substring (0, 6);
		}
		public void SetCharacterColor(Color color){
			_characterColor = color;
			_characterColorCode = ColorUtility.ToHtmlStringRGBA (_characterColor).Substring (0, 6);
		}

		#region Roles
		public void AssignRole(CHARACTER_ROLE role) {
			if(_role != null){
				_role.ChangedRole ();
			}
			switch (role) {
		        case CHARACTER_ROLE.CHIEFTAIN:
			        _role = new Chieftain(this);
			        break;
		        case CHARACTER_ROLE.VILLAGE_HEAD:
			        _role = new VillageHead(this);
			        break;
		        case CHARACTER_ROLE.WARLORD:
			        _role = new Warlord(this);
			        break;
		        case CHARACTER_ROLE.HERO:
			        _role = new Hero(this);
			        break;
		        //case CHARACTER_ROLE.ADVENTURER:
		        //	_role = new Adventurer(this);
		        //	break;
		        case CHARACTER_ROLE.COLONIST:
			        _role = new Colonist(this);
			        break;
                case CHARACTER_ROLE.WORKER:
                    _role = new Worker(this);
                    break;
		        case CHARACTER_ROLE.TAMED_BEAST:
			        _role = new TamedBeast(this);
			        break;
                case CHARACTER_ROLE.ANCIENT_VAMPIRE:
                    _role = new AncientVampire(this);
                    break;
		        case CHARACTER_ROLE.CRATER_BEAST:
			        _role = new CraterBeast(this);
			        break;
		        case CHARACTER_ROLE.SLYX:
			        _role = new Slyx(this);
			        break;
		        case CHARACTER_ROLE.VILLAIN:
			        _role = new Villain(this);
			        break;
                case CHARACTER_ROLE.FOLLOWER:
                    _role = new Follower(this);
                    break;
                case CHARACTER_ROLE.HERMIT:
                    _role = new Hermit(this);
                    break;
                case CHARACTER_ROLE.BANDIT:
                    _role = new Bandit(this);
                    break;
                case CHARACTER_ROLE.BEAST:
                    _role = new Beast(this);
                    break;
                default:
		            break;
			}
            if (_role != null) {
                _role.OnAssignRole();
            }
		}
		public void ChangeRole(){
			//TODO: Things to do when a character changes role

//			AssignRole(role);
			if(_raceSetting.tags.Contains(CHARACTER_TAG.SAPIENT)){
				CHARACTER_ROLE roleToCreate = CHARACTER_ROLE.WORKER;
				WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();
				if (characterRoleProductionDictionary.GetTotalOfWeights () > 0) {
					roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights ();
				}
				AssignRole(roleToCreate);
			}else{
				AssignRole (CHARACTER_ROLE.TAMED_BEAST);
			}
		}
		#endregion

		#region Character Class
		public void AssignClass(CharacterClass charClass) {
			_characterClass = charClass;
		}
		#endregion

		#region Traits
        private void GenerateTraits() {
            CharacterType baseCharacterType = CharacterManager.Instance.GetRandomCharacterType();
            _characterType = baseCharacterType;
            _allTraits = new List<TRAIT>(baseCharacterType.otherTraits);
            //Charisma
            if (baseCharacterType.charismaTrait == CHARISMA.NONE) {
                TRAIT charismaTrait = GenerateCharismaTrait();
                if(charismaTrait != TRAIT.NONE) {
                    _allTraits.Add(charismaTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.charismaTrait);
            }
            //Intelligence
            if (baseCharacterType.intelligenceTrait == INTELLIGENCE.NONE) {
                TRAIT intTrait = GenerateIntelligenceTrait();
                if(intTrait != TRAIT.NONE) {
                    _allTraits.Add(intTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.intelligenceTrait);
            }
            //Efficiency
            if (baseCharacterType.efficiencyTrait == EFFICIENCY.NONE) {
                TRAIT efficiencyTrait = GenerateEfficiencyTrait();
                if (efficiencyTrait != TRAIT.NONE) {
                    _allTraits.Add(efficiencyTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.efficiencyTrait);
            }
            //Military
            if (baseCharacterType.militaryTrait == MILITARY.NONE) {
                TRAIT militaryTrait = GenerateMilitaryTrait();
                _allTraits.Add(militaryTrait);
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.militaryTrait);
            }
            //Health
            if (baseCharacterType.healthTrait == HEALTH.NONE) {
                TRAIT healthTrait = GenerateHealthTrait();
                if(healthTrait != TRAIT.NONE) {
                    _allTraits.Add(healthTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.healthTrait);
            }
            //Strength
            if (baseCharacterType.strengthTrait == STRENGTH.NONE) {
                TRAIT strengthTrait = GenerateStrengthTrait();
                if (strengthTrait != TRAIT.NONE) {
                    _allTraits.Add(strengthTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.strengthTrait);
            }
            //Agility
            if (baseCharacterType.agilityTrait == AGILITY.NONE) {
                TRAIT agilityTrait = GenerateAgilityTrait();
                if (agilityTrait != TRAIT.NONE) {
                    _allTraits.Add(agilityTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.agilityTrait);
            }
            _traits = new List<Trait>();
            for (int i = 0; i < _allTraits.Count; i++) {
                TRAIT currTrait = _allTraits[i];
                Trait trait = CharacterManager.Instance.CreateNewTraitForCharacter(currTrait, this);
                if (trait != null) {
					AddTrait (trait);
                }
            }
        }
		public void AddTrait(Trait trait){
			trait.AssignCharacter(this);
			_traits.Add(trait);
			AddTraitBonuses (trait);
		}
		public void RemoveTrait(Trait trait){
			trait.AssignCharacter(null);
			_traits.Remove(trait);
			RemoveTraitBonuses (trait);
		}
        private TRAIT GenerateCharismaTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.CHARISMATIC; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.REPULSIVE; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateIntelligenceTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.SMART; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.DUMB; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateEfficiencyTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.EFFICIENT; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.INEPT; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateMilitaryTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.HOSTILE; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.PACIFIST; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateHealthTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.ROBUST; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.FRAGILE; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateStrengthTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.STRONG; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.WEAK; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateAgilityTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.AGILE; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.CLUMSY; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        public bool HasTrait(TRAIT trait) {
			for (int i = 0; i < _traits.Count; i++) {
				if(_traits[i].trait == trait) {
					return true;
				}
			}
			return false;
		}
		private void AddTraitBonuses(Trait trait){
			_statsModifierPercentage.intPercentage += trait.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage += trait.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage += trait.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage += trait.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
		private void RemoveTraitBonuses(Trait trait){
			_statsModifierPercentage.intPercentage -= trait.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage -= trait.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage -= trait.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage -= trait.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
        #endregion

		#region Character Tags
		public CharacterTag AssignTag(CHARACTER_TAG tag) {
			if(HasTag(tag)){
				return null;
			}
			CharacterTag charTag = null;
			switch (tag) {
			case CHARACTER_TAG.ANCIENT_KNOWLEDGE:
				charTag = new AncientKnowledge (this);
				break;
			case CHARACTER_TAG.CRIMINAL:
				charTag = new Criminal (this);
				break;
			case CHARACTER_TAG.HYPNOTIZED:
				charTag = new Hypnotized (this);
				break;
			case CHARACTER_TAG.SAPIENT:
				charTag = new Sapient (this);
				break;
			case CHARACTER_TAG.TERMINALLY_ILL:
				charTag = new TerminallyIll (this);
				break;
			case CHARACTER_TAG.VAMPIRE:
				charTag = new Vampire (this);
				break;
			case CHARACTER_TAG.LOST_HEIR:
				charTag = new LostHeir (this);
				break;
            case CHARACTER_TAG.SUCCESSOR:
                charTag = new Successor(this);
                break;
			case CHARACTER_TAG.TYRANNICAL:
				charTag = new Tyrannical(this);
				break;
			case CHARACTER_TAG.WARMONGER:
				charTag = new Warmonger(this);
				break;
			case CHARACTER_TAG.MILD_PSYTOXIN:
				charTag = new MildPsytoxin(this);
				break;
			case CHARACTER_TAG.MODERATE_PSYTOXIN:
				charTag = new ModeratePsytoxin(this);
				break;
			case CHARACTER_TAG.SEVERE_PSYTOXIN:
				charTag = new SeverePsytoxin(this);
				break;
			case CHARACTER_TAG.RITUALIST:
				charTag = new Ritualist(this);
				break;
			case CHARACTER_TAG.HERBALIST:
				charTag = new Herbalist(this);
				break;
            case CHARACTER_TAG.HIBERNATES:
                charTag = new Hibernates(this);
                break;
            case CHARACTER_TAG.PREDATOR:
                charTag = new Predator(this);
                break;
            }
			if(charTag != null){
				AddCharacterTag (charTag);
			}
            return charTag;
		}
		private void GenerateRaceTags(){
			for (int i = 0; i < _raceSetting.tags.Count; i++) {
				AssignTag (_raceSetting.tags [i]);
			}
		}
		public void AddCharacterTag(CharacterTag tag){
			_tags.Add(tag);
			AddCharacterTagBonuses (tag);
			tag.Initialize ();
		}
		public bool RemoveCharacterTag(CharacterTag tag){
			if(_tags.Remove(tag)){
				tag.OnRemoveTag();
				RemoveCharacterTagBonuses (tag);
				return true;
			}
			return false;
		}
		public bool RemoveCharacterTag(CHARACTER_TAG tag) {
            for (int i = 0; i < _tags.Count; i++) {
                CharacterTag currTag = _tags[i];
                if (currTag.tagType == tag) {
                    return RemoveCharacterTag(currTag);
                }
            }
			return false;
        }
        private void AddCharacterTagBonuses(CharacterTag tag){
			_statsModifierPercentage.intPercentage += tag.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage += tag.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage += tag.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage += tag.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
		private void RemoveCharacterTagBonuses(CharacterTag tag){
			_statsModifierPercentage.intPercentage -= tag.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage -= tag.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage -= tag.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage -= tag.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
		public bool HasTags(CHARACTER_TAG[] tagsToHave, bool mustHaveAll = false, bool includeParty = false){
			if(!includeParty){
				return DoesHaveTags (this, tagsToHave, mustHaveAll);
			}else{
				if(party != null){
					List<CHARACTER_TAG> tagsToHaveCopy = tagsToHave.ToList ();
					for (int i = 0; i < party.partyMembers.Count; i++) {
						for (int j = 0; j < party.partyMembers[i].tags.Count; j++) {
							for (int k = 0; k < tagsToHaveCopy.Count; k++) {
								if(party.partyMembers[i].tags[j].tagType == tagsToHaveCopy[k]) {
									tagsToHaveCopy.RemoveAt (k);
									break;
								}
							}
							if(tagsToHaveCopy.Count <= 0){
								return true;
							}
						}
					}
				}else{
					return DoesHaveTags (this, tagsToHave, mustHaveAll);
				}
			}
			return false;
		}
		private bool DoesHaveTags(Character currCharacter, CHARACTER_TAG[] tagsToHave, bool mustHaveAll = false){
			if(mustHaveAll){
				int tagsCount = 0;
				for (int i = 0; i < currCharacter.tags.Count; i++) {
					for (int j = 0; j < tagsToHave.Length; j++) {
						if(tagsToHave[j] == currCharacter.tags[i].tagType){
							tagsCount++;
							break;
						}
					}
					if(tagsCount >= tagsToHave.Length){
						return true;
					}
				}
			}else{
				for (int i = 0; i < currCharacter.tags.Count; i++) {
					for (int j = 0; j < tagsToHave.Length; j++) {
						if(tagsToHave[j] == currCharacter.tags[i].tagType){
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool HasTag(CHARACTER_TAG tag, bool includeParty = false) {
			if(!includeParty){
				for (int i = 0; i < _tags.Count; i++) {
					if(_tags[i].tagType == tag) {
						return true;
					}
				}
			}else{
				if(party != null){
					for (int i = 0; i < party.partyMembers.Count; i++) {
						for (int j = 0; j < party.partyMembers[i].tags.Count; j++) {
							if(party.partyMembers[i].tags[j].tagType == tag) {
								return true;
							}
						}
					}
				}else{
					for (int i = 0; i < _tags.Count; i++) {
						if(_tags[i].tagType == tag) {
							return true;
						}
					}
				}
			}

			return false;
		}
		public bool HasTag(string tag, bool includeParty = false) {
			if(!includeParty){
				for (int i = 0; i < _tags.Count; i++) {
					if(_tags[i].tagName == tag) {
						return true;
					}
				}
			}else{
				if(party != null){
					for (int i = 0; i < party.partyMembers.Count; i++) {
						for (int j = 0; j < party.partyMembers[i].tags.Count; j++) {
							if(party.partyMembers[i].tags[j].tagName == tag) {
								return true;
							}
						}
					}
				}else{
					for (int i = 0; i < _tags.Count; i++) {
						if(_tags[i].tagName == tag) {
							return true;
						}
					}
				}
			}

			return false;
		}
		public CharacterTag GetTag(CHARACTER_TAG tag, bool includeParty = false){
			if(!includeParty){
				for (int i = 0; i < _tags.Count; i++) {
					if(_tags[i].tagType == tag) {
						return _tags[i];
					}
				}
			}else{
				if(party != null){
					for (int i = 0; i < party.partyMembers.Count; i++) {
						for (int j = 0; j < party.partyMembers[i].tags.Count; j++) {
							if(party.partyMembers[i].tags[j].tagType == tag) {
								return party.partyMembers[i].tags[j];
							}
						}
					}
				}else{
					for (int i = 0; i < _tags.Count; i++) {
						if(_tags[i].tagType == tag) {
							return _tags[i];
						}
					}
				}
			}
			return null;
		}
		public CharacterTag GetTag(string tag, bool includeParty = false){
			if(!includeParty){
				for (int i = 0; i < _tags.Count; i++) {
					if(_tags[i].tagName == tag) {
						return _tags[i];
					}
				}
			}else{
				if(party != null){
					for (int i = 0; i < party.partyMembers.Count; i++) {
						for (int j = 0; j < party.partyMembers[i].tags.Count; j++) {
							if(party.partyMembers[i].tags[j].tagName == tag) {
								return party.partyMembers[i].tags[j];
							}
						}
					}
				}else{
					for (int i = 0; i < _tags.Count; i++) {
						if(_tags[i].tagName == tag) {
							return _tags[i];
						}
					}
				}
			}
			return null;
		}
		#endregion

        #region Faction
        public void SetFaction(Faction faction) {
			_faction = faction;
		}
		#endregion

		#region Party
        /*
         Create a new Party with this character as the leader.
             */
        public Party CreateNewParty() {
            Party newParty = new Party(this);
            return newParty;
        }
		public void SetParty(Party party) {
			_party = party;
		}
		#endregion

		#region Location
        public void SetSpecificLocation(ILocation specificLocation) {
            _specificLocation = specificLocation;
			if(_specificLocation != null){
				_currentRegion = _specificLocation.tileLocation.region;
			}
        }
		public bool IsCharacterInAdjacentRegionOfThis(Character targetCharacter){
			for (int i = 0; i < _currentRegion.adjacentRegionsViaMajorRoad.Count; i++) {
				if(targetCharacter.currentRegion.id == _currentRegion.adjacentRegionsViaMajorRoad[i].id){
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Quests
        /*
         Determine what action the character will do, and execute that action.
             */
		internal void DetermineAction() {
            //Set Task of character to do nothing for now
			//if(_role != null){
			//	if(_role.defaultRoleTask != null){
			//		_role.defaultRoleTask.OnChooseTask (this);
			//	}
			//}
   //         return;

//			if(isInCombat){
//				SetCurrentFunction (() => DetermineAction ());
//				return;
//			}
			if(_isFainted || _isPrisoner || _isDead || _isFollower){
				return;
			}
            if(_party != null) {
                //if the character is in a party, and is not the leader, do not decide any action
                if (!_party.IsCharacterLeaderOfParty(this)) {
                    return;
                }
            }
			if(_currentTask != null && !_currentTask.isDone){
				_currentTask.SetIsHalted (true);
			}
            if(nextTaskToDo != null) {
                //Force accept quest, if any
				nextTaskToDo.OnChooseTask(this);
//                nextTaskToDo.PerformTask();
                nextTaskToDo = null;
                return;
            }
			WeightedDictionary<CharacterTask> actionWeights = new WeightedDictionary<CharacterTask> ();
			if(_role != null){
				_role.AddTaskWeightsFromRole (actionWeights);
			}

			if (_role == null || (_role != null && !_role.cancelsAllOtherTasks)) {				
				for (int i = 0; i < _tags.Count; i++) {
					_tags [i].AddTaskWeightsFromTags (actionWeights);
				}
				if (currentQuest != null) {
					//Quest Tasks
					_questData.AddQuestTasksToWeightedDictionary(actionWeights);
				}
			}

            if (actionWeights.GetTotalOfWeights() > 0) {
                CharacterTask chosenTask = actionWeights.PickRandomElementGivenWeights();
                chosenTask.ResetTask();
                chosenTask.OnChooseTask(this);
            } else {
                actionWeights.LogDictionaryValues(this.raceSetting.race.ToString() + " weights!");
            }
//			chosenTask.PerformTask ();

//			WeightedDictionary<CharacterTask> actionWeights = _role.GetActionWeights();
//            AddActionWeightsFromTags(actionWeights); //Add weights from tags
//            if (actionWeights.GetTotalOfWeights () > 0) {
//				CharacterTask chosenAction = actionWeights.PickRandomElementGivenWeights();
//                if (chosenAction.taskType == TASK_TYPE.QUEST) {
//                    Debug.Log(this.name + " decides to " + ((OldQuest.Quest)chosenAction).questType.ToString() + " on " + Utilities.GetDateString(GameManager.Instance.Today()));
//                } else {
//                    Debug.Log(this.name + " decides to " + chosenAction.taskType.ToString() + " on " + Utilities.GetDateString(GameManager.Instance.Today()));
//                }
//                chosenAction.PerformTask(this);
//            } else {
//                throw new Exception(this.name + " could not decide action because weights are zero!");
//            }
		}
        /*
         Set a task that this character will accept next
             */
        internal void SetTaskToDoNext(CharacterTask taskToDo) {
            nextTaskToDo = taskToDo;
        }
//        private WeightedDictionary<CharacterTask> GetUnalignedActionWeights() {
//            WeightedDictionary<CharacterTask> actionWeights = new WeightedDictionary<CharacterTask>();
//
//            Rest restTask = new Rest(this);
//            actionWeights.AddElement(restTask, GetUnalignedRestWeight());
//
//            if(_lair != null) {
//                GoHome goHomeTask = new GoHome(this);
//                actionWeights.AddElement(goHomeTask, GetUnalignedGoHomeWeight());
//            }
//
//            DoNothing doNothingTask = new DoNothing(this);
//            actionWeights.AddElement(doNothingTask, GetUnalignedDoNothingWeight());
//
//            return actionWeights;
//        }
//        
//        private int GetUnalignedRestWeight() {
//            if (currentHP < maxHP) {
//                int percentMissing = (int)(100f - (remainingHP * 100));
//                if (percentMissing >= 50) {
//                    return 100; //+100 if HP is below 50%
//                } else {
//                    return 5 * percentMissing; //5 Weight per % of HP below max HP, 
//                }
//            }
//            return 0;
//        }
//        private int GetUnalignedDoNothingWeight() {
//            return 300;
//        }
//        private int GetUnalignedGoHomeWeight() {
//            return 50;
//        }
//
//        private void AddTaskWeightsFromQuest(WeightedDictionary<CharacterTask> tasks) {
//            if (_currentQuest != null) {
//                CharacterTask currentTaskForQuest = _currentQuest.GetCurrentTaskOfQuest();
//                tasks.AddElement(currentTaskForQuest, currentTaskForQuest.GetTaskWeight(this));
//            }
//        }
//
//        private WeightedDictionary<CharacterTask> GetTaskWeights() {
//            WeightedDictionary<CharacterTask> taskWeights = new WeightedDictionary<CharacterTask>();
//            if (_role != null) {
//                _role.AddTaskWeightsFromRole(taskWeights);
//            }
//            AddTaskWeightsFromTags(taskWeights);
//            AddTaskWeightsFromQuest(taskWeights);
//            return taskWeights;
//        }

        #endregion

        #region Tags
		public void AssignInitialTags(){
			int tagChance = UnityEngine.Random.Range (0, 100);
			CHARACTER_TAG[] initialTags = (CHARACTER_TAG[])System.Enum.GetValues (typeof(CHARACTER_TAG));
			for (int j = 0; j < initialTags.Length; j++) {
				CHARACTER_TAG tag = initialTags [j];
				if(tagChance < Utilities.GetTagWorldGenChance(tag)){
					AssignTag (tag);
				}
			}
		}
        #endregion

        #region HP
        public bool IsHealthFull() {
            return _currentHP >= _maxHP;
        }
		#endregion

		#region Avatar
		public void CreateNewAvatar() {
			//TODO: Only create one avatar per character, then enable disable it based on need, rather than destroying it then creating a new avatar when needed
            if(this._role != null) {
                if (this._role.roleType == CHARACTER_ROLE.COLONIST) {
                    //				GameObject avatarGO = (GameObject)GameObject.Instantiate (ObjectPoolManager.Instance.otherPrefabs [2], this.currLocation.transform.position, Quaternion.identity);
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("ColonistAvatar", this.currLocation.transform.position, Quaternion.identity);
                    ColonistAvatar avatar = avatarGO.GetComponent<ColonistAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                } else if (this._role.roleType == CHARACTER_ROLE.WARLORD) {
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("WarlordAvatar", this.currLocation.transform.position, Quaternion.identity);
                    WarlordAvatar avatar = avatarGO.GetComponent<WarlordAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                } else if (this._role.roleType == CHARACTER_ROLE.HERO) {
                    //				GameObject avatarGO = (GameObject)GameObject.Instantiate (ObjectPoolManager.Instance.otherPrefabs [2], this.currLocation.transform.position, Quaternion.identity);
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("HeroAvatar", this.currLocation.transform.position, Quaternion.identity);
                    HeroAvatar avatar = avatarGO.GetComponent<HeroAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                } else {
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterAvatar", this.currLocation.transform.position, Quaternion.identity);
                    CharacterAvatar avatar = avatarGO.GetComponent<CharacterAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                }
            } else {
                GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterAvatar", this.currLocation.transform.position, Quaternion.identity);
                CharacterAvatar avatar = avatarGO.GetComponent<CharacterAvatar>();
                if (party != null) {
                    avatar.Init(party);
                } else {
                    avatar.Init(this);
                }
            }
			
        }
		public void SetAvatar(CharacterAvatar avatar) {
			_avatar = avatar;
		}
		public void DestroyAvatar() {
			if(_avatar != null) {
				_avatar.InstantDestroyAvatar();
            }
        }
        public void GoToNearestNonHostileSettlement(Action onReachSettlement) {
//			if(isInCombat){
//				SetCurrentFunction (() => GoToNearestNonHostileSettlement (() => onReachSettlement()));
//				return;
//			}
            //check first if the character is already at a non hostile settlement
            if(this.currLocation.landmarkOnTile != null && this.currLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CITY
                && this.currLocation.landmarkOnTile.owner != null) {
                if(this.faction.id != this.currLocation.landmarkOnTile.owner.id) {
                    if(FactionManager.Instance.GetRelationshipBetween(this.faction, this.currLocation.landmarkOnTile.owner).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                        onReachSettlement();
                        return;
                    }
                } else {
                    onReachSettlement();
                    return;
                }
            }
            //character is not on a non hostile settlement
            List<Settlement> allSettlements = new List<Settlement>();
            for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) { //Get all the occupied settlements
                Tribe currTribe = FactionManager.Instance.allTribes[i];
                if(this.faction.id == currTribe.id ||
                    FactionManager.Instance.GetRelationshipBetween(this.faction, currTribe).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                    allSettlements.AddRange(currTribe.settlements);
                }
            }
            allSettlements = allSettlements.OrderBy(x => Vector2.Distance(this.currLocation.transform.position, x.tileLocation.transform.position)).ToList();
            if(_avatar == null) {
                CreateNewAvatar();
            }
			_avatar.SetTarget(allSettlements[0].tileLocation);
            _avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => onReachSettlement());
        }
        /*
         This is the default action to be done when a 
         character returns to a non hostile settlement after a quest.
             */
        internal void OnReachNonHostileSettlementAfterQuest() {
            //if (_party != null) {
            //    _party.CheckLeavePartyAfterQuest();
            //}
            //_currLocation.AddCharacterOnTile(this);
            DestroyAvatar();
            DetermineAction();
        }
		internal void GoToLocation(ILocation targetLocation, PATHFINDING_MODE pathfindingMode, Action doneAction = null){
            if (specificLocation == null) {
                throw new Exception("Specific location is null!");
            }
            if (targetLocation == null) {
                throw new Exception("target location is null!");
            }
			if (specificLocation == targetLocation) {
				//action doer is already at the target location
				if(doneAction != null){
					doneAction ();
				}
			} else {
				if (_avatar == null) {
					//Instantiate a new character avatar
					CreateNewAvatar();
				}
				_avatar.SetTarget(targetLocation);
				if(doneAction == null){
					_avatar.StartPath(pathfindingMode);
				}else{
					_avatar.StartPath(pathfindingMode, () => doneAction());
				}
			}
		}
		#endregion

		#region Task Management
        public void SetCurrentQuest(Quest currentQuest) {
            _questData.SetActiveQuest(currentQuest);
            UIManager.Instance.UpdateCharacterInfo();
        }
		public void AddNewQuest(OldQuest.Quest quest) {
			if (!_activeQuests.Contains(quest)) {
				_activeQuests.Add(quest);
			}
		}
		public void RemoveQuest(OldQuest.Quest quest) {
			_activeQuests.Remove(quest);
		}
		public void SetCurrentTask(CharacterTask currentTask) {
            if (_currentTask == null || _currentTask != currentTask) {
                if (onTaskChanged != null) {
                    onTaskChanged();
                }
            }
            _currentTask = currentTask;
        }
        public void AddActionOnTaskChanged(OnTaskChanged onTaskChangeAction) {
            onTaskChanged += onTaskChangeAction;
        }
        public void RemoveActionOnTaskChanged(OnTaskChanged onTaskChangeAction) {
            onTaskChanged -= onTaskChangeAction;
        }
        public void ResetOnTaskChangedActions() {
            onTaskChanged = null;
        }
        public List<OldQuest.Quest> GetQuestsOfType(QUEST_TYPE questType) {
			List<OldQuest.Quest> quests = new List<OldQuest.Quest>();
			for (int i = 0; i < _activeQuests.Count; i++) {
				OldQuest.Quest currQuest = _activeQuests[i];
				if(currQuest.questType == questType) {
					quests.Add(currQuest);
				}
			}
			return quests;
		}
		public List<CharacterTask> GetAllPossibleTasks(ILocation location){
			List<CharacterTask> possibleTasks = new List<CharacterTask> ();
            //Role Tasks
			if(_role != null){
				for (int i = 0; i < _role.roleTasks.Count; i++) {
					CharacterTask currentTask = _role.roleTasks [i];
					if(!currentTask.forGameOnly && currentTask.CanBeDone(this, location)){
						possibleTasks.Add (currentTask);
					}
				}
			}
            //Tag tasks
			if (_role == null || (_role != null && !_role.cancelsAllOtherTasks)) {
				for (int i = 0; i < _tags.Count; i++) {
					for (int j = 0; j < _tags [i].tagTasks.Count; j++) {
						CharacterTask currentTask = _tags [i].tagTasks [j];
						if (!currentTask.forGameOnly && currentTask.CanBeDone (this, location)) {
							possibleTasks.Add (currentTask);
						}
					}
				}
				//Quest Tasks
				if (currentQuest != null) {
					for (int i = 0; i < _questData.tasks.Count; i++) {
						CharacterTask currentTask = _questData.tasks [i];
						if (!currentTask.forGameOnly && !currentTask.isDone && currentTask.CanBeDone (this, location)) {
							possibleTasks.Add (currentTask);
						}
					}
				}
			}

			return possibleTasks;
		}
        #endregion

        #region Utilities
		public void SetName(string newName){
			_name = newName;
		}
        public Character GetFollowerByID(int id) {
            if (party != null) {
                for (int i = 0; i < party.partyMembers.Count; i++) {
                    Character currChar = party.partyMembers[i];
                    if (currChar.id == id) {
                        return currChar;
                    }
                }
            }
            return null;
        }
        public void SetFollowerState(bool state) {
            _isFollower = state;
        }
		public void SetHome(BaseLandmark newHome) {
            this._home = newHome;
        }
        public void SetLair(BaseLandmark newLair) {
            this._lair = newLair;
        }
		public void SetDoesNotTakePrisoners(bool state) {
			this._doesNotTakePrisoners = state;
		}
		public void SetCannotBeTakenAsPrisoner(bool state) {
			this._cannotBeTakenAsPrisoner = state;
		}
        public bool HasPathToParty(Party partyToJoin) {
            return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, _faction) != null;
        }
        public Settlement GetNearestNonHostileSettlement() {
			if(faction != null){
				List<Faction> nonHostileFactions = faction.GetMajorFactionsWithRelationshipStatus
					(new List<RELATIONSHIP_STATUS>() { RELATIONSHIP_STATUS.FRIENDLY, RELATIONSHIP_STATUS.NEUTRAL });

				List<Settlement> settlements = new List<Settlement>();
				nonHostileFactions.ForEach(x => settlements.AddRange(x.settlements));
				settlements.AddRange(_faction.settlements); //Add the settlements of the faction that this character belongs to

				settlements.OrderByDescending(x => currLocation.GetDistanceTo(x.tileLocation));

				return settlements.First();
			}
			return null;
        }
        public Settlement GetNearestSettlementFromFaction() {
            if(this.faction != null) {
                List<Settlement> factionSettlements = new List<Settlement>(faction.settlements);
                if (factionSettlements.Count > 0) {
					factionSettlements.OrderBy(x => this.currLocation.GetDistanceTo(x.tileLocation)).ToList();
                    return factionSettlements[0];
                }
            }
            return null;
        }
        public BaseLandmark GetNearestLandmarkWithoutHostiles() {
            Region currRegionLocation = specificLocation.tileLocation.region;
            List<BaseLandmark> elligibleLandmarks = new List<BaseLandmark>(currRegionLocation.allLandmarks);
            //elligibleLandmarks.Add(currRegionLocation.mainLandmark);
            //elligibleLandmarks.AddRange(currRegionLocation.landmarks);
            if (specificLocation is BaseLandmark) {
                elligibleLandmarks.Remove(specificLocation as BaseLandmark);
            }
            Dictionary<BaseLandmark, List<HexTile>> landmarksWithoutHostiles = new Dictionary<BaseLandmark, List<HexTile>>();
            Dictionary<BaseLandmark, List<HexTile>> landmarksWithHostiles = new Dictionary<BaseLandmark, List<HexTile>>();
            for (int i = 0; i < elligibleLandmarks.Count; i++) {
                BaseLandmark currLandmark = elligibleLandmarks[i];
				List<HexTile> path = PathGenerator.Instance.GetPath(specificLocation.tileLocation, currLandmark.tileLocation, PATHFINDING_MODE.USE_ROADS);
                if(path != null) {
                    //check for hostiles
                    if (!currLandmark.HasHostilitiesWith(this.faction)) {
                        if (landmarksWithoutHostiles.ContainsKey(currLandmark)) {
                            landmarksWithoutHostiles.Add(currLandmark, path);
                        }
                    } else {
                        if (landmarksWithHostiles.ContainsKey(currLandmark)) {
                            landmarksWithHostiles.Add(currLandmark, path);
                        }
                    }
                }
            }

            if (landmarksWithoutHostiles.Count > 0) {
                landmarksWithoutHostiles.OrderBy(x => x.Value.Count);
                return landmarksWithoutHostiles.Keys.First();
            } else {
                if (landmarksWithHostiles.Count > 0) {
                    landmarksWithHostiles.OrderBy(x => x.Value.Count);
                    return landmarksWithHostiles.Keys.First();
                }
            }

            return null;
        }
        public bool HasRelevanceToQuest(BaseLandmark landmark) {
            if (currentQuest != null) {
                for (int i = 0; i < questData.tasks.Count; i++) {
                    if (questData.tasks[i].targetLocation == landmark) {
                        return true;
                    }
                }
            }
            return false;
        }
        public void CenterOnCharacter() {
            CameraMove.Instance.CenterCameraOn(this.currLocation.gameObject);
        }

		//Death of this character if he/she is in the region specified
		private void RegionDeath(Region region){
			if(currentRegion.id == region.id){
				Death ();
			}
		}
        #endregion

        #region Relationships
        public void AddNewRelationship(Character relWith, Relationship relationship) {
            if (!_relationships.ContainsKey(relWith)) {
                _relationships.Add(relWith, relationship);
            } else {
                throw new Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
            }
        }
        public void RemoveRelationshipWith(Character relWith) {
            if (_relationships.ContainsKey(relWith)) {
                _relationships.Remove(relWith);
            }
        }
        public Relationship GetRelationshipWith(Character character) {
            if (_relationships.ContainsKey(character)) {
                return _relationships[character];
            }
            return null;
        }
        #endregion

        #region History
        internal void AddHistory(Log log) {
            ////check if character already has log
            //for (int i = 0; i < _history.Count; i++) {
            //    Log currLog = _history[i];
            //    if (currLog.key == log.key) {
            //        bool areIdentical = true;
            //        for (int j = 0; j < currLog.fillers.Count; j++) {
            //            object currFiller = currLog.fillers[j];
            //            object fillerFromLog = log.fillers.ElementAtOrDefault(j);
            //            if (fillerFromLog != currFiller) {
            //                areIdentical = false;
            //            }
            //        }
            //        if (areIdentical) {
            //            throw new Exception("IDENTICAL LOGS!");
            //        }
            //    }
            //}

            _history.Add(log);
            if (this._history.Count > 20) {
                this._history.RemoveAt(0);
            }
            //add logs to followers as well
            for (int i = 0; i < followers.Count; i++) {
                followers[i].AddHistory(log);
            }
        }
        //internal void AddHistory(string text, object obj = null){
        //	GameDate today = GameManager.Instance.Today ();
        //	string date = "[" + ((MONTH)today.month).ToString() + " " + today.day + ", " + today.year + "]";
        //	if(obj != null){
        //		if(obj is CombatPrototype){
        //			CombatPrototype combat = (CombatPrototype)obj;
        //			if(this.combatHistory.Count > 20){
        //				this.combatHistory.Remove (0);
        //			}
        //			_combatHistoryID += 1;
        //			combatHistory.Add (_combatHistoryID, combat);
        //			string combatText = "[url=" + _combatHistoryID.ToString() + "_combat]" + text + "[/url]";
        //			text = combatText;
        //		}
        //	}
        //	this._history.Insert (0, date + " " + text);
        //	if(this._history.Count > 20){
        //		this._history.RemoveAt (this._history.Count - 1);
        //	}
        //}
        #endregion

        #region Prisoner
        internal void SetPrisoner(bool state, object prisonerOf){
			_isPrisoner = state;
			_isPrisonerOf = prisonerOf;
			if(state){
				if(this.specificLocation != null){
					this.specificLocation.RemoveCharacterFromLocation (this);
				}
				string wardenName = string.Empty;
				if(_isPrisonerOf is Party){
					wardenName = ((Party)_isPrisonerOf).name;
				}else if(_isPrisonerOf is Character){
					wardenName = ((Character)_isPrisonerOf).name;
				}else if(_isPrisonerOf is BaseLandmark){
					wardenName = ((BaseLandmark)_isPrisonerOf).landmarkName;
				}
                Log becomePrisonerLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "became_prisoner");
                becomePrisonerLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                becomePrisonerLog.AddToFillers(_isPrisonerOf, wardenName, LOG_IDENTIFIER.TARGET_CHARACTER);
                AddHistory(becomePrisonerLog);

                if (_isPrisonerOf is Character) {
                    ((Character)_isPrisonerOf).AddHistory(becomePrisonerLog);
                } else if (_isPrisonerOf is BaseLandmark) {
                    ((BaseLandmark)_isPrisonerOf).AddHistory(becomePrisonerLog);
                }

                Unfaint ();
			}
		}
		internal void AddPrisoner(Character character){
			if (this._party != null) {
				this._party.AddPrisoner(character);
			}else{
				character.SetPrisoner (true, this);
				_prisoners.Add (character);
			}
		}
		internal void RemovePrisoner(Character character){
			_prisoners.Remove (character);
		}
		internal void ReleasePrisoner(){
			string wardenName = string.Empty;
            ILocation location = null;
            if (_isPrisonerOf is Party) {
                Party prisonerOf = _isPrisonerOf as Party;
                wardenName = prisonerOf.name;
                prisonerOf.RemovePrisoner(this);
                location = prisonerOf.specificLocation;
            } else if (_isPrisonerOf is Character) {
                Character prisonerOf = _isPrisonerOf as Character;
                wardenName = prisonerOf.name;
                prisonerOf.RemovePrisoner(this);
                location = prisonerOf.specificLocation;
            } else if (_isPrisonerOf is BaseLandmark) {
                BaseLandmark prisonerOf = _isPrisonerOf as BaseLandmark;
                wardenName = prisonerOf.landmarkName;
                prisonerOf.RemovePrisoner(this);
                location = prisonerOf;
            }
			this._specificLocation = location;

            SetPrisoner(false, null);

            Log releasePrisonerLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "release_prisoner");
            releasePrisonerLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

            AddHistory(releasePrisonerLog);
            if (this.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                BaseLandmark landmark = (BaseLandmark)this.specificLocation;
                landmark.AddHistory(releasePrisonerLog);
            }

            //When this character is released from imprisonment
            //Check if this character is a follower
            if (this.isFollower) {
                //if this character is
                if (this.faction != null) {
                    //Set this character as a civilian of the nearest settlement of his/her faction
                    Settlement settlement = GetNearestSettlementFromFaction();
                    settlement.AdjustCivilians(this.raceSetting.race, 1);
                }
            } else {
                //if not, make this character decide for itself again
                location.AddCharacterToLocation(this);
                DetermineAction();
            }
		}
		internal void TransferPrisoner(object newPrisonerOf){
			//Remove from previous prison
			if(_isPrisonerOf is Party){
				((Party)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is Character){
				((Character)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is BaseLandmark){
				((BaseLandmark)_isPrisonerOf).RemovePrisoner (this);
			}

			//Add prisoner to new prison
			if(newPrisonerOf is Party){
				((Party)newPrisonerOf).AddPrisoner (this);
			}else if(newPrisonerOf is Character){
				((Character)newPrisonerOf).AddPrisoner (this);
			}else if(newPrisonerOf is BaseLandmark){
				((BaseLandmark)newPrisonerOf).AddPrisoner (this);
			}
		}
		private void PrisonerDeath(){
			if(_isPrisonerOf is Party){
				((Party)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is Character){
				((Character)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is BaseLandmark){
				((BaseLandmark)_isPrisonerOf).RemovePrisoner (this);
			}
			SetPrisoner (false, null);
		}
		public void ConvertToFaction(){
            Faction previousFaction = this.faction;
			BaseLandmark prison = (BaseLandmark)_isPrisonerOf;
			Faction newFaction = prison.owner;
			SetFaction (newFaction);
			SetHome (prison);
			prison.AddCharacterToLocation(this);
			prison.AddCharacterHomeOnLandmark(this);
			ChangeRole ();
			prison.owner.AddNewCharacter(this);

            // when a character from another Faction switches to another Faction, Landmark Information will also be transferred to his new Faction.
            if (previousFaction != null) {
                for (int i = 0; i < previousFaction.landmarkInfo.Count; i++) {
                    BaseLandmark currLandmark = previousFaction.landmarkInfo[i];
                    newFaction.AddLandmarkInfo(currLandmark);
                }
            }
		}
		#endregion

		#region ICombatInitializer
		//public bool InitializeCombat(){
		//	if(_role != null && _role.roleType == CHARACTER_ROLE.WARLORD){
		//		//Start Combat with hostile or unaligned
		//		ICombatInitializer enemy = this.specificLocation.GetCombatEnemy (this);
		//		if(enemy != null){
		//			CombatPrototype combat = new CombatPrototype (this, enemy, this.specificLocation);
		//			combat.AddCharacters (SIDES.A, new List<Character>(){this});
		//			if(enemy is Party){
		//				combat.AddCharacters (SIDES.B, ((Party)enemy).partyMembers);
		//			}else{
		//				combat.AddCharacters (SIDES.B, new List<Character>(){((Character)enemy)});
		//			}
		//			this.specificLocation.SetCurrentCombat (combat);
		//			CombatThreadPool.Instance.AddToThreadPool (combat);
		//			return true;
		//		}
		//		return false;
		//	}else{
		//		return false;
		//	}
		//}
		public bool IsHostileWith(ICombatInitializer combatInitializer){
            if (this.faction == null) {
                return true; //this character has no faction
            }
            //Check here if the combatInitializer is hostile with this character, if yes, return true
            Faction factionOfEnemy = null;
            if(combatInitializer is Character) {
                factionOfEnemy = (combatInitializer as Character).faction;
            }else if(combatInitializer is Party) {
                factionOfEnemy = (combatInitializer as Party).faction;
            }
            if(factionOfEnemy != null) {
                if(factionOfEnemy.id == this.faction.id) {
                    return false; //characters are of same faction
                }
                FactionRelationship rel = this.faction.GetRelationshipWith(factionOfEnemy);
                if(rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                    return true; //factions of combatants are hostile
                }
                return false;
            } else {
                return true; //enemy has no faction
            }
			
		}
		public void ReturnCombatResults(CombatPrototype combat){
            this.SetIsInCombat(false);
			if (this.isDefeated) {
				//this character was defeated
				if(_currentTask != null && faction != null) {
					_currentTask.EndTask(TASK_STATUS.CANCEL);
				}
                if (!isDead && !isPrisoner) {
                    BaseLandmark targetLocation = GetNearestLandmarkWithoutHostiles();
                    if (targetLocation == null) {
                        throw new Exception(this.name + " could not find a non hostile location to run to!");
                    } else {
                        GoToLocation(targetLocation, PATHFINDING_MODE.USE_ROADS, () => DetermineAction());
                    }
                }
            } else{
                //this character won the combat, continue his/her current action if any
                if (currentFunction != null) {
                    currentFunction();
                    SetCurrentFunction(null);
                }
     //           else {
					//DetermineAction();
     //           }
			}
        }
		public void SetIsDefeated(bool state){
			_isDefeated = state;
		}
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
			if(_party != null){
				_party.SetCurrentFunction (() => function ());
			}else{
				_currentFunction = function;
			}
		}
		#endregion

		#region Landmarks
		public void AddExploredLandmark(BaseLandmark landmark){
			_exploredLandmarks.Add(landmark); //did not add checking if landmark is already in list, since I want to allow duplicates
            //schedule removal of landmark after 6 months
            GameDate expiration = GameManager.Instance.Today();
            expiration.AddMonths(6);
            SchedulingManager.Instance.AddEntry(expiration, () => RemoveLandmarkAsExplored(landmark));
		}
        private void RemoveLandmarkAsExplored(BaseLandmark landmark) {
            _exploredLandmarks.Remove(landmark);
        }
		#endregion

		#region Followers
		public void AddFollower(Character character){
			if(!_followers.Contains(character)){
				_followers.Add (character);
				character._isFollowerOf = this;
				character.SetFollowerState (true);
			}
		}
		public void RemoveFollower(Character character){
			if(_followers.Remove(character)){
				character._isFollowerOf = null;
				character.SetFollowerState (false);
			}
		}
		#endregion

		#region Psytoxin
		private void RegionPsytoxin(List<Region> regions){
			for (int i = 0; i < regions.Count; i++) {
				if(currentRegion.id == regions[i].id){
					InfectedByPsytoxin ();
					break;
				}
			}
		}
		private void InfectedByPsytoxin(){
			if(HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
				return;	
			}
			ModeratePsytoxin modPsytoxin = (ModeratePsytoxin)GetTag (CHARACTER_TAG.MODERATE_PSYTOXIN);
			if(modPsytoxin != null){
				modPsytoxin.TriggerWorsenCase ();
			}else{
				MildPsytoxin mildPsytoxin = (MildPsytoxin)GetTag (CHARACTER_TAG.MILD_PSYTOXIN);
				if(mildPsytoxin != null){
					mildPsytoxin.TriggerWorsenCase ();
				}else{
					AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);
				}
			}
		}
		#endregion

		#region Traces
		public void LeaveTraceOnLandmark(){
			ILocation location = specificLocation;
			if(location != null && location is BaseLandmark){
				BaseLandmark landmark = (BaseLandmark)location;
				int chance = UnityEngine.Random.Range (0, 100);
				int value = GetLeaveTraceChance ();
				if(chance < value){
                    landmark.AddTrace(this);
                }
			}
		}
		private int GetLeaveTraceChance(){
			STANCE stance = GetCurrentStance ();
			switch(stance){
			case STANCE.COMBAT:
				return 100;
			case STANCE.NEUTRAL:
				return 50;
			case STANCE.STEALTHY:
				return 25;
			}
			return 0;
		}
		public void AddTraceInfo(Character character, string identifier){
			if(_traceInfo.ContainsKey(character)){
				if(!_traceInfo [character].Contains(identifier)){
					_traceInfo [character].Add (identifier);
					Debug.Log (this.name +  " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
				}
			}else{
				_traceInfo.Add (character, new List<string> (){ identifier });
				Debug.Log (this.name +  " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
			}
		}
		public Character GetCharacterFromTraceInfo(string info){
			foreach (Character character in _traceInfo.Keys) {
				if(_traceInfo[character].Contains(info)){
					return character;
				}
			}
			return null;
		}
		#endregion
    }
}
