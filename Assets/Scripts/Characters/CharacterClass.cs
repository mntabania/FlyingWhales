﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class CharacterClass {
    [SerializeField] private string _className;
    [SerializeField] private int _baseAttackPower;
    //[SerializeField] private int _attackPowerPerLevel;
    //[SerializeField] private int _baseSpeed; //movement speed
    //[SerializeField] private int _speedPerLevel;
    //[SerializeField] private int _hpPerLevel;
    [SerializeField] private int _baseHP;
    [SerializeField] private int _baseAttackSpeed; //The lower the amount the faster the attack rate
    [SerializeField] private int _inventoryCapacity;
    //[SerializeField] private float _runSpeedMod;
    //[SerializeField] private float _walkSpeedMod;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _staminaReduction;

    [SerializeField] private string _identifier;
    [SerializeField] private string _traitNameOnTamedByPlayer; //If this is not empty, this trait will be added if the character is spawned in the player's side

    [SerializeField] private string[] _traitNames;
    [SerializeField] private string[] _interestedItemNames;
    [SerializeField] private STRUCTURE_TYPE[] _relatedStructures;
    //[SerializeField] private bool _isNormalNonCombatant;
    [SerializeField] private ELEMENTAL_TYPE _elementalType;
    //[SerializeField] private JOB _jobType;
    //[SerializeField] private COMBAT_POSITION _combatPosition;
    //[SerializeField] private COMBAT_TARGET _combatTarget;
    [SerializeField] private ATTACK_TYPE _attackType;
    [SerializeField] private RANGE_TYPE _rangeType;

    [SerializeField] private JOB_TYPE[] _priorityJobs;
    [SerializeField] private JOB_TYPE[] _secondaryJobs;
    [SerializeField] private JOB_TYPE[] _ableJobs;
    //[SerializeField] private DAMAGE_TYPE _damageType;
    //[SerializeField] private COMBAT_OCCUPIED_TILE _occupiedTileType;

    //private int _dodgeRate;
    //private int _parryRate;
    //private int _blockRate;

    #region getters/setters
    public string className => _className;
    //set { _className = value; }
    public string identifier => _identifier;
    public string traitNameOnTamedByPlayer => _traitNameOnTamedByPlayer;
    //public bool isNormalNonCombatant {
    //    get { return _isNormalNonCombatant; }
    //}
    public int baseAttackPower => _baseAttackPower;
    //public int attackPowerPerLevel {
    //    get { return _attackPowerPerLevel; }
    //}
    //public int baseSpeed {
    //    get { return _baseSpeed; }
    //}
    //public int speedPerLevel {
    //    get { return _speedPerLevel; }
    //}
    public int baseHP => _baseHP;
    public int baseAttackSpeed => _baseAttackSpeed;
    //public int hpPerLevel {
    //    get { return _hpPerLevel; }
    //}
    public float attackRange => _attackRange;
    public float staminaReduction => _staminaReduction;
    //public float runSpeedMod {
    //    get { return _runSpeedMod; }
    //}
    //public float walkSpeedMod {
    //    get { return _walkSpeedMod; }
    //}
    public int inventoryCapacity => _inventoryCapacity;
    //public CHARACTER_ROLE roleType {
    //    get { return _roleType; }
    //}
    //public JOB jobType {
    //    get { return _jobType; }
    //}
    public ELEMENTAL_TYPE elementalType => _elementalType;
    //public COMBAT_POSITION combatPosition {
    //    get { return _combatPosition; }
    //}
    //public COMBAT_TARGET combatTarget {
    //    get { return _combatTarget; }
    //}
    public ATTACK_TYPE attackType => _attackType;
    public RANGE_TYPE rangeType => _rangeType;
    //public DAMAGE_TYPE damageType {
    //    get { return _damageType; }
    //}
    //public COMBAT_OCCUPIED_TILE occupiedTileType {
    //    get { return _occupiedTileType; }
    //}
    //public string skillName {
    //    get { return _skillName; }
    //}
    public string[] traitNames => _traitNames;
    public string[] interestedItemNames => _interestedItemNames;
    public STRUCTURE_TYPE[] relatedStructures => _relatedStructures;
    public JOB_TYPE[] priorityJobs => _priorityJobs;
    public JOB_TYPE[] secondaryJobs => _secondaryJobs;
    public JOB_TYPE[] ableJobs => _ableJobs;
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
        newClass._inventoryCapacity = this._inventoryCapacity;
        newClass._interestedItemNames = this._interestedItemNames;
        newClass._relatedStructures = this._relatedStructures;
        newClass._priorityJobs = this._priorityJobs;
        newClass._secondaryJobs = this._secondaryJobs;
        newClass._ableJobs = this._ableJobs;
        newClass._staminaReduction = this._staminaReduction;
        newClass._traitNameOnTamedByPlayer = this._traitNameOnTamedByPlayer;
        //newClass._jobType = this._jobType;
        //Array.Copy(this._traitNames, newClass._traitNames, this._traitNames.Length);
        return newClass;
    }

    public void SetDataFromClassPanelUI() {
        this._className = ClassPanelUI.Instance.classNameInput.text;
        this._identifier = ClassPanelUI.Instance.identifierInput.text;
        this._traitNameOnTamedByPlayer = ClassPanelUI.Instance.tamedTraitInput.text;
        //this._isNormalNonCombatant = ClassPanelUI.Instance.nonCombatantToggle.isOn;
        this._baseAttackPower = int.Parse(ClassPanelUI.Instance.baseAttackPowerInput.text);
        //this._attackPowerPerLevel = int.Parse(ClassPanelUI.Instance.attackPowerPerLevelInput.text);
        //this._baseSpeed = int.Parse(ClassPanelUI.Instance.baseSpeedInput.text);
        //this._speedPerLevel = int.Parse(ClassPanelUI.Instance.speedPerLevelInput.text);
        this._baseHP = int.Parse(ClassPanelUI.Instance.baseHPInput.text);
        //this._hpPerLevel = int.Parse(ClassPanelUI.Instance.hpPerLevelInput.text);
        this._baseAttackSpeed = int.Parse(ClassPanelUI.Instance.baseAttackSpeedInput.text);
        this._attackRange = float.Parse(ClassPanelUI.Instance.attackRangeInput.text);
        this._staminaReduction = float.Parse(ClassPanelUI.Instance.staminaReductionInput.text);
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
        this._priorityJobs = ClassPanelUI.Instance.priorityJobs.ToArray();
        this._secondaryJobs = ClassPanelUI.Instance.secondaryJobs.ToArray();
        this._ableJobs = ClassPanelUI.Instance.ableJobs.ToArray();

        //this._jobType = (JOB) System.Enum.Parse(typeof(JOB), ClassPanelUI.Instance.jobTypeOptions.options[ClassPanelUI.Instance.jobTypeOptions.value].text);
    }

    #region Utilities
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
    public bool IsZombie() {
        return identifier == "Zombie";
    }
    public bool CanDoJob(JOB_TYPE jobType) {
        return priorityJobs.Contains(jobType) || ableJobs.Contains(jobType) || secondaryJobs.Contains(jobType);
    }
    public bool IsSpecialClass() {
        return identifier == "Special";
        //return className == "Necromancer" || className == "Hero" || className == "Vampire Lord" || className == "Cult Leader";
    }
    #endregion
}