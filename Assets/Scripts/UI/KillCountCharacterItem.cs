﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Traits;
using System.Linq;

public class KillCountCharacterItem : CharacterNameplateItem {

    public override void SetObject(Character character) {
        base.SetObject(character);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterRemovedTrait);
        Messenger.AddListener<Character>(Signals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnedToLife);
        Messenger.AddListener<Character>(Signals.CHARACTER_SWITCHED_ALTER_EGO, OnCharacterSwitchedAlterEgo);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterChangedFaction);
        Messenger.AddListener<Character>(Signals.ROLE_CHANGED, OnCharacterChangedRole);
        Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterRemovedTrait);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnedToLife);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_SWITCHED_ALTER_EGO, OnCharacterSwitchedAlterEgo);
        Messenger.RemoveListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterChangedFaction);
        Messenger.RemoveListener<Character>(Signals.ROLE_CHANGED, OnCharacterChangedRole);
        Messenger.RemoveListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        StopScroll();
    }
    private void UpdateInfo() {
        SetSupportingLabelState(true);
        if (character.isDead) {
            supportingLbl.text = "\"" + character.deathStr + "\"";
        } else {
            string text = string.Empty;
            Trait negDisTrait = character.traitContainer.GetAllTraitsOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE).FirstOrDefault();
            if (negDisTrait != null) {
                if (negDisTrait is Unconscious) {
                    text = "\"" + character.name + " was knocked out";
                } else if (negDisTrait is Restrained) {
                    text = "\"" + character.name + " was restrained by " + negDisTrait.responsibleCharacter.name + ".\"";
                } else if (negDisTrait is Paralyzed) {
                    text = "\"" + character.name + " became paralyzed.\"";
                }
                if(negDisTrait.responsibleCharacter != null) {
                    text += " by " + negDisTrait.responsibleCharacter.name + ".\"";
                } else {
                    text += ".\"";
                }
            } else {
                if (character.returnedToLife) {
                    if (character.characterClass.className == "Zombie") {
                        text = "\"" + character.name + " turned into a zombie.\"";
                    } else {
                        text = "\"" + character.name + " was resurrected into a mindless skeleton.\"";
                    }
                } else if (character.currentAlterEgo.name != CharacterManager.Original_Alter_Ego) {
                    text = "\"" + character.name + " turned into a " + character.currentAlterEgo.name + ".\"";
                } else if (character.role.roleType == CHARACTER_ROLE.MINION) {
                    text = "\"" + character.name + " became a minion.\"";
                }
            }
            if (string.IsNullOrEmpty(text)) {
                //character is not yet dead and not disabled, show current action insteadC
                if (character.currentActionNode != null) {
                    Log currentLog = character.currentActionNode.GetCurrentLog();
                    text = Utilities.LogReplacer(currentLog);
                }
            }
            supportingLbl.text = text;
            
        }
    }

    #region Listeners
    private void OnCharacterChangedRace(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterChangedFaction(Character character, Faction faction) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterChangedRole(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterDied(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (character.id == this.character.id && trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            UpdateInfo();
        }
    }
    private void OnCharacterRemovedTrait(Character character, Trait trait) {
        if (character.id == this.character.id && trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            UpdateInfo();
        }
    }
    private void OnCharacterReturnedToLife(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterSwitchedAlterEgo(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterDoingAction(Character character, ActualGoapNode action) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    #endregion
}
