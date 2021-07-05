using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using Object_Pools;
using Character_Talents;

public class CharacterClassComponent : CharacterComponent {
    public CharacterClass characterClass { get; private set; }
    public string previousClassName { get; private set; }
    public List<string> ableClasses { get; private set; }
    public bool shouldChangeClass { get; private set; }

    #region getters
    public bool canChangeClass => !characterClass.IsSpecialClass(); // && characterClass.className != "Ratman"; As per Marvs - allowed ratmen to change class for now June 26, 2021 - Changed by Mykel
    #endregion
    public CharacterClassComponent() {
        previousClassName = string.Empty;
        ableClasses = new List<string>();
    }

    public CharacterClassComponent(SaveDataCharacterClassComponent data) {
        characterClass = CharacterManager.Instance.GetCharacterClass(data.className);
        previousClassName = data.previousClassName;
        shouldChangeClass = data.shouldChangeClass;
        ableClasses = data.ableClasses;
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
    public void AssignClass(CharacterClass p_newClass, bool isInitial = false) {
        CharacterClass previousClass = characterClass;
        if (previousClass != null) {
            //owner.homeSettlement?.UnapplyAbleJobsFromSettlement(owner);
            if (!isInitial) {
                //only populate previous class value if class set is not Initial!
                //this was necessary since a characters class will be changed after it's talents have 
                //been randomized. Reference: https://trello.com/c/0DSPyf4d/4716-update-starting-village-structures-villager-classes-and-talents
                previousClassName = previousClass.className;
            }
            //This means that the character currently has a class and it will be replaced with a new class
            for (int i = 0; i < previousClass.traitNames.Length; i++) {
                owner.traitContainer.RemoveTrait(owner, previousClass.traitNames[i]); //Remove traits from class
            }
            if (previousClass.interestedItemNames != null) {
                owner.RemoveItemAsInteresting(previousClass.interestedItemNames);
            }
        }
        characterClass = p_newClass;
        owner.movementComponent.OnAssignedClass(p_newClass);
        //behaviourComponent.OnChangeClass(_characterClass, previousClass);
        if (isInitial) {
            if (GameManager.Instance.gameHasStarted) {
                owner.RecomputeResistanceInitialChangeClass(owner, "Farmer");    
            } else {
                //if villager is an initial villager, randomize resistances based on fixed values from here:
                //https://docs.google.com/spreadsheets/d/1XEsZ2Fzi9Hwnx6EVdH1boCcFLR2sCUNyDm-Hh_A734Y/edit#gid=746271279
                owner.RecomputePiercingAndResistanceForGameStart(owner, owner.characterClass.className);
            }
        } else {
            //owner.homeSettlement?.UpdateAbleJobsOfResident(owner);
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

        if (owner.talentComponent != null) {
            owner.talentComponent.ReevaluateAllTalents();
        }

        if (owner.isNormalCharacter) {
            if (!characterClass.IsCombatant()) {
                //Once a character becomes a non-combatant, he must leave party, because only combatant characters can be in a party
                if (owner.partyComponent.hasParty) {
                    owner.partyComponent.currentParty.RemoveMember(owner);
                }
            }
        }
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
    public void PopulateAbleCombatantClasses(List<string> p_classes) {
        for (int i = 0; i < ableClasses.Count; i++) {
            string className = ableClasses[i];
            CharacterClass cc = CharacterManager.Instance.GetCharacterClass(className);
            if (cc.IsCombatant()) {
                p_classes.Add(className);
            }
        }
    }
    public void PopulateAbleFoodProducerClasses(List<string> p_classes) {
        for (int i = 0; i < ableClasses.Count; i++) {
            string className = ableClasses[i];
            CharacterClass cc = CharacterManager.Instance.GetCharacterClass(className);
            if (cc.IsFoodProducer()) {
                p_classes.Add(className);
            }
        }
    }
    public void PopulateBasicProducerClasses(List<string> p_classes, FACTION_TYPE p_factionType) {
        for (int i = 0; i < ableClasses.Count; i++) {
            string className = ableClasses[i];
            CharacterClass cc = CharacterManager.Instance.GetCharacterClass(className);
            if (cc.IsBasicResourceProducer(p_factionType)) {
                p_classes.Add(className);
            }
        }
    }
    public void PopulateAbleSpecialCivilianClasses(List<string> p_classes) {
        for (int i = 0; i < ableClasses.Count; i++) {
            string className = ableClasses[i];
            CharacterClass cc = CharacterManager.Instance.GetCharacterClass(className);
            if (cc.IsSpecialCivilian()) {
                p_classes.Add(className);
            }
        }
    }
    public void RandomizeCurrentClassBasedOnAbleClasses() {
        string randomClass = CollectionUtilities.GetRandomElement(ableClasses);
        AssignClass(randomClass, true);
        OnUpdateCharacterClass();
    }
    #endregion

    #region Supply Capacity
    public int GetFoodSupplyCapacityValue() {
        int supply = 0;
        Character c = owner;
        if (c.structureComponent.HasWorkPlaceStructure()) {
            supply += GetFoodSupplyCapacityValueBase();
        }
        return supply;
    }
    public int GetFoodSupplyCapacityValueBase() {
        int supply = 0;
        Character c = owner;
        if (!c.isDead && c.characterClass.className.IsFoodProducerClassName()) {
            //If character is paralyzed, restrained or quarantined, he should not be counted
            bool isAvailable = !c.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && c.HasTalents();
            if (isAvailable) {
                //Only consider if the claimed work structure type is the appropriate one
                CharacterTalent foodTalent = c.talentComponent.GetTalent(CHARACTER_TALENT.Food);
                switch (foodTalent.level) {
                    case 1:
                    case 2:
                        supply += 8;
                        break;
                    case 3:
                    case 4:
                        supply += 16;
                        break;
                    case 5:
                        supply += 24;
                        break;
                    default:
                        break;
                }
            }
        }
        return supply;
    }
    public int GetResourceSupplyCapacityValue(string p_className) {
        int supply = 0;
        Character c = owner;
        if (c.structureComponent.HasWorkPlaceStructure()) {
            supply += GetResourceSupplyCapacityValueBase(p_className);
        }
        return supply;
    }
    public int GetResourceSupplyCapacityValueBase(string p_className) {
        int supply = 0;
        Character c = owner;
        if (!c.isDead && c.characterClass.className == p_className) {
            //If character is paralyzed, restrained or quarantined, he should not be counted
            bool isAvailable = !c.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && c.HasTalents();
            if (isAvailable) {
                //Only consider if the claimed work structure type is the appropriate one
                supply += 8;
                //CharacterTalent foodTalent = c.talentComponent.GetTalent(CHARACTER_TALENT.Food);
                //switch (foodTalent.level) {
                //    case 1:
                //    case 2:
                //        supply += 8;
                //        break;
                //    case 3:
                //    case 4:
                //        supply += 16;
                //        break;
                //    case 5:
                //        supply += 24;
                //        break;
                //    default:
                //        break;
                //}
            }
        }
        return supply;
    }
    public int GetCombatSupplyValue() {
        int supply = 0;
        Character c = owner;
        if (!c.isDead) {
            //If character is paralyzed, restrained or quarantined, he should not be counted
            bool isAvailable = !c.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && c.HasTalents();
            if (isAvailable) {
                if (characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                    supply += c.talentComponent.GetTalent(CHARACTER_TALENT.Martial_Arts).level;
                } else {
                    supply += c.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).level;
                }
            }
        }
        return supply;
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
    public List<string> ableClasses;
    #region Overrides
    public override void Save(CharacterClassComponent data) {
        className = data.characterClass.className;
        previousClassName = data.previousClassName;
        shouldChangeClass = data.shouldChangeClass;
        ableClasses = new List<string>(data.ableClasses);
    }

    public override CharacterClassComponent Load() {
        CharacterClassComponent component = new CharacterClassComponent(this);
        return component;
    }
    #endregion
}