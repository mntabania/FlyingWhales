using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class CharacterClass {
    [SerializeField] private string _className;
    [SerializeField] private int _baseAttackPower;
    [SerializeField] private int _baseHP;
    [SerializeField] private int _baseAttackSpeed; //The lower the amount the faster the attack rate
    [SerializeField] private int _inventoryCapacity;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _staminaReduction;
    [SerializeField] private string _identifier;
    [SerializeField] private string _traitNameOnTamedByPlayer; //If this is not empty, this trait will be added if the character is spawned in the player's side

    [SerializeField] private string[] _traitNames;
    [SerializeField] private string[] _interestedItemNames;

    [SerializeField] private ELEMENTAL_TYPE _elementalType;
    [SerializeField] private ATTACK_TYPE _attackType;
    [SerializeField] private RANGE_TYPE _rangeType;

    [SerializeField] private JOB_TYPE[] _ableJobs;

    #region getters/setters
    public string className => _className;
    public string identifier => _identifier;
    public string traitNameOnTamedByPlayer => _traitNameOnTamedByPlayer;
    public int baseAttackPower => _baseAttackPower;
    public int baseHP => _baseHP;
    public int baseAttackSpeed => _baseAttackSpeed;
    public float attackRange => _attackRange;
    public float staminaReduction => _staminaReduction;
    public int inventoryCapacity => _inventoryCapacity;
    public ELEMENTAL_TYPE elementalType => _elementalType;
    public ATTACK_TYPE attackType => _attackType;
    public RANGE_TYPE rangeType => _rangeType;
    public string[] traitNames => _traitNames;
    public string[] interestedItemNames => _interestedItemNames;
    public JOB_TYPE[] ableJobs => _ableJobs;
    #endregion

  //  public CharacterClass CreateNewCopy() {
  //      CharacterClass newClass = new CharacterClass();
  //      newClass._className = this._className;
  //      newClass._identifier = this._identifier;
  //      //newClass._isNormalNonCombatant = this._isNormalNonCombatant;
  //      newClass._baseAttackPower = this._baseAttackPower;
  //      //newClass._baseSpeed = this._baseSpeed;
  //      newClass._baseHP = this._baseHP;
  ////      newClass._attackPowerPerLevel = this._attackPowerPerLevel;
		////newClass._speedPerLevel = this._speedPerLevel;
  ////      newClass._hpPerLevel = this._hpPerLevel;
  //      newClass._attackRange = this._attackRange;
  //      //newClass._runSpeedMod = this._runSpeedMod;
  //      //newClass._walkSpeedMod = this._walkSpeedMod;
  //      newClass._baseAttackSpeed = this._baseAttackSpeed;
  //      newClass._elementalType = this._elementalType;
  //      //newClass._workActionType = this._workActionType;
  //      //newClass._combatPosition = this._combatPosition;
  //      //newClass._combatTarget = this._combatTarget;
  //      newClass._attackType = this._attackType;
  //      newClass._rangeType = this._rangeType;
  //      //newClass._damageType = this._damageType;
  //      //newClass._occupiedTileType = this._occupiedTileType;
  //      //newClass._roleType = this._roleType;
  //      //newClass._skillName = this._skillName;
  //      newClass._traitNames = this._traitNames;
  //      newClass._inventoryCapacity = this._inventoryCapacity;
  //      newClass._interestedItemNames = this._interestedItemNames;
  //      newClass._relatedStructures = this._relatedStructures;
  //      newClass._priorityJobs = this._priorityJobs;
  //      newClass._secondaryJobs = this._secondaryJobs;
  //      newClass._ableJobs = this._ableJobs;
  //      newClass._staminaReduction = this._staminaReduction;
  //      newClass._traitNameOnTamedByPlayer = this._traitNameOnTamedByPlayer;
  //      //newClass._jobType = this._jobType;
  //      //Array.Copy(this._traitNames, newClass._traitNames, this._traitNames.Length);
  //      return newClass;
  //  }

    public void SetDataFromClassPanelUI() {
        this._className = ClassPanelUI.Instance.classNameInput.text;
        this._identifier = ClassPanelUI.Instance.identifierInput.text;
        this._traitNameOnTamedByPlayer = ClassPanelUI.Instance.tamedTraitInput.text;
        this._baseAttackPower = int.Parse(ClassPanelUI.Instance.baseAttackPowerInput.text);
        this._baseHP = int.Parse(ClassPanelUI.Instance.baseHPInput.text);
        this._baseAttackSpeed = int.Parse(ClassPanelUI.Instance.baseAttackSpeedInput.text);
        this._attackRange = float.Parse(ClassPanelUI.Instance.attackRangeInput.text);
        this._staminaReduction = float.Parse(ClassPanelUI.Instance.staminaReductionInput.text);
        this._inventoryCapacity = int.Parse(ClassPanelUI.Instance.inventoryCapacityInput.text);
        this._elementalType = (ELEMENTAL_TYPE) System.Enum.Parse(typeof(ELEMENTAL_TYPE), ClassPanelUI.Instance.elementalTypeOptions.options[ClassPanelUI.Instance.elementalTypeOptions.value].text);
        this._attackType = (ATTACK_TYPE) System.Enum.Parse(typeof(ATTACK_TYPE), ClassPanelUI.Instance.attackTypeOptions.options[ClassPanelUI.Instance.attackTypeOptions.value].text);
        this._rangeType = (RANGE_TYPE) System.Enum.Parse(typeof(RANGE_TYPE), ClassPanelUI.Instance.rangeTypeOptions.options[ClassPanelUI.Instance.rangeTypeOptions.value].text);
        this._traitNames = ClassPanelUI.Instance.traitNames.ToArray();
        //this._relatedStructures = ClassPanelUI.Instance.relatedStructures.ToArray();
        this._interestedItemNames = UtilityScripts.Utilities.ConvertStringToArray(ClassPanelUI.Instance.interestedItemNamesInput.text, ',');
        //this._priorityJobs = ClassPanelUI.Instance.priorityJobs.ToArray();
        //this._secondaryJobs = ClassPanelUI.Instance.secondaryJobs.ToArray();
        this._ableJobs = ClassPanelUI.Instance.ableJobs.ToArray();
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
        return ableJobs != null && ableJobs.Contains(jobType);
    }
    public bool IsSpecialClass() {
        return identifier == "Special";
    }
    public bool IsFoodProducer() {
        return className == "Farmer" || className == "Butcher" || className == "Fisher";
    }
    public bool IsBasicResourceProducer() {
        return className == "Logger" || className == "Miner";
    }
    public bool IsBasicResourceProducer(FACTION_TYPE p_factionType) {
        switch (p_factionType) {
            case FACTION_TYPE.Elven_Kingdom:
                return className == "Logger";
            case FACTION_TYPE.Human_Empire:
                return className == "Miner";
            default:
                return className == "Logger" || className == "Miner";
        }
    }
    public bool IsSpecialCivilian() {
        return className == "Skinner" || className == "Crafter" || className == "Merchant";
    }
    #endregion
}