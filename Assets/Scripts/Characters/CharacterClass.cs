using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterClass {
    [SerializeField] private string _className;
    [SerializeField] private int _baseAttackPower;
    //[SerializeField] private int _attackPowerPerLevel;
    //[SerializeField] private int _baseSpeed; //movement speed
    //[SerializeField] private int _speedPerLevel;
    //[SerializeField] private int _hpPerLevel;
    [SerializeField] private int _baseHP;
    [SerializeField] private int _baseAttackSpeed; //The lower the amount the faster the attack rate
    [SerializeField] private float _attackRange;
    [SerializeField] private int _inventoryCapacity;
    //[SerializeField] private float _runSpeedMod;
    //[SerializeField] private float _walkSpeedMod;

    [SerializeField] private string[] _traitNames;
    [SerializeField] private string[] _interestedItemNames;
    [SerializeField] private STRUCTURE_TYPE[] _relatedStructures;
    [SerializeField] private string _identifier;
    //[SerializeField] private bool _isNormalNonCombatant;
    [SerializeField] private ELEMENTAL_TYPE _elementalType;
    //[SerializeField] private JOB _jobType;
    //[SerializeField] private COMBAT_POSITION _combatPosition;
    //[SerializeField] private COMBAT_TARGET _combatTarget;
    [SerializeField] private ATTACK_TYPE _attackType;
    [SerializeField] private RANGE_TYPE _rangeType;
    //[SerializeField] private DAMAGE_TYPE _damageType;
    //[SerializeField] private COMBAT_OCCUPIED_TILE _occupiedTileType;

    //private int _dodgeRate;
    //private int _parryRate;
    //private int _blockRate;

    #region getters/setters
    public string className {
        get { return _className; }
        //set { _className = value; }
    }
    public string identifier {
        get { return _identifier; }
    }
    //public bool isNormalNonCombatant {
    //    get { return _isNormalNonCombatant; }
    //}
    public int baseAttackPower {
        get { return _baseAttackPower; }
    }
    //public int attackPowerPerLevel {
    //    get { return _attackPowerPerLevel; }
    //}
    //public int baseSpeed {
    //    get { return _baseSpeed; }
    //}
    //public int speedPerLevel {
    //    get { return _speedPerLevel; }
    //}
    public int baseHP {
        get { return _baseHP; }
    }
    public int baseAttackSpeed {
        get { return _baseAttackSpeed; }
    }
    //public int hpPerLevel {
    //    get { return _hpPerLevel; }
    //}
    public float attackRange {
        get { return _attackRange; }
    }
    //public float runSpeedMod {
    //    get { return _runSpeedMod; }
    //}
    //public float walkSpeedMod {
    //    get { return _walkSpeedMod; }
    //}
    public int inventoryCapacity {
        get { return _inventoryCapacity; }
    }
    //public CHARACTER_ROLE roleType {
    //    get { return _roleType; }
    //}
    //public JOB jobType {
    //    get { return _jobType; }
    //}
    public ELEMENTAL_TYPE elementalType {
        get { return _elementalType; }
    }
    //public COMBAT_POSITION combatPosition {
    //    get { return _combatPosition; }
    //}
    //public COMBAT_TARGET combatTarget {
    //    get { return _combatTarget; }
    //}
    public ATTACK_TYPE attackType {
        get { return _attackType; }
    }
    public RANGE_TYPE rangeType {
        get { return _rangeType; }
    }
    //public DAMAGE_TYPE damageType {
    //    get { return _damageType; }
    //}
    //public COMBAT_OCCUPIED_TILE occupiedTileType {
    //    get { return _occupiedTileType; }
    //}
    //public string skillName {
    //    get { return _skillName; }
    //}
    public string[] traitNames {
        get { return _traitNames; }
    }
    public string[] interestedItemNames {
        get { return _interestedItemNames; }
    }
    public STRUCTURE_TYPE[] relatedStructures {
        get { return _relatedStructures; }
    }
    #endregion

    public CharacterClass CreateNewCopy() {
        CharacterClass newClass = new CharacterClass();
        newClass._className = this._className;
        newClass._identifier = this._identifier;
        //newClass._isNormalNonCombatant = this._isNormalNonCombatant;
        newClass._baseAttackPower = this._baseAttackPower;
        //newClass._baseSpeed = this._baseSpeed;
        newClass._baseHP = this._baseHP;
  //      newClass._attackPowerPerLevel = this._attackPowerPerLevel;
		//newClass._speedPerLevel = this._speedPerLevel;
  //      newClass._hpPerLevel = this._hpPerLevel;
        newClass._attackRange = this._attackRange;
        //newClass._runSpeedMod = this._runSpeedMod;
        //newClass._walkSpeedMod = this._walkSpeedMod;
        newClass._baseAttackSpeed = this._baseAttackSpeed;
        newClass._elementalType = this._elementalType;
        //newClass._workActionType = this._workActionType;
        //newClass._combatPosition = this._combatPosition;
        //newClass._combatTarget = this._combatTarget;
        newClass._attackType = this._attackType;
        newClass._rangeType = this._rangeType;
        //newClass._damageType = this._damageType;
        //newClass._occupiedTileType = this._occupiedTileType;
        //newClass._roleType = this._roleType;
        //newClass._skillName = this._skillName;
        newClass._traitNames = this._traitNames;
        //newClass._jobType = this._jobType;
        //Array.Copy(this._traitNames, newClass._traitNames, this._traitNames.Length);
        return newClass;
    }

    public void SetData(ClassComponent classComponent) {
        this._className = classComponent.className;
        //this._baseAttackPower = classComponent.baseAttackPower;
        //this._attackPowerPerLevel = classComponent.attackPowerPerLevel;
        //this._baseSpeed = classComponent.baseSpeed;
        //this._speedPerLevel = classComponent.speedPerLevel;
        //this._baseHP = classComponent.baseHP;
        //this._hpPerLevel = (float)classComponent.hpPerLevel;
        //this._workActionType = classComponent.workActionType;
        //this._dodgeRate = classComponent.dodgeRate;
        //this._parryRate = classComponent.parryRate;
        //this._blockRate = classComponent.blockRate;
        //this._skillName = classComponent.skill.name;
    }

    public void SetDataFromClassPanelUI() {
        this._className = ClassPanelUI.Instance.classNameInput.text;
        this._identifier = ClassPanelUI.Instance.identifierInput.text;
        //this._isNormalNonCombatant = ClassPanelUI.Instance.nonCombatantToggle.isOn;
        this._baseAttackPower = int.Parse(ClassPanelUI.Instance.baseAttackPowerInput.text);
        //this._attackPowerPerLevel = int.Parse(ClassPanelUI.Instance.attackPowerPerLevelInput.text);
        //this._baseSpeed = int.Parse(ClassPanelUI.Instance.baseSpeedInput.text);
        //this._speedPerLevel = int.Parse(ClassPanelUI.Instance.speedPerLevelInput.text);
        this._baseHP = int.Parse(ClassPanelUI.Instance.baseHPInput.text);
        //this._hpPerLevel = int.Parse(ClassPanelUI.Instance.hpPerLevelInput.text);
        this._baseAttackSpeed = int.Parse(ClassPanelUI.Instance.baseAttackSpeedInput.text);
        this._attackRange = float.Parse(ClassPanelUI.Instance.attackRangeInput.text);
        //this._runSpeedMod = float.Parse(ClassPanelUI.Instance.runSpeedModInput.text);
        this._inventoryCapacity = int.Parse(ClassPanelUI.Instance.inventoryCapacityInput.text);
        this._elementalType = (ELEMENTAL_TYPE) System.Enum.Parse(typeof(ELEMENTAL_TYPE), ClassPanelUI.Instance.elementalTypeOptions.options[ClassPanelUI.Instance.elementalTypeOptions.value].text);
        //this._combatPosition = (COMBAT_POSITION) System.Enum.Parse(typeof(COMBAT_POSITION), ClassPanelUI.Instance.combatPositionOptions.options[ClassPanelUI.Instance.combatPositionOptions.value].text);
        //this._combatTarget = (COMBAT_TARGET)System.Enum.Parse(typeof(COMBAT_TARGET), ClassPanelUI.Instance.combatTargetOptions.options[ClassPanelUI.Instance.combatTargetOptions.value].text);
        this._attackType = (ATTACK_TYPE) System.Enum.Parse(typeof(ATTACK_TYPE), ClassPanelUI.Instance.attackTypeOptions.options[ClassPanelUI.Instance.attackTypeOptions.value].text);
        this._rangeType = (RANGE_TYPE) System.Enum.Parse(typeof(RANGE_TYPE), ClassPanelUI.Instance.rangeTypeOptions.options[ClassPanelUI.Instance.rangeTypeOptions.value].text);
        //this._damageType = (DAMAGE_TYPE) System.Enum.Parse(typeof(DAMAGE_TYPE), ClassPanelUI.Instance.damageTypeOptions.options[ClassPanelUI.Instance.damageTypeOptions.value].text);
        //this._occupiedTileType = (COMBAT_OCCUPIED_TILE) System.Enum.Parse(typeof(COMBAT_OCCUPIED_TILE), ClassPanelUI.Instance.occupiedTileOptions.options[ClassPanelUI.Instance.occupiedTileOptions.value].text);
        //this._roleType = (CHARACTER_ROLE) System.Enum.Parse(typeof(CHARACTER_ROLE), ClassPanelUI.Instance.roleOptions.options[ClassPanelUI.Instance.roleOptions.value].text);
        //this._skillName = ClassPanelUI.Instance.skillOptions.options[ClassPanelUI.Instance.skillOptions.value].text;
        this._traitNames = ClassPanelUI.Instance.traitNames.ToArray();
        this._relatedStructures = ClassPanelUI.Instance.relatedStructures.ToArray();
        this._interestedItemNames = UtilityScripts.Utilities.ConvertStringToArray(ClassPanelUI.Instance.interestedItemNamesInput.text, ',');
        //this._jobType = (JOB) System.Enum.Parse(typeof(JOB), ClassPanelUI.Instance.jobTypeOptions.options[ClassPanelUI.Instance.jobTypeOptions.value].text);
    }
    public bool IsCombatant() {
        if(_traitNames != null) {
            for (int i = 0; i < _traitNames.Length; i++) {
                if(_traitNames[i] == "Combatant") {
                    return true;
                }
            }
        }
        return false;
    }
    //public void ConstructData() {
    //    ConstructSkills();
    //}
    //private void ConstructSkills() {
    //    _skill = SkillManager.Instance.allSkills[_skillName];
    //}
}