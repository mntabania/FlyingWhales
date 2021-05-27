using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using Object_Pools;

public class CharacterClassComponent : CharacterComponent {
    public CharacterClass characterClass { get; private set; }
    public string previousClassName { get; private set; }
    public List<string> ableClasses { get; private set; }
    public bool shouldChangeClass { get; private set; }

    #region getters
    public bool canChangeClass => !characterClass.IsSpecialClass() && characterClass.className != "Ratman";
    #endregion
    public CharacterClassComponent() {
        previousClassName = string.Empty;
        ableClasses = new List<string>();
    }

    public CharacterClassComponent(SaveDataCharacterClassComponent data) {
        characterClass = CharacterManager.Instance.GetCharacterClass(data.className);
        previousClassName = data.previousClassName;
        shouldChangeClass = data.shouldChangeClass;
    }

    #region General
    public void AssignClass(string className, bool isInitial = false) {
        if (characterClass == null || className != characterClass.className) {
            if (CharacterManager.Instance.HasCharacterClass(className)) {
                AssignClass(CharacterManager.Instance.GetCharacterClass(className), isInitial);
            } else {
                throw new Exception($"There is no class named {className} but it is being assigned to {owner.name}");
            }
        }
    }
    public void AssignClass(CharacterClass characterClass, bool isInitial = false) {
        CharacterClass previousClass = characterClass;
        if (previousClass != null) {
            owner.homeSettlement?.UnapplyAbleJobsFromSettlement(owner);
            previousClassName = previousClass.className;
            //This means that the character currently has a class and it will be replaced with a new class
            for (int i = 0; i < previousClass.traitNames.Length; i++) {
                owner.traitContainer.RemoveTrait(owner, previousClass.traitNames[i]); //Remove traits from class
            }
            if (previousClass.interestedItemNames != null) {
                owner.RemoveItemAsInteresting(previousClass.interestedItemNames);
            }
        }
        this.characterClass = characterClass;
        owner.movementComponent.OnAssignedClass(characterClass);
        //behaviourComponent.OnChangeClass(_characterClass, previousClass);
        if (!isInitial) {
            owner.homeSettlement?.UpdateAbleJobsOfResident(owner);
            OnUpdateCharacterClass();
            Messenger.Broadcast(CharacterSignals.CHARACTER_CLASS_CHANGE, owner, previousClass, this.characterClass);
        }
        owner.combatComponent.UpdateElementalType();
    }
    public void OnUpdateCharacterClass() {
        CharacterClassData classData = CharacterManager.Instance.GetOrCreateCharacterClassData(characterClass.className);
        if (classData != null) {
            owner.combatComponent.combatBehaviourParent.SetCombatBehaviour(classData.combatBehaviourType, owner);
            owner.combatComponent.specialSkillParent.SetSpecialSkill(classData.combatSpecialSkillType);
        }
        for (int i = 0; i < characterClass.traitNames.Length; i++) {
            owner.traitContainer.AddTrait(owner, characterClass.traitNames[i]);
        }
        if (characterClass.interestedItemNames != null) {
            owner.AddItemAsInteresting(characterClass.interestedItemNames);
        }
        owner.combatComponent.UpdateBasicData(false);
        owner.needsComponent.UpdateBaseStaminaDecreaseRate();
        owner.visuals.UpdateAllVisuals(owner);

        owner.UpdateCanCombatState();

        //Misc
        if (previousClassName == "Ratman") {
            owner.movementComponent.SetEnableDigging(false);
        }
        if (characterClass.className == "Ratman") {
            owner.movementComponent.SetEnableDigging(true);
        }
        //Should not remove necromancer trait when necromancer becomes werewolf because it is only temporary
        if (previousClassName == "Necromancer" && characterClass.className != "Werewolf") {
            owner.traitContainer.RemoveTrait(owner, "Necromancer");
        }
        if (characterClass.className == "Necromancer" && previousClassName != "Werewolf") {
            owner.traitContainer.AddTrait(owner, "Necromancer");
        }
        if (characterClass.className == "Hero") {
            //Reference: https://www.notion.so/ruinarch/Hero-9697369ffca6410296f852f295ee0090
            owner.traitContainer.RemoveAllTraitsByType(owner, TRAIT_TYPE.FLAW);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "became_hero", providedTags: LOG_TAG.Major);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            LogPool.Release(log);
            owner.traitContainer.AddTrait(owner, "Blessed");
        }

        //Whenever character class is updated, update attack and hp also, since it changes upon changing the ATTACK_TYPE of character
        owner.combatComponent.UpdateAttack();
        owner.combatComponent.UpdateMaxHPAndProportionateHP();
    }
    public void OverridePreviousClassName(string p_className) {
        previousClassName = p_className;
    }
    public void SetShouldChangeClass(bool p_state) {
        shouldChangeClass = p_state;
    }
    #endregion

    #region Able Classes
    public bool AddAbleClass(string p_className) {
        if (!HasAbleClass(p_className)) {
            ableClasses.Add(p_className);
            return true;
        }
        return false;
    }
    public bool RemoveAbleClass(string p_className) {
        return ableClasses.Remove(p_className);
    }
    public bool HasAbleClass(string p_className) {
        return ableClasses.Contains(p_className);
    }
    public string GetAbleClassesText() {
        string log = string.Empty;
        for (int i = 0; i < ableClasses.Count; i++) {
            if (i > 0) {
                log += ", ";
            }
            log += ableClasses[i];
        }
        return log;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterClassComponent data) {

    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterClassComponent : SaveData<CharacterClassComponent> {
    public string className;
    public string previousClassName;
    public bool shouldChangeClass;

    #region Overrides
    public override void Save(CharacterClassComponent data) {
        className = data.characterClass.className;
        previousClassName = data.previousClassName;
        shouldChangeClass = data.shouldChangeClass;
    }

    public override CharacterClassComponent Load() {
        CharacterClassComponent component = new CharacterClassComponent(this);
        return component;
    }
    #endregion
}