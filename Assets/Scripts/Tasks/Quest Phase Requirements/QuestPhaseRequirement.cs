using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ECS;

/*
 Base class for the requirements of a quest phase before the quest can advance.
 Each character that accepted the quest will have their own instance 
 of this class in their quest data.
     */
public class QuestPhaseRequirement : ICloneable{

    protected bool _isRequirementMet;
    protected ECS.Character owner;

    #region getters/setters
    public bool isRequirementMet {
        get { return _isRequirementMet; }
    }
    #endregion

    public QuestPhaseRequirement() {
        _isRequirementMet = false;
    }
	public virtual void ActivateRequirement(ECS.Character owner){ this.owner = owner; } //What happens when this requirement is relevant
    public virtual void DeactivateRequirement() { } //What happens when this requirement is no longer relevant
    public object Clone() {
        return this.MemberwiseClone();
    }
}

public class MustHaveItems : QuestPhaseRequirement {
    private List<string> neededItems;
    public MustHaveItems(string neededItem) : base(){
        neededItems = new List<string>();
        neededItems.Add(neededItem);
    }
    public MustHaveItems(List<string> neededItems) : base() {
        this.neededItems = new List<string>(neededItems);
    }
    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<ECS.Item, ECS.Character>(Signals.ITEM_OBTAINED, CheckIfRequirementMet);
    }
    public override void DeactivateRequirement() {
        base.DeactivateRequirement();
        Messenger.RemoveListener<ECS.Item, ECS.Character>(Signals.ITEM_OBTAINED, CheckIfRequirementMet);
    }
    protected void CheckIfRequirementMet(ECS.Item item, ECS.Character character) {
        if (character.id == owner.id) {
            for (int i = 0; i < neededItems.Count; i++) {
                string currNeededItem = neededItems[i];
                if (!character.HasItem(currNeededItem)) {
                    //the character does not yet have all of the needed items, return.
                    return;
                }
            }
            Debug.Log(owner.name + " has met requirements!: " + this.GetType().ToString());
            //the character has all needed items, requirement has been met
            _isRequirementMet = true;
            character.questData.CheckPhaseAdvancement();
        }
    }
}

public class MustFindItem : QuestPhaseRequirement {
    private string neededItem;
    private bool includeTrace;

    public MustFindItem(string neededItem, bool includeTrace = false) : base() {
        this.neededItem = neededItem;
        this.includeTrace = includeTrace;
    }
    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<ECS.Character, string>(Signals.FOUND_ITEM, CheckIfRequirementMet);
        if (includeTrace) {
            Messenger.AddListener<ECS.Character, string>(Signals.FOUND_TRACE, CheckIfRequirementMet);
        }
    }
    public override void DeactivateRequirement() {
        base.DeactivateRequirement();
        Messenger.RemoveListener<ECS.Character, string>(Signals.FOUND_ITEM, CheckIfRequirementMet);
        if (includeTrace) {
            Messenger.RemoveListener<ECS.Character, string>(Signals.FOUND_TRACE, CheckIfRequirementMet);
        }
    }
    protected void CheckIfRequirementMet(ECS.Character character, ECS.Item item) {
        if (character.id == owner.id) {
            if (!item.itemName.Equals(neededItem)) {
                return; //character did not find the right item
            }
            Debug.Log(owner.name + " has met requirements!: " + this.GetType().ToString());
            //the character found the needed item
            _isRequirementMet = true;
            character.questData.CheckPhaseAdvancement();
        }
    }
    protected void CheckIfRequirementMet(ECS.Character character, string item) {
        if (character.id == owner.id) {
            if (!item.Equals(neededItem)) {
                return; //character did not find the right item
            }
            Debug.Log(owner.name + " has met requirements!: " + this.GetType().ToString());
            //the character found the needed item
            _isRequirementMet = true;
            character.questData.CheckPhaseAdvancement();
        }
    }
}

public class MustFinishAllPhaseTasks : QuestPhaseRequirement {
    public MustFinishAllPhaseTasks() : base() {}
    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<ECS.Character, CharacterTask>(Signals.TASK_SUCCESS, CheckIfRequirementMet);
    }
    public override void DeactivateRequirement() {
        base.DeactivateRequirement();
        Messenger.RemoveListener<ECS.Character, CharacterTask>(Signals.TASK_SUCCESS, CheckIfRequirementMet);
    }
    protected void CheckIfRequirementMet(ECS.Character character, CharacterTask succeededTask) {
        if (character.id == owner.id && succeededTask.assignedCharacter.id == character.id) {
            for (int i = 0; i < character.questData.tasks.Count; i++) {
                if (!character.questData.tasks[i].isDone) {
                    return; //character has not yet finished all tasks
                }
            }
            //the character finished all the tasks
            _isRequirementMet = true;
            owner.questData.CheckPhaseAdvancement();
        }
    }
}

public class MustKillCharacter : QuestPhaseRequirement {
    private ECS.Character target;

    public MustKillCharacter(ECS.Character target) : base() {
        this.target = target;
    }
    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<Character, ECS.Character>(Signals.CHARACTER_KILLED, CheckIfRequirementMet);
    }
    public override void DeactivateRequirement() {
        base.DeactivateRequirement();
        Messenger.RemoveListener<Character, ECS.Character>(Signals.CHARACTER_KILLED, CheckIfRequirementMet);
    }
    protected void CheckIfRequirementMet(Character killer, ECS.Character characterThatDied) {
        if (characterThatDied.id == target.id) {
            if (killer.id == owner.id) {
                //the target has died
                _isRequirementMet = true;
                owner.questData.CheckPhaseAdvancement();
            }
            //if (killer is ECS.Character) {
            //    if ((killer as ECS.Character).id == owner.id) {
            //        //the target has died
            //        _isRequirementMet = true;
            //        owner.questData.CheckPhaseAdvancement();
            //    }
            //} else if (killer is Party) {
            //    if ((killer as Party).partyMembers.Contains(owner)) {
            //        //the target has died
            //        _isRequirementMet = true;
            //        owner.questData.CheckPhaseAdvancement();
            //    }
            //}
        }
    }
}

public class CharacterMustDie : QuestPhaseRequirement {
    private ECS.Character target;

    public CharacterMustDie(ECS.Character target) : base() {
        this.target = target;
    }
    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, CheckIfRequirementMet);
    }
    public override void DeactivateRequirement() {
        base.DeactivateRequirement();
        Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_DEATH, CheckIfRequirementMet);
    }
    protected void CheckIfRequirementMet(ECS.Character character) {
        if (character.id == target.id) {
            //the target has died
            _isRequirementMet = true;
            character.questData.CheckPhaseAdvancement();
        }
    }
}

