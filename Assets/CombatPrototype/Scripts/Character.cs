﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class Character : ICharacter, ILeader, IInteractable, IPointOfInterest {
    public delegate void OnCharacterDeath();
    public OnCharacterDeath onCharacterDeath;

    public delegate void DailyAction();
    public DailyAction onDailyAction;

    protected string _name;
    protected string _firstName;
    protected string _characterColorCode;
    protected int _id;
    protected int _gold;
    protected int _currentInteractionTick;
    protected int _lastLevelUpDay;
    protected int _doNotDisturb;
    protected float _actRate;
    protected bool _isDead;
    protected bool _isFainted;
    protected bool _isInCombat;
    protected bool _isBeingInspected;
    protected bool _hasBeenInspected;
    protected bool _alreadyTargetedByGrudge;
    protected GENDER _gender;
    protected MODE _currentMode;
    protected CharacterClass _characterClass;
    protected RaceSetting _raceSetting;
    protected CharacterRole _role;
    protected Job _job;
    protected Faction _faction;
    protected CharacterParty _ownParty;
    protected CharacterParty _currentParty;
    protected Region _currentRegion;
    protected Weapon _equippedWeapon;
    protected Armor _equippedArmor;
    protected Item _equippedAccessory;
    protected Item _equippedConsumable;
    protected CharacterBattleTracker _battleTracker;
    protected CharacterBattleOnlyTracker _battleOnlyTracker;
    protected PortraitSettings _portraitSettings;
    protected Color _characterColor;
    protected Minion _minion;
    protected Interaction _forcedInteraction;
    protected CombatCharacter _currentCombatCharacter;
    protected PairCombatStats[] _pairCombatStats;
    protected List<Item> _inventory;
    protected List<Skill> _skills;
    protected List<Log> _history;
    protected List<Trait> _traits;
    protected List<Interaction> _currentInteractions;
    protected Dictionary<ELEMENT, float> _elementalWeaknesses;
    protected Dictionary<ELEMENT, float> _elementalResistances;
    protected Dictionary<Character, List<string>> _traceInfo;
    protected PlayerCharacterItem _playerCharacterItem;

    //Stats
    protected SIDES _currentSide;
    protected int _currentHP;
    protected int _currentRow;
    protected int _level;
    protected int _experience;
    protected int _maxExperience;
    protected int _sp;
    protected int _maxSP;
    protected int _attackPowerMod;
    protected int _speedMod;
    protected int _maxHPMod;
    protected int _attackPowerPercentMod;
    protected int _speedPercentMod;
    protected int _maxHPPercentMod;
    protected int _combatBaseAttack;
    protected int _combatBaseSpeed;
    protected int _combatBaseHP;
    protected int _combatAttackFlat;
    protected int _combatAttackMultiplier;
    protected int _combatSpeedFlat;
    protected int _combatSpeedMultiplier;
    protected int _combatHPFlat;
    protected int _combatHPMultiplier;
    protected int _combatPowerFlat;
    protected int _combatPowerMultiplier;

    public Area homeArea { get; protected set; }
    public Dwelling homeStructure { get; protected set; }
    public LocationStructure currentStructure { get; private set; } //what structure is this character currently in.
    public Area defendingArea { get; private set; }
    public MORALITY morality { get; private set; }
    public CharacterToken characterToken { get; private set; }
    public WeightedDictionary<INTERACTION_TYPE> interactionWeights { get; private set; }
    public SpecialToken tokenInInventory { get; private set; }
    public Dictionary<Character, List<RelationshipTrait>> relationships { get; private set; }

    private Dictionary<STAT, float> _buffs;

    public Dictionary<int, Combat> combatHistory;

    public Color skinColor { get; private set; }
    public Color hairColor { get; private set; }

    public float hSkinColor { get; private set; }
    public float hHairColor { get; private set; }

    #region getters / setters
    public string firstName {
        get { return _firstName; }
    }
    public virtual string name {
        get {
            //if(_minion != null) {
            //    return _minion.name;
            //}
            return _firstName;
        }
    }
    public string coloredName {
        get { return "<color=#" + this._characterColorCode + ">" + name + "</color>"; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + name + "</link>"; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + "<color=#" + this._characterColorCode + ">" + name + "</color></link>"; }
    }
    public string raceClassName {
        get {
            if (Utilities.IsRaceBeast(race)) {
                return Utilities.NormalizeString(race.ToString());
            }
            return Utilities.GetNormalizedRaceAdjective(race) + " " + characterClass.className;
        }
    }
    public int id {
        get { return _id; }
    }
    public GENDER gender {
        get { return _gender; }
    }
    public MODE currentMode {
        get { return _currentMode; }
    }
    public RACE race {
        get { return _raceSetting.race; }
    }
    public CharacterClass characterClass {
        get { return this._characterClass; }
    }
    public RaceSetting raceSetting {
        get { return _raceSetting; }
    }
    public CharacterRole role {
        get { return _role; }
    }
    public Job job {
        get { return _job; }
    }
    public Faction faction {
        get { return _faction; }
    }
    public virtual Party ownParty {
        get { return _ownParty; }
    }
    public CharacterParty party {
        get { return _ownParty; }
    }
    public virtual Party currentParty {
        get { return _currentParty; }
    }
    public HexTile currLocation {
        get { return (_currentParty.specificLocation != null ? _currentParty.specificLocation.coreTile : null); }
    }
    public Area specificLocation {
        get { return _currentParty.specificLocation; }
    }
    public List<Item> inventory {
        get { return this._inventory; }
    }
    public List<Skill> skills {
        get { return this._skills; }
    }
    public int currentRow {
        get { return this._currentRow; }
    }
    public SIDES currentSide {
        get { return this._currentSide; }
    }
    public bool isDead {
        get { return this._isDead; }
    }
    public bool isFainted {
        get { return this._isFainted; }
    }
    public Color characterColor {
        get { return _characterColor; }
    }
    public string characterColorCode {
        get { return _characterColorCode; }
    }
    public List<Log> history {
        get { return this._history; }
    }
    public int gold {
        get { return _gold; }
    }
    public bool isInCombat {
        get {
            return _isInCombat;
        }
    }
    public bool isFactionless { //is the character part of the neutral faction? or no faction?
        get {
            if (FactionManager.Instance.neutralFaction == null) {
                return faction == null;
            } else {
                if (faction == null || FactionManager.Instance.neutralFaction.id == faction.id) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }
    public Dictionary<Character, List<string>> traceInfo {
        get { return _traceInfo; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    public int level {
        get { return _level; }
    }
    public int currentSP {
        get { return _sp; }
    }
    public int maxSP {
        get { return _maxSP; }
    }
    public int experience {
        get { return _experience; }
    }
    public int maxExperience {
        get { return _maxExperience; }
    }
    public int speed {
        get {
            int total = (int) ((_characterClass.baseSpeed + _speedMod) * (1f + ((_raceSetting.speedModifier + _speedPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int attackPower {
        get {
            int total = (int) ((_characterClass.baseAttackPower + _attackPowerMod) * (1f + ((_raceSetting.attackPowerModifier + _attackPowerPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int maxHP {
        get {
            int total = (int) ((_characterClass.baseHP + _maxHPMod) * (1f + ((_raceSetting.hpModifier + _maxHPPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int combatBaseAttack {
        get { return _combatBaseAttack; }
        set { _combatBaseAttack = value; }
    }
    public int combatBaseSpeed {
        get { return _combatBaseSpeed; }
        set { _combatBaseSpeed = value; }
    }
    public int combatBaseHP {
        get { return _combatBaseHP; }
        set { _combatBaseHP = value; }
    }
    public int combatAttackFlat {
        get { return _combatAttackFlat; }
        set { _combatAttackFlat = value; }
    }
    public int combatAttackMultiplier {
        get { return _combatAttackMultiplier; }
        set { _combatAttackMultiplier = value; }
    }
    public int combatSpeedFlat {
        get { return _combatSpeedFlat; }
        set { _combatSpeedFlat = value; }
    }
    public int combatSpeedMultiplier {
        get { return _combatSpeedMultiplier; }
        set { _combatSpeedMultiplier = value; }
    }
    public int combatHPFlat {
        get { return _combatHPFlat; }
        set { _combatHPFlat = value; }
    }
    public int combatHPMultiplier {
        get { return _combatHPMultiplier; }
        set { _combatHPMultiplier = value; }
    }
    public int combatPowerFlat {
        get { return _combatPowerFlat; }
        set { _combatPowerFlat = value; }
    }
    public int combatPowerMultiplier {
        get { return _combatPowerMultiplier; }
        set { _combatPowerMultiplier = value; }
    }
    public int currentHP {
        get { return this._currentHP; }
    }
    public PairCombatStats[] pairCombatStats {
        get { return _pairCombatStats; }
        set { _pairCombatStats = value; }
    }
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    public float actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public Weapon equippedWeapon {
        get { return _equippedWeapon; }
    }
    public Armor equippedArmor {
        get { return _equippedArmor; }
    }
    public Item equippedAccessory {
        get { return _equippedAccessory; }
    }
    public Item equippedConsumable {
        get { return _equippedConsumable; }
    }
    public CharacterBattleTracker battleTracker {
        get { return _battleTracker; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public float computedPower {
        get { return GetComputedPower(); }
    }
    public ICHARACTER_TYPE icharacterType {
        get { return ICHARACTER_TYPE.CHARACTER; }
    }
    public Minion minion {
        get { return _minion; }
    }
    public int doNotDisturb {
        get { return _doNotDisturb; }
    }
    public bool isBeingInspected {
        get { return _isBeingInspected; }
    }
    public bool hasBeenInspected {
        get { return _hasBeenInspected; }
    }
    public bool alreadyTargetedByGrudge {
        get { return _alreadyTargetedByGrudge; }
    }
    public bool isLeader {
        get { return job.jobType == JOB.LEADER; }
    }
    public QUEST_GIVER_TYPE questGiverType {
        get { return QUEST_GIVER_TYPE.CHARACTER; }
    }
    public bool isDefender {
        get { return defendingArea != null; }
    }
    public List<Trait> traits {
        get { return _traits; }
    }
    public List<Interaction> currentInteractions {
        get { return _currentInteractions; }
    }
    public Dictionary<STAT, float> buffs {
        get { return _buffs; }
    }
    public PlayerCharacterItem playerCharacterItem {
        get { return _playerCharacterItem; }
    }
    public Interaction forcedInteraction {
        get { return _forcedInteraction; }
    }
    public int currentInteractionTick {
        get { return _currentInteractionTick; }
    }
    public bool isHoldingItem {
        get { return tokenInInventory != null; }
    }
    public CombatCharacter currentCombatCharacter {
        get { return _currentCombatCharacter; }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.CHARACTER; }
    }
    #endregion

    public Character(string className, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        //_characterClass = CharacterManager.Instance.classesDictionary[className].CreateNewCopy();
        _raceSetting = RaceManager.Instance.racesDictionary[race.ToString()].CreateNewCopy();
        if (CharacterManager.Instance.classesDictionary.ContainsKey(className)) {
            AssignClass(CharacterManager.Instance.classesDictionary[className]);
        } else {
            throw new Exception("There is no class named " + className + " but it is being assigned to " + this.name);
        }
        _gender = gender;
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        if (this is CharacterArmyUnit) {
            _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(RACE.HUMANS, GENDER.MALE);
        } else {
            _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        }
        if (_characterClass.roleType != CHARACTER_ROLE.NONE) {
            AssignRole(_characterClass.roleType);
        }
        AssignRandomJob();
        SetMorality(MORALITY.GOOD);
        //_skills = GetGeneralSkills();

        //_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        //ConstructBodyPartDict(_raceSetting.bodyParts);

        //AllocateStatPoints(10);
        SetTraitsFromRace();
        ResetToFullHP();
        //CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(className);
        //if(setup != null) {
        //    GenerateSetupAttributes(setup);
        //    if(setup.optionalRole != CHARACTER_ROLE.NONE) {
        //        AssignRole(setup.optionalRole);
        //    }
        //}
    }
    public Character(CharacterSaveData data) : this() {
        _id = Utilities.SetID(this, data.id);
        //_characterClass = CharacterManager.Instance.classesDictionary[data.className].CreateNewCopy();
        _raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()].CreateNewCopy();
        AssignClass(CharacterManager.Instance.classesDictionary[data.className]);
        _gender = data.gender;
        SetName(data.name);
        //LoadRelationships(data.relationshipsData);
        _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        if (_characterClass.roleType != CHARACTER_ROLE.NONE) {
            AssignRole(_characterClass.roleType);
        }
        AssignRandomJob();
        SetMorality(data.morality);

        //_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        //ConstructBodyPartDict(_raceSetting.bodyParts);
        //_skills = GetGeneralSkills();
        //_skills = new List<Skill>();
        //_skills.Add(_characterClass.skill);
        //_skills.AddRange (GetBodyPartSkills ());
        //GenerateSetupTags(baseSetup);

        //AllocateStatPoints(10);
        //EquipItemsByClass();
        //SetTraitsFromClass();
        SetTraitsFromRace();
        //EquipPreEquippedItems(baseSetup);
        //CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(data.className);
        //if (setup != null) {
        //    GenerateSetupAttributes(setup);
        //    //if (setup.optionalRole != CHARACTER_ROLE.NONE) {
        //    //    AssignRole(setup.optionalRole);
        //    //}
        //}
        ResetToFullHP();
        //DetermineAllowedMiscActions();
    }
    public Character() {
        SetIsDead(false);
        _isFainted = false;
        //_isDefeated = false;
        //_isIdle = false;
        _traceInfo = new Dictionary<Character, List<string>>();
        _history = new List<Log>();
        _traits = new List<Trait>();


        //RPG
        _level = 1;
        _experience = 0;
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        _battleTracker = new CharacterBattleTracker();
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        //_equippedItems = new List<Item>();
        _inventory = new List<Item>();
        combatHistory = new Dictionary<int, Combat>();
        _currentInteractions = new List<Interaction>();
        characterToken = new CharacterToken(this);
        tokenInInventory = null;
        interactionWeights = new WeightedDictionary<INTERACTION_TYPE>();
        relationships = new Dictionary<Character, List<RelationshipTrait>>();

        //AllocateStats();
        //EquipItemsByClass();
        //ConstructBuffs();

        skinColor = Color.HSVToRGB(UnityEngine.Random.Range(1, 80f)/360f, 15f/100f, 100f/100f);
        hairColor = Color.HSVToRGB(UnityEngine.Random.Range(0f, 360f)/360f, 25f/100f, 90f/100f);

        hSkinColor = UnityEngine.Random.Range(-360f, 360f);
        hHairColor = UnityEngine.Random.Range(-360f, 360f);

        GetRandomCharacterColor();
        //_combatHistoryID = 0;
#if !WORLD_CREATION_TOOL
        SetDailyInteractionGenerationTick(GetMonthInteractionTick());
#endif
        SubscribeToSignals();
    }
    public void Initialize() { }

    #region Signals
    private void SubscribeToSignals() {
        //Messenger.AddListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.AddListener(Signals.HOUR_ENDED, EverydayAction);
        //Messenger.AddListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
        //Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, RemoveRelationshipWith);
        //Messenger.AddListener<Area>(Signals.AREA_DELETED, OnAreaDeleted);
        //Messenger.AddListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
        Messenger.AddListener(Signals.DAY_STARTED, DailyInteractionGeneration);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);
        Messenger.AddListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
    }
    public void UnsubscribeSignals() {
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.RemoveListener(Signals.HOUR_ENDED, EverydayAction);
        //Messenger.RemoveListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, RemoveRelationshipWith);
        //Messenger.RemoveListener<Area>(Signals.AREA_DELETED, OnAreaDeleted);
        //Messenger.RemoveListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyInteractionGeneration);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);
        Messenger.RemoveListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
    }
    #endregion

    //      private void AllocateStatPoints(int statAllocation){
    //          _baseStrength = 0;
    //          _baseIntelligence = 0;
    //          _baseAgility = 0;
    //          _baseVitality = 0;

    //	WeightedDictionary<string> statWeights = new WeightedDictionary<string> ();
    //	statWeights.AddElement ("strength", (int) _characterClass.strWeightAllocation);
    //	statWeights.AddElement ("intelligence", (int) _characterClass.intWeightAllocation);
    //	statWeights.AddElement ("agility", (int) _characterClass.agiWeightAllocation);
    //	statWeights.AddElement ("vitality", (int) _characterClass.vitWeightAllocation);

    //	if(statWeights.GetTotalOfWeights() > 0){
    //		string chosenStat = string.Empty;
    //		for (int i = 0; i < statAllocation; i++) {
    //			chosenStat = statWeights.PickRandomElementGivenWeights ();
    //			if (chosenStat == "strength") {
    //				_baseStrength += 1;
    //			}else if (chosenStat == "intelligence") {
    //				_baseIntelligence += 1;
    //			}else if (chosenStat == "agility") {
    //				_baseAgility += 1;
    //			}else if (chosenStat == "vitality") {
    //				_baseVitality += 1;
    //			}
    //		}
    //	}
    //}
    //Enables or Disables skills based on skill requirements
    public void EnableDisableSkills(Combat combat) {
        //bool isAllAttacksInRange = true;
        //bool isAttackInRange = false;

        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            //            if (skill is AttackSkill){
            //                AttackSkill attackSkill = skill as AttackSkill;
            //                if(attackSkill.spCost > _sp) {
            //                    skill.isEnabled = false;
            //                    continue;
            //                }
            //} else 
            if (skill is FleeSkill) {
                skill.isEnabled = false;
                //if (this.currentHP >= (this.maxHP / 2)) {
                //    skill.isEnabled = false;
                //    continue;
                //}
            }
        }

        //Character class skills
        //if(_equippedWeapon != null) {
        //    for (int i = 0; i < _level; i++) {
        //        if(i < _characterClass.skillsPerLevel.Count) {
        //            if (_characterClass.skillsPerLevel[i] != null) {
        //                for (int j = 0; j < _characterClass.skillsPerLevel[i].Length; j++) {
        //                    Skill skill = _characterClass.skillsPerLevel[i][j];
        //                    skill.isEnabled = true;

        //                    //Check for allowed weapon types
        //                    if (skill.allowedWeaponTypes != null) {
        //                        for (int k = 0; k < skill.allowedWeaponTypes.Length; k++) {
        //                            if (!skill.allowedWeaponTypes.Contains(_equippedWeapon.weaponType)) {
        //                                skill.isEnabled = false;
        //                                continue;
        //                            }
        //                        }
        //                    }

        //                    if (skill is AttackSkill) {
        //                        AttackSkill attackSkill = skill as AttackSkill;
        //                        if (attackSkill.spCost > _sp) {
        //                            skill.isEnabled = false;
        //                            continue;
        //                        }
        //                    }
        //                }
        //            }
        //        } else {
        //            break;
        //        }
        //    }

        //}

    }
    //Changes row number of this character
    public void SetRowNumber(int rowNumber) {
        this._currentRow = rowNumber;
    }
    //Changes character's side
    public void SetSide(SIDES side) {
        this._currentSide = side;
    }
    //Adjust current HP based on specified paramater, but HP must not go below 0
    public virtual void AdjustHP(int amount, bool triggerDeath = false) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        Messenger.Broadcast(Signals.ADJUSTED_HP, this);
        if (triggerDeath && previous != this._currentHP) {
            if (this._currentHP == 0) {
                Death();
            }
        }
    }

    private string GetFaintOrDeath() {
        return "die";
        //WeightedDictionary<string> faintDieDict = new WeightedDictionary<string> ();
        //int faintWeight = 100;
        //int dieWeight = 50;
        //if(HasTrait(TRAIT.GRITTY)){
        //	faintWeight += 50;
        //}
        //if(HasTrait(TRAIT.ROBUST)){
        //	faintWeight += 50;
        //}
        //if(HasTrait(TRAIT.FRAGILE)){
        //	dieWeight += 50;
        //}
        //faintDieDict.AddElement ("faint", 100);
        //faintDieDict.AddElement ("die", 50);

        //return faintDieDict.PickRandomElementGivenWeights ();
    }
    //public void FaintOrDeath(ICharacter killer) {
    //    string pickedWeight = GetFaintOrDeath();
    //    if (pickedWeight == "faint") {
    //        if (currentParty.currentCombat == null) {
    //            Faint();
    //        } else {
    //            currentParty.currentCombat.CharacterFainted(this);
    //        }
    //    } else if (pickedWeight == "die") {
    //        if (currentParty.currentCombat != null) {
    //            currentParty.currentCombat.CharacterDeath(this, killer);
    //        }
    //        Death();
    //        //            if (this.currentCombat == null){
    //        //	Death ();
    //        //}else{
    //        //	this.currentCombat.CharacterDeath (this);
    //        //}
    //    }
    //}
    //When character will faint
    internal void Faint() {
        if (!_isFainted) {
            _isFainted = true;
            SetHP(1);
            ////Set Task to Fainted
            //Faint faintTask = new Faint(this);
            //faintTask.OnChooseTask(this)
            ; }
    }
    internal void Unfaint() {
        if (_isFainted) {
            _isFainted = false;
            SetHP(1);
        }
    }
    //Character's death
    public void SetIsDead(bool isDead) {
        _isDead = isDead;
    }
    public void ReturnToLife() {
        if (_isDead) {
            SetIsDead(false);
            SubscribeToSignals();
            _ownParty.ReturnToLife();
        }
    }
    public void Death() {
        if (!_isDead) {
            SetIsDead(true);
            UnsubscribeSignals();

            CombatManager.Instance.ReturnCharacterColorToPool(_characterColor);

            if (currentParty.specificLocation == null) {
                throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }

            //if (currentParty.specificLocation != null && currentParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            //    Log deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death");
            //    deathLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //    AddHistory(deathLog);
            //    (currentParty.specificLocation.coreTile.landmarkOnTile).AddHistory(deathLog);
            //}

            //Drop all Items
            //            while (_equippedItems.Count > 0) {
            //	ThrowItem (_equippedItems [0]);
            //}
            while (_inventory.Count > 0) {
                ThrowItem(_inventory[0]);
            }
            if (ownParty.specificLocation != null && tokenInInventory != null) {
                tokenInInventory.SetOwner(null);
                DropToken(ownParty.specificLocation, currentStructure);
            }
            if (this.race != RACE.SKELETON && this.race != RACE.BEAST) {
                ownParty.specificLocation.AddCorpse(this);
            }
            if (!IsInOwnParty()) {
                _currentParty.RemoveCharacter(this);
            }
            _ownParty.PartyDeath();
            //if (currentParty.id != _ownParty.id) {
            //}
            //_ownParty.PartyDeath();
            //Remove ActionData
            //_actionData.DetachActionData();

            //if(_home != null){
            //                //Remove character home on landmark
            //	_home.RemoveCharacterHomeOnLandmark (this);
            //}

            if (this._faction != null) {
                this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
            }

            //if (_specificLocation != null) {
            //    _specificLocation.RemoveCharacterFromLocation(this);
            //}
            //if (_avatar != null) {
            //    if (_avatar.mainCharacter.id == this.id) {
            //        DestroyAvatar();
            //    } else {
            //        _avatar.RemoveCharacter(this); //if the character has an avatar, remove it from the list of characters
            //    }
            //}
            //if (_isPrisoner){
            //	PrisonerDeath ();
            //}
            if (_role != null) {
                _role.DeathRole();
            }
            if (homeArea != null) {
                Area home = homeArea;
                homeArea.RemoveResident(this);
                SetHome(home); //keep this data with character to prevent errors
            }
            //while(_tags.Count > 0){
            //	RemoveCharacterAttribute (_tags [0]);
            //}
            //while (questData.Count != 0) {
            //    questData[0].AbandonQuest();
            //}
            //				if(Messenger.eventTable.ContainsKey("CharacterDeath")){
            //					Messenger.Broadcast ("CharacterDeath", this);
            //				}
            if (_minion != null) {
                PlayerManager.Instance.player.RemoveMinion(_minion);
            }
            if (onCharacterDeath != null) {
                onCharacterDeath();
            }
            onCharacterDeath = null;
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this);
            //if (killer != null) {
            //    Messenger.Broadcast(Signals.CHARACTER_KILLED, killer, this);
            //}


            //ObjectState deadState = _characterObject.GetState("Dead");
            //_characterObject.ChangeState(deadState);

            //GameObject.Destroy(_icon.gameObject);
            //_icon = null;

            Debug.Log(this.name + " died!");
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "death");
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //log.AddToFillers(specificLocation, specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
            AddHistory(log);
        }
    }
    public void Assassinate(Character assassin) {
        Debug.Log(assassin.name + " assassinated " + name);
        Death();
    }
    internal void AddActionOnDeath(OnCharacterDeath onDeathAction) {
        onCharacterDeath += onDeathAction;
    }
    internal void RemoveActionOnDeath(OnCharacterDeath onDeathAction) {
        onCharacterDeath -= onDeathAction;
    }

    #region Items
    //If a character picks up an item, it is automatically added to his/her inventory
    internal void PickupItem(Item item, bool broadcast = true) {
        Item newItem = item;
        if (_inventory.Contains(newItem)) {
            throw new Exception(this.name + " already has an instance of " + newItem.itemName);
        }
        this._inventory.Add(newItem);
        //newItem.SetPossessor (this);
        if (newItem.owner == null) {
            OwnItem(newItem);
        }
#if !WORLD_CREATION_TOOL
        Log obtainLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "obtain_item");
        obtainLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        obtainLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
        AddHistory(obtainLog);
        _ownParty.specificLocation.AddHistory(obtainLog);
#endif
        if (broadcast) {
            Messenger.Broadcast(Signals.ITEM_OBTAINED, newItem, this);
        }
        newItem.OnItemPutInInventory(this);
    }
    internal void ThrowItem(Item item, bool addInLandmark = true) {
        if (item.isEquipped) {
            UnequipItem(item);
        }
        //item.SetPossessor (null);
        this._inventory.Remove(item);
        //item.exploreWeight = 15;
        //if (addInLandmark) {
        //    Area location = _ownParty.specificLocation;
        //    if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
        //        BaseLandmark landmark = location as BaseLandmark;
        //        landmark.AddItem(item);
        //    }
        //}
        Messenger.Broadcast(Signals.ITEM_THROWN, item, this);
    }
    internal void ThrowItem(string itemName, int quantity, bool addInLandmark = true) {
        for (int i = 0; i < quantity; i++) {
            if (HasItem(itemName)) {
                ThrowItem(GetItemInInventory(itemName), addInLandmark);
            }
        }
    }
    internal void DropItem(Item item) {
        ThrowItem(item);
        Area location = _ownParty.specificLocation;
        if (location != null) {
            //BaseLandmark landmark = location as BaseLandmark;
            Log dropLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "drop_item");
            dropLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            dropLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
            dropLog.AddToFillers(location, location.name, LOG_IDENTIFIER.LANDMARK_1);
            AddHistory(dropLog);
            location.AddHistory(dropLog);
        }

    }
    public void EquipItem(string itemName) {
        Item item = ItemManager.Instance.CreateNewItemInstance(itemName);
        if (item != null) {
            EquipItem(item);
        }
    }

    /*
        this is the real way to equip an item
        this will return a boolean whether the character successfully equipped
        the item or not.
            */
    internal bool EquipItem(Item item) {
        bool hasEquipped = false;
        if (item.itemType == ITEM_TYPE.WEAPON) {
            Weapon weapon = item as Weapon;
            hasEquipped = TryEquipWeapon(weapon);
        } else if (item.itemType == ITEM_TYPE.ARMOR) {
            Armor armor = item as Armor;
            hasEquipped = TryEquipArmor(armor);
        } else if (item.itemType == ITEM_TYPE.ACCESSORY) {
            hasEquipped = TryEquipAccessory(item);
        } else if (item.itemType == ITEM_TYPE.CONSUMABLE) {
            hasEquipped = TryEquipConsumable(item);
        }
        if (hasEquipped) {
            if (item.attributeNames != null) {
                for (int i = 0; i < item.attributeNames.Count; i++) {
                    Trait newTrait = AttributeManager.Instance.allTraits[item.attributeNames[i]];
                    AddTrait(newTrait);
                }
            }
#if !WORLD_CREATION_TOOL
            Log equipLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "equip_item");
            equipLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            equipLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
            AddHistory(equipLog);
#endif
            Messenger.Broadcast(Signals.ITEM_EQUIPPED, item, this);
        }
        return hasEquipped;
    }
    //Unequips an item of a character, whether it's a weapon, armor, etc.
    public void UnequipItem(Item item) {
        if (item.itemType == ITEM_TYPE.WEAPON) {
            UnequipWeapon(item as Weapon);
        } else if (item.itemType == ITEM_TYPE.ARMOR) {
            UnequipArmor(item as Armor);
        } else if (item.itemType == ITEM_TYPE.ACCESSORY) {
            UnequipAccessory(item);
        } else if (item.itemType == ITEM_TYPE.CONSUMABLE) {
            UnequipConsumable(item);
        }
        if (item.attributeNames != null) {
            for (int i = 0; i < item.attributeNames.Count; i++) {
                Trait newTrait = AttributeManager.Instance.allTraits[item.attributeNames[i]];
                RemoveTrait(newTrait);
            }
        }
        Messenger.Broadcast(Signals.ITEM_UNEQUIPPED, item, this);
    }
    //Own an Item
    internal void OwnItem(Item item) {
        item.SetOwner(this);
    }
    //Transfer item ownership
    internal void TransferItemOwnership(Item item, Character newOwner) {
        newOwner.OwnItem(item);
    }
    //Try to equip a weapon to a body part of this character and add it to the list of items this character have
    internal bool TryEquipWeapon(Weapon weapon) {
        //if (!_characterClass.allowedWeaponTypes.Contains(weapon.weaponType)) {
        //    return false;
        //}
        _equippedWeapon = weapon;
        weapon.SetEquipped(true);
        return true;
    }
    //Unequips weapon of a character
    private void UnequipWeapon(Weapon weapon) {
        weapon.SetEquipped(false);
        _equippedWeapon = null;
    }
    //Try to equip an armor to a body part of this character and add it to the list of items this character have
    internal bool TryEquipArmor(Armor armor) {
        armor.SetEquipped(true);
        _equippedArmor = armor;
        return true;
    }
    //Unequips armor of a character
    private void UnequipArmor(Armor armor) {
        armor.SetEquipped(false);
        _equippedArmor = null;
    }
    //Try to equip an accessory
    internal bool TryEquipAccessory(Item accessory) {
        accessory.SetEquipped(true);
        _equippedAccessory = accessory;
        return true;
    }
    //Unequips accessory of a character
    private void UnequipAccessory(Item accessory) {
        accessory.SetEquipped(false);
        _equippedAccessory = null;
    }
    //Try to equip an consumable
    internal bool TryEquipConsumable(Item consumable) {
        consumable.SetEquipped(true);
        _equippedConsumable = consumable;
        return true;
    }
    //Unequips consumable of a character
    private void UnequipConsumable(Item consumable) {
        consumable.SetEquipped(false);
        _equippedConsumable = null;
    }
    internal bool HasItem(string itemName) {
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            return true;
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            return true;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            return true;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            return true;
        }
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Equals(itemName)) {
                return true;
            }
        }
        return false;
    }
    internal bool HasItem(Item item) {
        if (_equippedWeapon != null && _equippedWeapon.itemName == item.itemName) {
            return true;
        } else if (_equippedArmor != null && _equippedArmor.itemName == item.itemName) {
            return true;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == item.itemName) {
            return true;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == item.itemName) {
            return true;
        }
        if (inventory.Contains(item)) {
            return true;
        }
        return false;
    }
    /*
        Does this character have an item that is like the required item.
        For example, if you want to check if the character has any scrolls,
        without specifying the types of scrolls.
            */
    internal bool HasItemLike(string itemName, int quantity) {
        int counter = 0;
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            counter++;
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            counter++;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            counter++;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            counter++;
        }
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Contains(itemName)) {
                counter++;
            }
        }
        if (counter >= quantity) {
            return true;
        } else {
            return false;
        }
    }
    public List<Item> GetItemsLike(string itemName) {
        List<Item> items = new List<Item>();
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            items.Add(_equippedWeapon);
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            items.Add(_equippedArmor);
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            items.Add(_equippedAccessory);
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            items.Add(_equippedConsumable);
        }
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Contains(itemName)) {
                items.Add(currItem);
            }
        }
        return items;
    }
    internal Item GetItemInInventory(string itemName) {
        for (int i = 0; i < _inventory.Count; i++) {
            Item currItem = _inventory[i];
            if (currItem.itemName.Equals(itemName)) {
                return currItem;
            }
        }
        return null;
    }
    public void GiveItemsTo(List<Item> items, Character otherCharacter) {
        for (int i = 0; i < items.Count; i++) {
            Item currItem = items[i];
            if (this.HasItem(currItem)) { //check if the character still has the item that he wants to give
                this.ThrowItem(currItem, false);
                otherCharacter.PickupItem(currItem);
                Debug.Log(this.name + " gave item " + currItem.itemName + " to " + otherCharacter.name);
            }
        }
    }
    //private void EquipItemsByClass() {
    //    if (_characterClass != null) {
    //        if (_characterClass.weaponTierNames != null && _characterClass.weaponTierNames.Count > 0) {
    //            EquipItem(_characterClass.weaponTierNames[0]);
    //        }
    //        if (_characterClass.armorTierNames != null && _characterClass.armorTierNames.Count > 0) {
    //            EquipItem(_characterClass.armorTierNames[0]);
    //        }
    //        if (_characterClass.accessoryTierNames != null && _characterClass.accessoryTierNames.Count > 0) {
    //            EquipItem(_characterClass.accessoryTierNames[0]);
    //        }
    //    }
    //}
    public void UpgradeWeapon() {
        //if (_characterClass != null && _equippedWeapon != null) {
        //    if (_characterClass.weaponTierNames != null && _characterClass.weaponTierNames.Count > 0) {
        //        bool foundEquipped = false;
        //        for (int i = 0; i < _characterClass.weaponTierNames.Count; i++) {
        //            if (foundEquipped) {
        //                //Found equipped item, now equip next on the list for upgrade
        //                EquipItem(_characterClass.weaponTierNames[i]);
        //                break;
        //            } else {
        //                if (_equippedWeapon.itemName == _characterClass.weaponTierNames[i]) {
        //                    foundEquipped = true;
        //                }
        //            }
        //        }
        //    }
        //}
    }
    public void UpgradeArmor() {
        //if (_characterClass != null && _equippedArmor != null) {
        //    if (_characterClass.armorTierNames != null && _characterClass.armorTierNames.Count > 0) {
        //        bool foundEquipped = false;
        //        for (int i = 0; i < _characterClass.armorTierNames.Count; i++) {
        //            if (foundEquipped) {
        //                //Found equipped item, now equip next on the list for upgrade
        //                EquipItem(_characterClass.armorTierNames[i]);
        //                break;
        //            } else {
        //                if (_equippedArmor.itemName == _characterClass.armorTierNames[i]) {
        //                    foundEquipped = true;
        //                }
        //            }
        //        }
        //    }
        //}
    }
    public void UpgradeAccessory() {
        //if (_characterClass != null && _equippedAccessory != null) {
        //    if (_characterClass.accessoryTierNames != null && _characterClass.accessoryTierNames.Count > 0) {
        //        bool foundEquipped = false;
        //        for (int i = 0; i < _characterClass.accessoryTierNames.Count; i++) {
        //            if (foundEquipped) {
        //                //Found equipped weapon, now equip next on the list for upgrade
        //                EquipItem(_characterClass.accessoryTierNames[i]);
        //                break;
        //            } else {
        //                if (_equippedAccessory.itemName == _characterClass.accessoryTierNames[i]) {
        //                    foundEquipped = true;
        //                }
        //            }
        //        }
        //    }
        //}
    }
    #endregion

    //#region Status Effects
    //internal void AddStatusEffect(STATUS_EFFECT statusEffect) {
    //    this._statusEffects.Add(statusEffect);
    //}
    //internal void RemoveStatusEffect(STATUS_EFFECT statusEffect) {
    //    this._statusEffects.Remove(statusEffect);
    //}
    //internal void CureStatusEffects() {
    //    for (int i = 0; i < _statusEffects.Count; i++) {
    //        STATUS_EFFECT statusEffect = _statusEffects[i];
    //        int chance = Utilities.rng.Next(0, 100);
    //        if (chance < 15) {
    //            _ownParty.currentCombat.AddCombatLog(this.name + " is cured from " + statusEffect.ToString().ToLower() + ".", this.currentSide);
    //            RemoveStatusEffect(statusEffect);
    //            i--;
    //        }
    //    }
    //}
    //internal bool HasStatusEffect(STATUS_EFFECT statusEffect) {
    //    if (_statusEffects.Contains(statusEffect)) {
    //        return true;
    //    }
    //    return false;
    //}
    //#endregion

    #region Skills
    //private List<Skill> GetGeneralSkills(){
    //          List<Skill> allGeneralSkills = new List<Skill>();
    //          foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
    //              allGeneralSkills.Add(skill.CreateNewCopy());
    //          }
    //          return allGeneralSkills;
    //}
    //public List<Skill> GetClassSkills() {
    //    List<Skill> skills = new List<Skill>();
    //    for (int i = 0; i < level; i++) {
    //        if (i < characterClass.skillsPerLevel.Count) {
    //            if (characterClass.skillsPerLevel[i] != null) {
    //                for (int j = 0; j < characterClass.skillsPerLevel[i].Length; j++) {
    //                    Skill skill = characterClass.skillsPerLevel[i][j];
    //                    skills.Add(skill);
    //                }
    //            }
    //        }
    //    }
    //    return skills;
    //}
    //public List<AttackSkill> GetClassAttackSkills() {
    //    List<AttackSkill> skills = new List<AttackSkill>();
    //    for (int i = 0; i < level; i++) {
    //        if (i < characterClass.skillsPerLevel.Count) {
    //            if (characterClass.skillsPerLevel[i] != null) {
    //                for (int j = 0; j < characterClass.skillsPerLevel[i].Length; j++) {
    //                    Skill skill = characterClass.skillsPerLevel[i][j];
    //                    if(skill is AttackSkill) {
    //                        skills.Add(skill as AttackSkill);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return skills;
    //}
    //private List<Skill> GetBodyPartSkills(){
    //	List<Skill> allBodyPartSkills = new List<Skill>();
    //	foreach (Skill skill in SkillManager.Instance.bodyPartSkills.Values) {
    //		bool requirementsPassed = true;
    //		//Skill skill	= SkillManager.Instance.bodyPartSkills [skillName];
    //		for (int j = 0; j < skill.skillRequirements.Length; j++) {
    //			if(!HasAttribute(skill.skillRequirements[j].attributeRequired, skill.skillRequirements[j].itemQuantity)){
    //				requirementsPassed = false;
    //				break;
    //			}
    //		}
    //		if(requirementsPassed){
    //			allBodyPartSkills.Add (skill.CreateNewCopy ());
    //		}
    //	}
    //	return allBodyPartSkills;
    //}
    #endregion

    #region Roles
    public void AssignRole(CHARACTER_ROLE role) {
        bool wasRoleChanged = false;
        if (_role != null) {
            _role.ChangedRole();
#if !WORLD_CREATION_TOOL
            Log roleChangeLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "change_role");
            roleChangeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            AddHistory(roleChangeLog);
#endif
            wasRoleChanged = true;
        }
        switch (role) {
            case CHARACTER_ROLE.HERO:
            _role = new Hero(this);
            break;
            case CHARACTER_ROLE.VILLAIN:
            _role = new Villain(this);
            break;
            case CHARACTER_ROLE.CIVILIAN:
            _role = new Civilian(this);
            break;
            case CHARACTER_ROLE.KING:
            _role = new King(this);
            break;
            case CHARACTER_ROLE.PLAYER:
            _role = new PlayerRole(this);
            break;
            case CHARACTER_ROLE.GUARDIAN:
            _role = new Guardian(this);
            break;
            case CHARACTER_ROLE.BEAST:
            _role = new Beast(this);
            break;
            case CHARACTER_ROLE.LEADER:
            _role = new Leader(this);
            break;
            case CHARACTER_ROLE.BANDIT:
            _role = new Bandit(this);
            break;
            case CHARACTER_ROLE.ARMY:
            _role = new Army(this);
            SetName(this.characterClass.className);
            break;
            default:
            break;
        }
        if (_role != null) {
            _role.OnAssignRole();
#if !WORLD_CREATION_TOOL
            AddDefaultInteractions();
#endif
        }
        if (wasRoleChanged) {
            Messenger.Broadcast(Signals.ROLE_CHANGED, this);
        }
    }
    #endregion

    #region Character Class
    public void AssignClass(CharacterClass charClass) {
        _characterClass = charClass.CreateNewCopy();
        _skills = new List<Skill>();
        _skills.Add(_characterClass.skill);
        //EquipItemsByClass();
        SetTraitsFromClass();
    }
    #endregion

    #region Job
    private void AssignRandomJob() {
        if (CharacterManager.Instance.IsClassADeadlySin(_characterClass.className)) {
            AssignJob(_characterClass.jobType);
        } else {
            JOB[] jobs = new JOB[] { JOB.DIPLOMAT, JOB.DEBILITATOR, JOB.EXPLORER, JOB.INSTIGATOR, JOB.RAIDER, JOB.RECRUITER, JOB.SPY };
            AssignJob(jobs[UnityEngine.Random.Range(0, jobs.Length)]);
            //AssignJob(JOB.RAIDER);
        }
    }
    public void AssignJob(JOB jobType) {
        switch (jobType) {
            case JOB.SPY:
                _job = new Spy(this);
                break;
            case JOB.RAIDER:
                _job = new Raider(this);
                break;
            case JOB.INSTIGATOR:
                _job = new Instigator(this);
                break;
            case JOB.EXPLORER:
                _job = new Explorer(this);
                break;
            case JOB.DEBILITATOR:
                _job = new Dissuader(this);
                break;
            case JOB.DIPLOMAT:
                _job = new Diplomat(this);
                break;
            case JOB.RECRUITER:
                _job = new Recruiter(this);
                break;
            case JOB.LEADER:
                _job = new LeaderJob(this);
                break;
            case JOB.WORKER:
                _job = new Worker(this);
                break;
            default:
                _job = new Job(this, JOB.NONE);
                break;
        }
        _job.OnAssignJob();
    }
    #endregion

    #region Faction
    public void SetFaction(Faction newFaction) {
        if (_faction != null 
            && newFaction != null
            && _faction.id == newFaction.id) {
            //ignore change, because character is already part of that faction
            return;
        }
        _faction = newFaction;
        OnChangeFaction();
        UpdateTokenOwner();
        if (_faction != null) {
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
        }
    }
    public void ChangeFactionTo(Faction newFaction) {
        if (this.faction.id == newFaction.id) {
            return; //if the new faction is the same, ignore change
        }
        faction.RemoveCharacter(this);
        newFaction.AddNewCharacter(this);
    }
    private void OnChangeFaction() {
        //check if this character has a Criminal Trait, if so, remove it
        Trait criminal = GetTrait("Criminal");
        if (criminal != null) {
            RemoveTrait(criminal, false);
        }
    }
    public void FoundFaction(string factionName, Area location) {
        SetForcedInteraction(null);
        MigrateHomeTo(location);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName(factionName);
        newFaction.SetLeader(this);
        ChangeFactionTo(newFaction);
        FactionManager.Instance.neutralFaction.RemoveFromOwnedAreas(location);
        LandmarkManager.Instance.OwnArea(newFaction, race, location);
        newFaction.SetFactionActiveState(true);
    }
    #endregion

    #region Party
    /*
        Create a new Party with this character as the leader.
            */
    public virtual Party CreateOwnParty() {
        if (_ownParty != null) {
            _ownParty.RemoveCharacter(this);
        }
        CharacterParty newParty = new CharacterParty(this);
        SetOwnedParty(newParty);
        newParty.AddCharacter(this);
        //newParty.CreateCharacterObject();
        return newParty;
    }
    public virtual void SetOwnedParty(Party party) {
        _ownParty = party as CharacterParty;
    }
    public virtual void SetCurrentParty(Party party) {
        _currentParty = party as CharacterParty;
    }
    public void OnRemovedFromParty() {
        SetCurrentParty(ownParty); //set the character's party to it's own party
        //if (ownParty is CharacterParty) {
        //    if ((ownParty as CharacterParty).actionData.currentAction != null) {
        //        (ownParty as CharacterParty).actionData.currentAction.EndAction(ownParty, (ownParty as CharacterParty).actionData.currentTargetObject);
        //    }
        //}
        if (this.minion != null) {
            this.minion.SetEnabledState(true); //reenable this minion, since it could've been disabled because it was part of another party
        }
    }
    public void OnAddedToParty() {
        if (currentParty.id != ownParty.id) {
            ownParty.specificLocation.RemoveCharacterFromLocation(ownParty);
            //ownParty.icon.SetVisualState(false);
        }
    }
    public void OnAddedToPlayer() {
        //if (ownParty.specificLocation is BaseLandmark) {
        //    ownParty.specificLocation.RemoveCharacterFromLocation(ownParty);
        //}
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(ownParty);
        //if (this.homeArea != null) {
        //    this.homeArea.RemoveResident(this);
        //}
        //PlayerManager.Instance.player.playerArea.AddResident(this);
        MigrateHomeTo(PlayerManager.Instance.player.playerArea);
    }
    public bool IsInParty() {
        if (currentParty.characters.Count > 1) {
            return true; //if the character is in a party that has more than 1 characters
        }
        return false;
    }
    public bool IsInOwnParty() {
        if (currentParty.id == ownParty.id) {
            return true;
        }
        return false;
    }
    #endregion

    #region Location
    public bool IsCharacterInAdjacentRegionOfThis(Character targetCharacter) {
        for (int i = 0; i < _currentRegion.adjacentRegionsViaRoad.Count; i++) {
            if (targetCharacter.party.currentRegion.id == _currentRegion.adjacentRegionsViaRoad[i].id) {
                return true;
            }
        }
        return false;
    }
    public void SetCurrentStructureLocation(LocationStructure currentStructure) {
        this.currentStructure = currentStructure;
    }
    public void MoveToRandomStructureInArea() {
        LocationStructure locationStructure = specificLocation.GetRandomStructure();
        MoveToAnotherStructure(locationStructure);
    }
    public void MoveToAnotherStructure(LocationStructure newStructure) {
        if(currentStructure != null) {
            currentStructure.RemoveCharacterAtLocation(this);
        }
        newStructure.AddCharacterAtLocation(this);
    }
    public void MoveToAnotherStructure(STRUCTURE_TYPE structureType) {
        if (currentStructure != null) {
            currentStructure.RemoveCharacterAtLocation(this);
        }
        if (specificLocation.HasStructure(structureType)) {
            LocationStructure newStructure = specificLocation.GetRandomStructureOfType(structureType);
            newStructure.AddCharacterAtLocation(this);
        } else {
            throw new Exception("Can't move " + name + " to a " + structureType.ToString() + " because " + specificLocation.name + " does not have that structure!");
        }
    }
    #endregion

    #region Utilities
    public void ChangeGender(GENDER gender) {
        _gender = gender;
        Messenger.Broadcast(Signals.GENDER_CHANGED, this, gender);
    }
    public void ChangeRace(RACE race) {
        if (_raceSetting.race == race) {
            return; //current race is already the new race, no change
        }
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        //Update Portrait to use new race
        _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, this);
    }
    public void ChangeClass(string className) {
        //TODO: Change data as needed
        string previousClassName = _characterClass.className;
        CharacterClass charClass = CharacterManager.Instance.classesDictionary[className];
        AssignClass(charClass);
        //_characterClass = charClass.CreateNewCopy();
        OnCharacterClassChange();

#if !WORLD_CREATION_TOOL
        //_homeLandmark.tileLocation.areaOfTile.excessClasses.Remove(previousClassName);
        //_homeLandmark.tileLocation.areaOfTile.missingClasses.Remove(_characterClass.className);

        //Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChangeClassAction", "change_class");
        //log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //log.AddToFillers(null, previousClassName, LOG_IDENTIFIER.STRING_1);
        //log.AddToFillers(null, _characterClass.className, LOG_IDENTIFIER.STRING_2);
        //AddHistory(log);
        //check equipped items
#endif

    }
    public void SetName(string newName) {
        _name = newName;
        _firstName = _name.Split(' ')[0];
    }
    //If true, character can't do daily action (onDailyAction), i.e. actions, needs
    //public void SetIsIdle(bool state) {
    //    _isIdle = state;
    //}
    //public bool HasPathToParty(Party partyToJoin) {
    //    return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.PASSABLE, _faction) != null;
    //}
    public void CenterOnCharacter() {
        if (!this.isDead) {
            CameraMove.Instance.CenterCameraOn(currentParty.specificLocation.coreTile.gameObject);
        }
    }
    private void GetRandomCharacterColor() {
        _characterColor = CombatManager.Instance.UseRandomCharacterColor();
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    public void SetCharacterColor(Color color) {
        _characterColor = color;
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    //public void EverydayAction() {
    //    if (onDailyAction != null) {
    //        onDailyAction();
    //    }
    //    CheckForPPDeath();
    //}
    //public void AdvertiseSelf(ActionThread actionThread) {
    //    if(actionThread.character.id != this.id && _currentRegion.id == actionThread.character.party.currentRegion.id) {
    //        actionThread.AddToChoices(_characterObject);
    //    }
    //}
    //public bool CanObtainResource(List<RESOURCE> resources) {
    //    if (this.role != null) {//characters without a role cannot get actions, and therefore cannot obtain resources
    //        for (int i = 0; i < _ownParty.currentRegion.landmarks.Count; i++) {
    //            BaseLandmark landmark = _ownParty.currentRegion.landmarks[i];
    //            StructureObj iobject = landmark.landmarkObj;
    //            if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
    //                for (int k = 0; k < iobject.currentState.actions.Count; k++) {
    //                    CharacterAction action = iobject.currentState.actions[k];
    //                    if (action.actionData.resourceGiven != RESOURCE.NONE && resources.Contains(action.actionData.resourceGiven)) { //does the action grant a resource, and is that a resource that is needed
    //                        if (action.MeetsRequirements(_ownParty, landmark) && action.CanBeDone(iobject) && action.CanBeDoneBy(_ownParty, iobject)) { //Filter
    //                            //if the character can do an action that yields a needed resource, return true
    //                            return true;
    //                        }
    //                    }

    //                }
    //            }
    //        }
    //    }
    //    return false;
    //}
    public bool IsSpecialCivilian() {
        if (this.characterClass != null) {
            if (this.characterClass.className.Equals("Farmer") || this.characterClass.className.Equals("Miner") || this.characterClass.className.Equals("Retired Hero") ||
                this.characterClass.className.Equals("Shopkeeper") || this.characterClass.className.Equals("Woodcutter")) {
                return true;
            }
        }
        return false;
    }
    private void OnOtherCharacterDied(Character characterThatDied) {
        if (characterThatDied.id != this.id) {
            //Friend friend = this.GetFriendTraitWith(characterThatDied);
            //if (friend != null) {
            //    RemoveTrait(friend);
            //}

            //Enemy enemy = this.GetEnemyTraitWith(characterThatDied);
            //if (enemy != null) {
            //    RemoveTrait(enemy);
            //}
            List<RelationshipTrait> rels = GetAllRelationshipTraitWith(characterThatDied);
            if (rels != null) {
                for (int i = 0; i < rels.Count; i++) {
                    RemoveTrait(rels[i]);
                }
            }
        }
    }
    //public bool IsCharacterLovedOne(Character otherCharacter) {
    //    Relationship rel = GetRelationshipWith(otherCharacter);
    //    if (rel != null) {
    //        CHARACTER_RELATIONSHIP[] lovedOneStatuses = new CHARACTER_RELATIONSHIP[] {
    //            CHARACTER_RELATIONSHIP.FATHER,
    //            CHARACTER_RELATIONSHIP.MOTHER,
    //            CHARACTER_RELATIONSHIP.BROTHER,
    //            CHARACTER_RELATIONSHIP.SISTER,
    //            CHARACTER_RELATIONSHIP.SON,
    //            CHARACTER_RELATIONSHIP.DAUGHTER,
    //            CHARACTER_RELATIONSHIP.LOVER,
    //            CHARACTER_RELATIONSHIP.HUSBAND,
    //            CHARACTER_RELATIONSHIP.WIFE,
    //        };
    //        if (rel.HasAnyStatus(lovedOneStatuses)) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public void SetMode(MODE mode) {
        _currentMode = mode;
    }
    public void AdjustDoNotDisturb(int amount) {
        _doNotDisturb += amount;
        if(_doNotDisturb < 0) {
            _doNotDisturb = 0;
        }
    }
    public void SetAlreadyTargetedByGrudge(bool state) {
        _alreadyTargetedByGrudge = state;
    }
    public void AttackAnArea(Area target) {
        Interaction attackInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ATTACK, target);
        attackInteraction.AddEndInteractionAction(() => _ownParty.GoHomeAndDisband());
        attackInteraction.SetCanInteractionBeDoneAction(() => IsTargetStillViable(target));
        _ownParty.GoToLocation(target, PATHFINDING_MODE.NORMAL, null, () => SetForcedInteraction(attackInteraction));
    }
    private bool IsTargetStillViable(Area target) {
        return target.owner != null;
    }
    public void ReturnToOriginalHomeAndFaction(Area ogHome, Faction ogFaction) { 
        //first, check if the character's original faction is still alive
        if (!ogFaction.isDestroyed) { //if it is, 
            this.ChangeFactionTo(ogFaction);  //transfer the character to his original faction
            if (ogFaction.id == FactionManager.Instance.neutralFaction.id) { //if the character's original faction is the neutral faction
                if (ogHome.owner == null && !ogHome.IsResidentsFull()) { //check if his original home is still unowned
                    //if it is and it has not reached it's resident capacity, return him to his original home
                    MigrateHomeTo(ogHome);
                } else { //if it does not meet those requirements
                    //check if the neutral faction still has any available areas that have not reached capacity yet
                    List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
                    if (validNeutralAreas.Count > 0) {
                        //if it does, pick randomly from those
                        Area chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if not, keep the characters current home
                }
            } else { //if it is not, check if his original home is still owned by that faction and it has not yet reached it's resident capacity
                if (ogHome.owner == ogFaction && !ogHome.IsResidentsFull()) {
                    //if it meets those requirements, return the character's home to that location
                    MigrateHomeTo(ogHome);
                } else { //if not, get another area owned by his faction that has not yet reached capacity
                    List<Area> validAreas = ogFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
                    if (validAreas.Count > 0) {
                        Area chosenArea = validAreas[UnityEngine.Random.Range(0, validAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if there are still no areas that can be his home, keep his current one.
                }
            }
        } else { //if not
            //transfer the character to the neutral faction instead
            this.ChangeFactionTo(FactionManager.Instance.neutralFaction);
            List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
            if (validNeutralAreas.Count > 0) {  //then check if the neutral faction has any owned areas that have not yet reached area capacity
                //if it does, pick from any of those, then set it as the characters new home
                Area chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                MigrateHomeTo(chosenArea);
            }
            //if it does not, keep the characters current home
        }
    }
    //public void MoveToStructure(LocationStructure structure) {
    //    if (this.currentStructure != null) {
    //        this.currentStructure.RemoveCharacterAtLocation(this);
    //    }
    //    structure.AddCharacterAtLocation(this);
    //}
    public override string ToString() {
        return name;
    }
    #endregion

    #region Relationships
    private void AddRelationship(Character character, RelationshipTrait newRel) {
        if (!relationships.ContainsKey(character)) {
            relationships.Add(character, new List<RelationshipTrait>());
        }
        relationships[character].Add(newRel);
        OnRelationshipWithCharacterAdded(character);
    }
    private void RemoveRelationship(Character character) {
        if (relationships.ContainsKey(character)) {
            relationships.Remove(character);
        }
    }
    private void RemoveRelationship(Character character, RelationshipTrait rel) {
        if (relationships.ContainsKey(character)) {
            relationships[character].Remove(rel);

            if (relationships[character].Count == 0) {
                RemoveRelationship(character);
            }
        }
    }
    public RelationshipTrait GetRelationshipTraitWith(Character character, RELATIONSHIP_TRAIT type) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].Count; i++) {
                RelationshipTrait relTrait = relationships[character][i];
                if (relTrait.relType == type) {
                    return relTrait;
                }
            }
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipTraitWith(Character character) {
        if (relationships.ContainsKey(character)) {
            return relationships[character];
        }
        return null;
    }
    public List<Character> GetCharactersWithRelationship(RELATIONSHIP_TRAIT type) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<Character, List<RelationshipTrait>> kvp in relationships) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (kvp.Value[i].relType == type) {
                    characters.Add(kvp.Key);
                    break;
                }
            }
        }
        return characters;
    }
    public Character GetCharacterWithRelationship(RELATIONSHIP_TRAIT type) {
        foreach (KeyValuePair<Character, List<RelationshipTrait>> kvp in relationships) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (kvp.Value[i].relType == type) {
                    return kvp.Key;
                }
            }
        }
        return null;
    }
    public bool CanHaveRelationshipWith(RELATIONSHIP_TRAIT type, Character target) {
        switch (type) {
            case RELATIONSHIP_TRAIT.LOVER:
            case RELATIONSHIP_TRAIT.PARAMOUR:
                //- **Lover:** Positive, Permanent (Can only have 1)
                //- **Paramour:** Positive, Transient (Can only have 1)
                if (GetCharacterWithRelationship(type) == null) {
                    Character rel = target.GetCharacterWithRelationship(type);
                    if (rel == null || rel.id == this.id) {
                        return true;
                    }
                }
                return false;
            case RELATIONSHIP_TRAIT.MASTER:
            case RELATIONSHIP_TRAIT.SERVANT:
                //check if this character is not already a master or a servant and if the target character is also not already a master or a servant
                if (GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER) == null && GetCharacterWithRelationship(RELATIONSHIP_TRAIT.SERVANT) == null
                    && target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER) == null && target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.SERVANT) == null) {
                    return true;
                }
                return false;
            case RELATIONSHIP_TRAIT.MENTOR:
            case RELATIONSHIP_TRAIT.STUDENT:
                //check if this character is not already a mentor or a student
                if (GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MENTOR) == null && GetCharacterWithRelationship(RELATIONSHIP_TRAIT.STUDENT) == null
                    && target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MENTOR) == null && target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.STUDENT) == null) {
                    return true;
                }
                return false;
        }
        return true;
    }
    private void OnRelationshipWithCharacterAdded(Character targetCharacter) {
        //check if they share the same home, then migrate them accordingly
        if (this.homeArea.id == targetCharacter.homeArea.id) {
            homeArea.AssignCharacterToDwellingInArea(this);
            homeArea.AssignCharacterToDwellingInArea(targetCharacter);
        }
    }
    public bool HasRelationshipOfEffectWith(Character character, TRAIT_EFFECT effect) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].Count; i++) {
                RelationshipTrait currTrait = relationships[character][i];
                if (currTrait.effect == effect) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfEffectWith(Character character, List<TRAIT_EFFECT> effect) {
        if (relationships.ContainsKey(character)) {
            for (int i = 0; i < relationships[character].Count; i++) {
                RelationshipTrait currTrait = relationships[character][i];
                if (effect.Contains(currTrait.effect)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfEffect(TRAIT_EFFECT effect) {
        foreach (KeyValuePair<Character, List<RelationshipTrait>> kvp in relationships) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (effect == kvp.Value[i].effect) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfEffect(List<TRAIT_EFFECT> effect) {
        foreach (KeyValuePair<Character, List<RelationshipTrait>> kvp in relationships) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (effect.Contains(kvp.Value[i].effect)) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region History
    public void AddHistory(Log log) {
        if (!_history.Contains(log)) {
            _history.Add(log);
            //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && this.id == UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id) {
            //    Debug.Log("Added log to history of " + this.name + ". " + log.isInspected);
            //}
            if (this._history.Count > 300) {
                this._history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }

    }
    #endregion

    #region Character
    public bool IsHostileWith(Character character) {
        if (this.faction == null) {
            return true; //this character has no faction
        }
        //if (this.currentAction != null && this.currentAction.HasHostilitiesBecauseOfTask(combatInitializer)) {
        //    return true;
        //}
        //Check here if the combatInitializer is hostile with this character, if yes, return true
        Faction factionOfEnemy = character.faction;

        //if (combatInitializer.icharacterType == ICHARACTER_TYPE.CHARACTER) {
        //    factionOfEnemy = (combatInitializer as Character).faction;
        //}else if(combatInitializer is Party) {
        //    factionOfEnemy = (combatInitializer as Party).faction;
        //}
        if (factionOfEnemy != null) {
            if (factionOfEnemy.id == this.faction.id) {
                return false; //characters are of same faction
            }
            FactionRelationship rel = this.faction.GetRelationshipWith(factionOfEnemy);
            if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                return true; //factions of combatants are hostile
            }
            return false;
        } else {
            return true; //enemy has no faction
        }

    }
    public STANCE GetCurrentStance() {
        return STANCE.NEUTRAL;
    }
    #endregion

    #region Combat Handlers
    public void SetIsInCombat(bool state) {
        _isInCombat = state;
    }
    public void SetCombatCharacter(CombatCharacter combatCharacter) {
        _currentCombatCharacter = combatCharacter;
    }
    #endregion

    #region Portrait Settings
    public void SetPortraitSettings(PortraitSettings settings) {
        _portraitSettings = settings;
    }
    #endregion

    #region RPG
    private bool hpMagicRangedStatMod;
    public void LevelUp() {
        //Only level up once per day
        //if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
        //    return;
        //}
        //_lastLevelUpDay = GameManager.Instance.continuousDays;
        if (_level < CharacterManager.Instance.maxLevel) {
            _level += 1;
            //Add stats per level from class
            if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
                AdjustAttackMod(_characterClass.attackPowerPerLevel);
                AdjustSpeedMod(_characterClass.speedPerLevel);
                AdjustMaxHPMod(_characterClass.hpPerLevel);
            } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
                if (_level % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel);
                }
                AdjustSpeedMod(_characterClass.speedPerLevel);
            } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel);
                }
                if ((_level - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
                AdjustSpeedMod(_characterClass.speedPerLevel);
            }

            //Reset to full health and sp
            ResetToFullHP();

            if(_playerCharacterItem != null) {
                _playerCharacterItem.UpdateMinionItem();
                Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
            }
        }
    }
    public void LevelUp(int amount) {
        //Only level up once per day
        //if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
        //    return;
        //}
        //_lastLevelUpDay = GameManager.Instance.continuousDays;
        int supposedLevel = _level + amount;
        if (supposedLevel > CharacterManager.Instance.maxLevel) {
            amount = CharacterManager.Instance.maxLevel - level;
        } else if (supposedLevel < 0) {
            amount -= (supposedLevel - 1);
        }
        //Add stats per level from class
        if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
            AdjustAttackMod(_characterClass.attackPowerPerLevel * amount);
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
            AdjustMaxHPMod(_characterClass.hpPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            for (int i = 0; i < range; i++) {
                if (i % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            for (int i = _level; i <= _level + range; i++) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                }
                if (i != 1 && (i - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        }
        _level += amount;

        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        if (_playerCharacterItem != null) {
            _playerCharacterItem.UpdateMinionItem();
        }
    }
    public void SetLevel(int amount) {
        int previousLevel = _level;
        _level = amount;
        if (_level < 1) {
            _level = 1;
        }else if (_level > CharacterManager.Instance.maxLevel) {
            _level = CharacterManager.Instance.maxLevel;
        }

        //Add stats per level from class
        int difference = _level - previousLevel;
        if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
            AdjustAttackMod(_characterClass.attackPowerPerLevel * difference);
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
            AdjustMaxHPMod(_characterClass.hpPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            for (int i = 0; i < range; i++) {
                if (i % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            for (int i = _level; i <= _level + range; i++) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                }
                if (i != 1 && (i - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        }

        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        if (_playerCharacterItem != null) {
            _playerCharacterItem.UpdateMinionItem();
        }

        //Reset Experience
        //_experience = 0;
        //RecomputeMaxExperience();
    }
    public void OnCharacterClassChange() {
        if (_currentHP > _maxHPMod) {
            _currentHP = _maxHPMod;
        }
        if (_sp > _maxSP) {
            _sp = _maxSP;
        }
    }
    public void AdjustExperience(int amount) {
        _experience += amount;
        if (_experience >= _maxExperience) {
            _experience = 0;
            //LevelUp();
        }
    }
    public void AdjustElementalWeakness(ELEMENT element, float amount) {
        _elementalWeaknesses[element] += amount;
    }
    public void AdjustElementalResistance(ELEMENT element, float amount) {
        _elementalResistances[element] += amount;
    }
    public void AdjustSP(int amount) {
        _sp += amount;
        _sp = Mathf.Clamp(_sp, 0, _maxSP);
    }
    private void RecomputeMaxExperience() {
        _maxExperience = Mathf.CeilToInt(100f * ((Mathf.Pow((float) _level, 1.25f)) / 1.1f));
    }
    public void ResetToFullHP() {
        SetHP(maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    private float GetComputedPower() {
        float compPower = 0f;
        for (int i = 0; i < currentParty.characters.Count; i++) {
            compPower += currentParty.characters[i].attackPower;
        }
        return compPower;
    }
    public void SetHP(int amount) {
        this._currentHP = amount;
    }
    public void SetMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPMod = amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustAttackMod(int amount) {
        _attackPowerMod += amount;
    }
    public void AdjustAttackPercentMod(int amount) {
        _attackPowerPercentMod += amount;
    }
    public void AdjustMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPMod += amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustMaxHPPercentMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPPercentMod += amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustSpeedMod(int amount) {
        _speedMod += amount;
    }
    public void AdjustSpeedPercentMod(int amount) {
        _speedPercentMod += amount;
    }
    public bool IsHealthFull() {
        return _currentHP >= maxHP;
    }
    #endregion

    #region Home
    public void SetHome(Area newHome) {
        this.homeArea = newHome;
    }
    public void SetHomeStructure(Dwelling homeStructure) {
        this.homeStructure = homeStructure;
    }
    public bool IsLivingWith(RELATIONSHIP_TRAIT type) {
        if (homeStructure.residents.Count > 1) {
            Character relTarget = GetCharacterWithRelationship(type);
            if (homeStructure.residents.Contains(relTarget)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Work
    //public bool LookForNewWorkplace() {
    //    if (_characterClass.workActionType == ACTION_TYPE.WORKING) {
    //        _workplace = _homeLandmark;
    //        return true;
    //    } else {
    //        List<BaseLandmark> workplaceChoices = new List<BaseLandmark>();
    //        for (int i = 0; i < _homeLandmark.tileLocation.areaOfTile.landmarks.Count; i++) {
    //            StructureObj structure = _homeLandmark.tileLocation.areaOfTile.landmarks[i].landmarkObj;
    //            for (int j = 0; j < structure.currentState.actions.Count; j++) {
    //                if (structure.currentState.actions[j].actionType == _characterClass.workActionType) {
    //                    workplaceChoices.Add(_homeLandmark.tileLocation.areaOfTile.landmarks[i]);
    //                    break;
    //                }
    //            }
    //        }
    //        if (workplaceChoices.Count != 0) {
    //            _workplace = workplaceChoices[UnityEngine.Random.Range(0, workplaceChoices.Count)];
    //            return true;
    //        }
    //        //throw new Exception("Could not find workplace for " + this.name);
    //    }
    //    return false;
    //}
    //public void MigrateTo(BaseLandmark newHomeLandmark) {
    //    Area previousHome = null;
    //    if(homeArea != null) {
    //        previousHome = homeArea.tileLocation.areaOfTile;
    //        homeArea.RemoveCharacterHomeOnLandmark(this);
    //    }
    //    newHomeLandmark.AddCharacterHomeOnLandmark(this);
    //    Messenger.Broadcast(Signals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeLandmark.tileLocation.areaOfTile);
    //}
    public void MigrateHomeTo(Area newHomeArea, bool broadcast = true) {
        Area previousHome = null;
        if (homeArea != null) {
            previousHome = homeArea;
            homeArea.RemoveResident(this);
        }
        newHomeArea.AddResident(this);
        if (broadcast) {
            Messenger.Broadcast(Signals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeArea);
        }
    }
    public void MigrateHomeStructureTo(Dwelling dwelling) {
        if (this.homeStructure != null) {
            if (this.homeStructure == dwelling) {
                return; //ignore change
            }
            //remove character from his/her old home
            this.homeStructure.RemoveResident(this);
        }
        dwelling.AddResident(this);
    }
    private void OnCharacterMigratedHome(Character character, Area previousHome, Area homeArea) {
        if (character.id != this.id && this.homeArea.id == homeArea.id) {
            if (GetAllRelationshipTraitWith(character) != null) {
                this.homeArea.AssignCharacterToDwellingInArea(this); //redetermine home, in case new character with relationship has moved area to same area as this character
            }
        }
    }
    #endregion

    #region IInteractable
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
        if (_currentParty.icon != null) {
            _currentParty.icon.UpdateVisualState();
        }
        //if (_currentParty.specificLocation != null && _currentParty.specificLocation.coreTile.landmarkOnTile != null) {
        //    _currentParty.specificLocation.coreTile.landmarkOnTile.landmarkVisual.ToggleCharactersVisibility();
        //}
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = state;
    }
    public void EndedInspection() {
        
    }
    public void AddInteraction(Interaction interaction) {
        //_currentInteractions.Add(interaction);
        if (interaction == null) {
            throw new Exception("Something is trying to add null interaction");
        }
        interaction.SetCharacterInvolved(this);
        interaction.interactable.AddInteraction(interaction);
        //interaction.Initialize(this);
        //Messenger.Broadcast(Signals.ADDED_INTERACTION, this as IInteractable, interaction);
    }
    public void RemoveInteraction(Interaction interaction) {
        //if (_currentInteractions.Remove(interaction)) {
        interaction.interactable.RemoveInteraction(interaction);
        //Messenger.Broadcast(Signals.REMOVED_INTERACTION, this as IInteractable, interaction);
        //}
    }
    #endregion

    #region Defender
    public void OnSetAsDefender(Area defending) {
        defendingArea = defending;
        //this.ownParty.specificLocation.RemoveCharacterFromLocation(this.ownParty, false);
        //ownParty.SetSpecificLocation(defending.coreTile.landmarkOnTile);
    }
    public void OnRemoveAsDefender() {
        //defendingArea.coreTile.landmarkOnTile.AddCharacterToLocation(this.ownParty);
        defendingArea = null;
    }
    public bool IsDefending(BaseLandmark landmark) {
        if (defendingArea != null && defendingArea.id == landmark.id) {
            return true;
        }
        return false;
    }
    #endregion

    #region Traits
    public void CreateInitialTraitsByClass() {
        //Attack Type
        if (characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
            AddTrait(AttributeManager.Instance.allTraits["Physical Attacker"]);
        } else if (characterClass.attackType == ATTACK_TYPE.MAGICAL) {
            AddTrait(AttributeManager.Instance.allTraits["Magic User"]);
        }

        //Range Type
        if (characterClass.rangeType == RANGE_TYPE.MELEE) {
            AddTrait(AttributeManager.Instance.allTraits["Melee Attack"]);
        } else if (characterClass.rangeType == RANGE_TYPE.RANGED) {
            AddTrait(AttributeManager.Instance.allTraits["Ranged Attack"]);
        }

        //Combat Position
        if (characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
            AddTrait(AttributeManager.Instance.allTraits["Frontline Combatant"]);
        } else if (characterClass.combatPosition == COMBAT_POSITION.BACKLINE) {
            AddTrait(AttributeManager.Instance.allTraits["Backline Combatant"]);
        }

        //Class Name
        if (characterClass.className == "Knight" || characterClass.className == "Marauder" || characterClass.className == "Barbarian") {
            AddTrait(AttributeManager.Instance.allTraits["Melee Trait"]);
        } else if (characterClass.className == "Stalker" || characterClass.className == "Archer" || characterClass.className == "Hunter") {
            AddTrait(AttributeManager.Instance.allTraits["Ranged Trait"]);
        } else if (characterClass.className == "Druid" || characterClass.className == "Mage" || characterClass.className == "Shaman") {
            AddTrait(AttributeManager.Instance.allTraits["Magic Trait"]);
        } else if (characterClass.className == "Spinner" || characterClass.className == "Abomination") {
            AddTrait(AttributeManager.Instance.allTraits["Melee Vulnerable"]);
        } else if (characterClass.className == "Ravager") {
            AddTrait(AttributeManager.Instance.allTraits["Ranged Vulnerable"]);
        } else if (characterClass.className == "Dragon") {
            AddTrait(AttributeManager.Instance.allTraits["Dragon Trait"]);
        } else if (characterClass.className == "Greed") {
            AddTrait(AttributeManager.Instance.allTraits["Greed Trait"]);
        } else if (characterClass.className == "Lust") {
            AddTrait(AttributeManager.Instance.allTraits["Lust Trait"]);
        } else if (characterClass.className == "Envy") {
            AddTrait(AttributeManager.Instance.allTraits["Envy Trait"]);
        }

        //Random Traits
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 10) {
            AddTrait(new Craftsman());
        }
    }
    public void AddTrait(Trait trait) {
        if (trait.IsUnique() && GetTrait(trait.name) != null) {
            return;
        }
        //if (trait is RelationshipTrait) {
        //    RelationshipTrait rt = trait as RelationshipTrait;
        //    if (!CanHaveRelationshipWith(rt.relType, rt.targetCharacter)) {
        //        Debug.LogWarning("Cannot have " + rt.relType.ToString() + " relationship with " + rt.targetCharacter.name + ". Ignoring adding it");
        //        return;
        //    }
        //}
        _traits.Add(trait);
        ApplyTraitEffects(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddDays(trait.daysDuration);
            SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait));
        }
        trait.OnAddTrait(this);
        Messenger.Broadcast(Signals.TRAIT_ADDED, this);
        if (trait is RelationshipTrait) {
            RelationshipTrait rel = trait as RelationshipTrait;
            AddRelationship(rel.targetCharacter, rel);
        }
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true) {
        if (_traits.Remove(trait)) {
            UnapplyTraitEffects(trait);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this);
            }
            Messenger.Broadcast(Signals.TRAIT_REMOVED, this);
            if (trait is RelationshipTrait) {
                RelationshipTrait rel = trait as RelationshipTrait;
                RemoveRelationship(rel.targetCharacter, rel);
            }
            return true;
        }
        return false;
    }
    public Trait GetTrait(string traitName) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName) {
                return _traits[i];
            }
        }
        return null;
    }
    public Trait GetTraitOr(string traitName1, string traitName2) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName1 || _traits[i].name == traitName2) {
                return _traits[i];
            }
        }
        return null;
    }
    public bool HasTraitOf(TRAIT_TYPE traitType) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == traitType) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        for (int i = 0; i < traits.Count; i++) {
            Trait currTrait = traits[i];
            if (currTrait.effect == effect && currTrait.type == type) {
                return true;
            }
        }
        return false;
    }
    public List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        List<Trait> removedTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == traitType) {
                removedTraits.Add(_traits[i]);
                _traits.RemoveAt(i);
                i--;
            }
        }
        return removedTraits;
    }
    public Trait GetRandomNegativeTrait() {
        List<Trait> negativeTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].effect == TRAIT_EFFECT.NEGATIVE) {
                negativeTraits.Add(_traits[i]);
            }
        }
        if (negativeTraits.Count > 0) {
            return negativeTraits[UnityEngine.Random.Range(0, negativeTraits.Count)];
        }
        return null;
    }
    private void ApplyTraitEffects(Trait trait) {
        if(trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(1);
        }
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackPercentMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPPercentMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedPercentMod((int) traitEffect.amount);
                    }
                } else {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedMod((int) traitEffect.amount);
                    }
                }
            }
        }
    }
    private void UnapplyTraitEffects(Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(-1);
        }
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackPercentMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPPercentMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedPercentMod(-(int) traitEffect.amount);
                    }
                } else {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedMod(-(int) traitEffect.amount);
                    }
                }
            }
        }
    }
    private void SetTraitsFromClass() {
        if (_characterClass.traitNames != null) {
            for (int i = 0; i < _characterClass.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_characterClass.traitNames[i]];
                AddTrait(trait);
            }
        }
    }
    private void SetTraitsFromRace() {
        if (_raceSetting.traitNames != null) {
            for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_raceSetting.traitNames[i]];
                AddTrait(trait);
            }
        }
    }
    public Friend GetFriendTraitWith(Character character) {
        for (int i = 0; i < _traits.Count; i++) {
            if(_traits[i] is Friend) {
                Friend friendTrait = _traits[i] as Friend;
                if(friendTrait.targetCharacter.id == character.id) {
                    return friendTrait;
                }
            }
        }
        return null;
    }
    public Enemy GetEnemyTraitWith(Character character) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i] is Enemy) {
                Enemy enemyTrait = _traits[i] as Enemy;
                if (enemyTrait.targetCharacter == character) {
                    return enemyTrait;
                }
            }
        }
        return null;
    }
    public void GenerateRandomTraits() {
        //All characters have a 1 in 8 chance of having Crooked trait when spawned
        if (UnityEngine.Random.Range(0, 8) < 1) {
            AddTrait(AttributeManager.Instance.allTraits["Crooked"]);
            //Debug.Log(this.name + " is set to be Crooked");
        }
    }
    public bool ReleaseFromAbduction() {
        Trait trait = GetTrait("Abducted");
        if (trait != null) {
            Abducted abductedTrait = trait as Abducted;
            RemoveTrait(abductedTrait);
            ReturnToOriginalHomeAndFaction(abductedTrait.originalHome, this.faction);
            //MigrateTo(abductedTrait.originalHomeLandmark);

            Interaction interactionAbducted = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, specificLocation);
            InduceInteraction(interactionAbducted);
            return true;
        }
        return false;
    }
    public SpecialToken CraftAnItem() {
        Craftsman craftsmanTrait = GetTrait("Craftsman") as Craftsman;
        if(craftsmanTrait != null) {
            //SpecialTokenSettings settings = TokenManager.Instance.GetTokenSettings(craftsmanTrait.craftedItemName);
            return TokenManager.Instance.CreateSpecialToken(craftsmanTrait.craftedItemName); //, settings.appearanceWeight
        }
        return null;
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Minion
    public void SetMinion(Minion minion) {
        _minion = minion;
        UnsubscribeSignals();
    }
    public void RecruitAsMinion() {
        if (!IsInOwnParty()) {
            _currentParty.RemoveCharacter(this);
        }
        MigrateHomeTo(PlayerManager.Instance.player.playerArea);

        specificLocation.RemoveCharacterFromLocation(this.currentParty);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(this.currentParty);

        ChangeFactionTo(PlayerManager.Instance.player.playerFaction);

        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(this);
        PlayerManager.Instance.player.AddMinion(newMinion);

        SetForcedInteraction(null);

        if (!characterToken.isObtainedByPlayer) {
            PlayerManager.Instance.player.AddToken(characterToken);
        }
    }
    #endregion

    #region Buffs
    public void ConstructBuffs() {
        _buffs = new Dictionary<STAT, float>();
        STAT[] stats = Utilities.GetEnumValues<STAT>();
        for (int i = 0; i < stats.Length; i++) {
            _buffs.Add(stats[i], 0f);
        }
    }
    public void AddBuff(Buff buff) {
        if (_buffs.ContainsKey(buff.buffedStat)) {
            _buffs[buff.buffedStat] += buff.percentage;
        }
    }
    public void RemoveBuff(Buff buff) {
        if (_buffs.ContainsKey(buff.buffedStat)) {
            _buffs[buff.buffedStat] -= buff.percentage;
        }
    }
    #endregion

    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        _playerCharacterItem = item;
    }

    #region Interaction
    private int GetMonthInteractionTick() {
        int daysInMonth = GameManager.daysInMonth[GameManager.Instance.month];
        int remainingDaysInMonth = GameManager.Instance.continuousDays % daysInMonth;
        int startDay = GameManager.Instance.continuousDays + remainingDaysInMonth + 1;
        return UnityEngine.Random.Range(startDay, startDay + daysInMonth);
    }
    public void DisableInteractionGeneration() {
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyInteractionGeneration);
    }
    public void AddInteractionWeight(INTERACTION_TYPE type, int weight) {
        interactionWeights.AddElement(type, weight);
    }
    public void RemoveInteractionFromWeights(INTERACTION_TYPE type, int weight) {
        interactionWeights.RemoveElement(type);
    }
    public void SetDailyInteractionGenerationTick() {
        if(specificLocation == null || specificLocation.id == homeArea.id) {
            _currentInteractionTick = GetMonthInteractionTick();
        } else {
            int remainingDaysInWeek = GameManager.Instance.continuousDays % 7;
            int startDay = GameManager.Instance.continuousDays + remainingDaysInWeek + 1;
            _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + 7);
        }
    }
    public void SetDailyInteractionGenerationTick(int tick) {
        _currentInteractionTick = tick;
    }
    public void DailyInteractionGeneration() {
        if (_currentInteractionTick == GameManager.Instance.continuousDays) {
            //if(job.jobType != JOB.NONE) {
            //    job.CreateRandomInteractionForNonMinionCharacters();
            //}
            GenerateDailyInteraction();
            SetDailyInteractionGenerationTick();
        } 
        //else if (_currentInteractionTick > GameManager.Instance.continuousDays) {
        //    SetDailyInteractionGenerationTick();
        //}
    }
    public void GenerateDailyInteraction() {
        if (!IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || _job == null) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if (job.jobType == JOB.NONE) {
            return;
            //_job.CreateRandomInteractionForNonMinionCharacters();
        }
        string interactionLog = GameManager.Instance.TodayLogString() + "Generating daily interaction for " + this.name;
        if (_forcedInteraction != null) {
            interactionLog += "\nUsing forced interaction: " + _forcedInteraction.type.ToString();
            AddInteraction(_forcedInteraction);
            //if(_forcedInteraction.CanInteractionBeDoneBy(this)) {
            //      AddInteraction(_forcedInteraction);
            //} else {
            //    Interaction unable = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.UNABLE_TO_PERFORM, this.specificLocation.coreTile.landmarkOnTile);
            //    AddInteraction(unable);
            //    interactionLog += "\nCan't do forced interaction: " + _forcedInteraction.type.ToString();
            //}
            _forcedInteraction = null;
        } else {
            if(specificLocation.id != homeArea.id) {
                //Character actions away from home
                WeightedDictionary<string> awayFromHomeInteractionWeights = new WeightedDictionary<string>();
                awayFromHomeInteractionWeights.AddElement("DoNothing", 50);

                if (tokenInInventory != null && tokenInInventory.npcAssociatedInteractionType != INTERACTION_TYPE.NONE && tokenInInventory.CanBeUsedBy(this) && InteractionManager.Instance.CanCreateInteraction(tokenInInventory.npcAssociatedInteractionType, this)) {
                    awayFromHomeInteractionWeights.AddElement(tokenInInventory.tokenName, 70);
                }

                foreach (KeyValuePair<INTERACTION_TYPE, int> kvp in CharacterManager.Instance.awayFromHomeInteractionWeights) {
                    if (InteractionManager.Instance.CanCreateInteraction(kvp.Key, this)) {
                        awayFromHomeInteractionWeights.AddElement(kvp.Key.ToString(), kvp.Value); //15
                    }
                }

                string result = awayFromHomeInteractionWeights.PickRandomElementGivenWeights();
                if(result == "DoNothing") {
                }else if (tokenInInventory != null && result == tokenInInventory.tokenName) {
                    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(tokenInInventory.npcAssociatedInteractionType, specificLocation);
                    if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_CHARACTER) {
                        (interaction as UseItemOnCharacter).SetItemToken(tokenInInventory);
                    } else if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_SELF) {
                        (interaction as UseItemOnSelf).SetItemToken(tokenInInventory);
                    } else if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_LOCATION) {
                        (interaction as UseItemOnLocation).SetItemToken(tokenInInventory);
                    }
                    AddInteraction(interaction);
                } else {
                    INTERACTION_TYPE interactionType = (INTERACTION_TYPE) Enum.Parse(typeof(INTERACTION_TYPE), result);
                    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(interactionType, specificLocation);
                    AddInteraction(interaction);
                }
            } else {
                //Character actions at home
                WeightedDictionary<string> atHomeInteractionWeights = new WeightedDictionary<string>();
                atHomeInteractionWeights.AddElement("DoNothing", 100);
                foreach (KeyValuePair<INTERACTION_TYPE, int> kvp in CharacterManager.Instance.atHomeInteractionWeights) {
                    if (InteractionManager.Instance.CanCreateInteraction(kvp.Key, this)) {
                        atHomeInteractionWeights.AddElement(kvp.Key.ToString(), kvp.Value); //15
                    }
                }
                if (tokenInInventory != null) {
                    if (tokenInInventory.npcAssociatedInteractionType != INTERACTION_TYPE.NONE && tokenInInventory.CanBeUsedBy(this) && InteractionManager.Instance.CanCreateInteraction(tokenInInventory.npcAssociatedInteractionType, this)) {
                        atHomeInteractionWeights.AddElement(tokenInInventory.tokenName, 70);
                    } else if(tokenInInventory.npcAssociatedInteractionType == INTERACTION_TYPE.USE_ITEM_ON_SELF) {
                        atHomeInteractionWeights.AddElement("ItemNotUsable", 70);
                    }
                } else {
                    for (int i = 0; i < specificLocation.possibleSpecialTokenSpawns.Count; i++) {
                        if(specificLocation.possibleSpecialTokenSpawns[i].owner == this.faction) {
                            atHomeInteractionWeights.AddElement("PickUp", 70);
                            break;
                        }
                    }
                }
                string result = atHomeInteractionWeights.PickRandomElementGivenWeights();

                if (result == "DoNothing") {
                }   else if (result == "ItemNotUsable") {
                    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.DROP_ITEM, specificLocation);
                    AddInteraction(interaction);
                } else if (result == "PickUp") {
                    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.PICK_ITEM, specificLocation);
                    AddInteraction(interaction);
                } else if (tokenInInventory != null && result == tokenInInventory.tokenName) {
                    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(tokenInInventory.npcAssociatedInteractionType, specificLocation);
                    if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_CHARACTER) {
                        (interaction as UseItemOnCharacter).SetItemToken(tokenInInventory);
                    } else if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_SELF) {
                        (interaction as UseItemOnSelf).SetItemToken(tokenInInventory);
                    } else if (interaction.type == INTERACTION_TYPE.USE_ITEM_ON_LOCATION) {
                        (interaction as UseItemOnLocation).SetItemToken(tokenInInventory);
                    }
                    AddInteraction(interaction);
                } else {
                    INTERACTION_TYPE interactionType = (INTERACTION_TYPE) Enum.Parse(typeof(INTERACTION_TYPE), result);
                    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(interactionType, specificLocation);
                    AddInteraction(interaction);
                }
            }



            //int chance = UnityEngine.Random.Range(0, 100);
            //if(chance >= 15) {
            //    //Character will not perform
            //    return;
            //}
            //WeightedDictionary<INTERACTION_TYPE> validInteractions = GetValidInteractionWeights();
            //if (validInteractions != null) {
            //    if (validInteractions.GetTotalOfWeights() > 0) {
            //        interactionLog += "\n" + validInteractions.GetWeightsSummary("Generating interaction:");
            //        INTERACTION_TYPE chosenInteraction = validInteractions.PickRandomElementGivenWeights();
            //        //create interaction of type
            //        BaseLandmark interactable = specificLocation.coreTile.landmarkOnTile;
            //        if (interactable == null) {
            //            throw new Exception(GameManager.Instance.TodayLogString() + this.name + "'s specific location (" + specificLocation.locationName + ") is not a landmark!");
            //        }
            //        Interaction createdInteraction = InteractionManager.Instance.CreateNewInteraction(chosenInteraction, specificLocation.coreTile.landmarkOnTile);

            //        if (job.jobType == JOB.LEADER) {
            //            //For Faction Upgrade Interaction Only
            //            Area area = _homeLandmark.tileLocation.areaOfTile;
            //            area.AdjustSuppliesInBank(-100);
            //            createdInteraction.SetMinionSuccessAction(() => area.AdjustSuppliesInBank(100));
            //        }

            //        AddInteraction(createdInteraction);
            //    } else {
            //        interactionLog += "\nCannot generate interaction because weights are not greater than zero";
            //    }
            //} else {
            //    interactionLog += "\nCannot generate interaction because there are no interactions for job: " + job.jobType.ToString();
            //}
        }
        //Debug.Log(interactionLog);
    }
    public void SetForcedInteraction(Interaction interaction) {
        _forcedInteraction = interaction;
    }
    public void InduceInteraction(Interaction interaction) {
        SetForcedInteraction(interaction);
        SetDailyInteractionGenerationTick(GameManager.Instance.continuousDays + 1);
    }
    //private void DefaultAllExistingInteractions() {
    //    for (int i = 0; i < _currentInteractions.Count; i++) {
    //        if (!_currentInteractions[i].hasActivatedTimeOut) {
    //            _currentInteractions[i].TimedOutRunDefault();
    //            i--;
    //        }
    //    }
    //}
    public Interaction GetInteractionOfType(INTERACTION_TYPE type) {
        for (int i = 0; i < _currentInteractions.Count; i++) {
            Interaction currInteraction = _currentInteractions[i];
            if (currInteraction.type == type) {
                return currInteraction;
            }
        }
        return null;
    }
    private WeightedDictionary<INTERACTION_TYPE> GetValidInteractionWeights() {
        List<CharacterInteractionWeight> jobInteractions = InteractionManager.Instance.GetJobNPCInteractionWeights(job.jobType);
        WeightedDictionary<INTERACTION_TYPE> weights = new WeightedDictionary<INTERACTION_TYPE>();
        if (jobInteractions != null) {
            for (int i = 0; i < jobInteractions.Count; i++) {
                if (GetInteractionOfType(jobInteractions[i].interactionType) == null && InteractionManager.Instance.CanCreateInteraction(jobInteractions[i].interactionType, this)) {
                    weights.AddElement(jobInteractions[i].interactionType, jobInteractions[i].weight);
                }
            }
        }
        if (InteractionManager.Instance.CanCreateInteraction(INTERACTION_TYPE.RETURN_HOME, this)) {
            weights.AddElement(INTERACTION_TYPE.RETURN_HOME, 10);
        }
        return weights;
    }
    private void AddDefaultInteractions() {
        List<CharacterInteractionWeight> defaultInteractions = InteractionManager.Instance.GetDefaultInteractionWeightsForRole(this.role.roleType);
        if (defaultInteractions != null) {
            for (int i = 0; i < defaultInteractions.Count; i++) {
                CharacterInteractionWeight currWeight = defaultInteractions[i];
                interactionWeights.AddElement(currWeight.interactionType, currWeight.weight);
            }
        }
    }
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.LEVEL:
            LevelUp(reward.amount);
            break;
            case REWARD.SUPPLY:
            if(minion != null) {
                PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, reward.amount);
            } else {
                homeArea.AdjustSuppliesInBank(reward.amount);
            }
            break;
            default:
            break;
        }
    }
    #endregion

    #region Token Inventory
    public void ObtainToken(SpecialToken token) {
        SetToken(token);
        token.SetOwner(this.faction);
        token.OnObtainToken(this);
        //token.AdjustQuantity(-1);
    }
    public void UnobtainToken() {
        tokenInInventory.OnUnobtainToken(this);
        SetToken(null);
    }
    public void ConsumeToken() {
        tokenInInventory.OnConsumeToken(this);
        SetToken(null);
    }
    private void SetToken(SpecialToken token) {
        tokenInInventory = token;
    }
    public void DropToken(Area location, LocationStructure structure) {
        if (tokenInInventory != null) {
            location.AddSpecialTokenToLocation(tokenInInventory, structure);
            UnobtainToken();
        }
    }
    public void PickUpToken(SpecialToken token, Area location) {
        if (tokenInInventory == null) {
            location.RemoveSpecialTokenFromLocation(token);
            ObtainToken(token);
        }
    }
    public void PickUpRandomToken(Area location) {
        if (tokenInInventory == null) {
            WeightedDictionary<SpecialToken> pickWeights = new WeightedDictionary<SpecialToken>();
            for (int i = 0; i < location.possibleSpecialTokenSpawns.Count; i++) {
                SpecialToken token = location.possibleSpecialTokenSpawns[i];
                if(token.npcAssociatedInteractionType != INTERACTION_TYPE.USE_ITEM_ON_SELF) {
                    pickWeights.AddElement(token, 60);
                } else if(token.CanBeUsedBy(this)) {
                    pickWeights.AddElement(token, 100);
                }
            }
            if(pickWeights.Count > 0) {
                SpecialToken chosenToken = pickWeights.PickRandomElementGivenWeights();
                PickUpToken(chosenToken, location);
            }
        }
    }
    private void UpdateTokenOwner() {
        if (tokenInInventory != null) {
            tokenInInventory.SetOwner(this.faction);
        }
    }
    #endregion
}
