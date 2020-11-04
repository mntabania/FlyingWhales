using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Monster : ICharacter {
    //Serialized fields
    private string _name;
    private MONSTER_TYPE _type;
    private MONSTER_CATEGORY _category;
    private int _level;
    private int _experienceDrop;
    private int _maxHP;
    private int _maxSP;
    private int _attackPower;
    private int _speed;
    private int _pDef;
    private int _mDef;
    private float _dodgeChance;
    private float _hitChance;
    private float _critChance;
    private List<Skill> _skills;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistances;
    //To add item drops and their chances

    private string _characterColorCode;
    private int _id;
    private int _currentHP;
    private int _currentSP;
    private int _actRate;
    private int _currentRow;
    private bool _isDead;
    private Color _characterColor;
    private Combat _currentCombat;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private SIDES _currentSide;
    private List<BodyPart> _bodyParts;


    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + "]" + "<color=" + this._characterColorCode + ">" + this._name + "</link>"; }
    }
    public int id {
        get { return _id; }
    }
    public int attackPower {
        get { return _attackPower; }
    }
    public int actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public int strength {
        get { return 0; }
    }
    public int intelligence {
        get { return 0; }
    }
    public int agility {
        get { return _speed; }
    }
    public int baseAgility {
        get { return _speed; }
    }
    public int level {
        get { return _level; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public int maxHP {
        get { return _maxHP; }
    }
    public int currentRow {
        get { return _currentRow; }
    }
    public int currentSP {
        get { return _currentSP; }
    }
    public float critChance {
        get { return _critChance; }
    }
    public float critDamage {
        get { return 0f; }
    }
    public bool isDead {
        get { return _isDead; }
    }
    public GENDER gender {
        get { return GENDER.MALE; }
    }
    public SIDES currentSide {
        get { return _currentSide; }
    }
    public Combat currentCombat {
        get { return _currentCombat; }
        set { _currentCombat = value; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public List<Skill> skills {
        get { return _skills; }
    }
    public List<BodyPart> bodyParts {
        get { return _bodyParts; }
    }
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    #endregion

    public Monster() {
        _id = Utilities.SetID(this);
        _isDead = false;
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        SetCharacterColor(Color.red);
    }

    public Monster CreateNewCopy() {
        Monster newMonster = new Monster();
        newMonster._name = this._name;
        newMonster._type = this._type;
        newMonster._category = this._category;
        newMonster._level = this._level;
        newMonster._experienceDrop = this._experienceDrop;
        newMonster._maxHP = this._maxHP;
        newMonster._maxSP = this._maxSP;
        newMonster._attackPower = this._attackPower;
        newMonster._speed = this._speed;
        newMonster._pDef = this._pDef;
        newMonster._mDef = this._mDef;
        newMonster._dodgeChance = this._dodgeChance;
        newMonster._hitChance = this._hitChance;
        newMonster._critChance = this._critChance;

        newMonster._skills = new List<Skill>();
        for (int i = 0; i < this._skills.Count; i++) {
            newMonster._skills.Add(_skills[i].CreateNewCopy());
        }
        newMonster._elementalWeaknesses = new Dictionary<ELEMENT, float>(this._elementalWeaknesses);
        newMonster._elementalResistances = new Dictionary<ELEMENT, float>(this._elementalResistances);

        return newMonster;
    }

    public void SetData(MonsterComponent monsterComponent) {
        this._name = monsterComponent.monsterName;
        this._type = monsterComponent.type;
        this._category = monsterComponent.category;
        this._experienceDrop = monsterComponent.experienceDrop;
        this._level = monsterComponent.level;
        this._maxHP = monsterComponent.maxHP;
        this._maxSP = monsterComponent.maxSP;
        this._attackPower = monsterComponent.attackPower;
        this._speed = monsterComponent.speed;
        this._pDef = monsterComponent.pDef;
        this._mDef = monsterComponent.mDef;
        this._dodgeChance = monsterComponent.dodgeChance;
        this._hitChance = monsterComponent.hitChance;
        this._critChance = monsterComponent.critChance;

        this._skills = new List<Skill>();
        for (int i = 0; i < monsterComponent.skillNames.Count; i++) {
            _skills.Add(SkillManager.Instance.allSkills[monsterComponent.skillNames[i]]);
        }

        this._elementalWeaknesses = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < monsterComponent.elementChanceWeaknesses.Count; i++) {
            ElementChance elementChance = monsterComponent.elementChanceWeaknesses[i];
            this._elementalWeaknesses.Add(elementChance.element, elementChance.chance);
        }

        this._elementalResistances = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < monsterComponent.elementChanceResistances.Count; i++) {
            ElementChance elementChance = monsterComponent.elementChanceResistances[i];
            this._elementalResistances.Add(elementChance.element, elementChance.chance);
        }
    }

    #region Utilities
    public void SetCharacterColor(Color color) {
        _characterColor = color;
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    public void Death() {
        _isDead = true;
    }
    #endregion

    #region Interface
    public void SetSide(SIDES side) {
        _currentSide = side;
    }
    public void SetRowNumber(int rowNumber) {
        _currentRow = rowNumber;
    }
    public void AdjustSP(int amount) {
        _currentSP += amount;
        _currentSP = Mathf.Clamp(_currentSP, 0, _maxSP);
    }
    public void AdjustHP(int amount) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                FaintOrDeath();
            }
        }
    }
    public void FaintOrDeath() {
        this.currentCombat.CharacterDeath(this);
        Death();
    }
    public int GetPDef(ICharacter enemy) {
        return _pDef;
    }
    public int GetMDef(ICharacter enemy) {
        return _mDef;
    }
    public void ResetToFullHP() {
        AdjustHP(_maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    #endregion
}
