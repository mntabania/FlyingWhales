﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class CharacterParty : Party {
    private bool _isIdle; //can't do action, needs will not deplete

    #region getters/setters
    public override string name {
        get {
            return GetPartyName();
        }
    }
    public bool isIdle {
        get { return _isIdle; }
    }
    public bool isBusy { //if the party's current action is not null and their action is not rest, they are busy
        get { return IsBusy(); }
    }
    #endregion

    public CharacterParty() : base (null){

    }

    public CharacterParty(Character owner): base(owner) {
        _isIdle = false;
#if !WORLD_CREATION_TOOL
        //Messenger.AddListener(Signals.DAY_ENDED, EverydayAction);
        //Messenger.AddListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //ConstructResourceInventory();
#endif
    }

    #region Utilities
    private string GetPartyName() {
        if (owner is CharacterArmyUnit) {
            if (characters.Count > 1) {
                string name = "Army of:";
                for (int i = 0; i < characters.Count; i++) {
                    name += "\n" + characters[i].name;
                }
                return name;
            } else {
                return owner.name;
            }
        } else {
            return base.name;
        }
    }
    //If true, party can't do daily action (onDailyAction), i.e. actions, needs
    public void SetIsIdle(bool state) {
        _isIdle = state;
    }
    public bool IsOwnerDead() {
        return _owner.isDead;
    }
    private bool IsBusy() {
        if (owner.minion != null) {
            //if the owner of the party is a minion, just check if it is enabled
            //if it is not enabled, means that the minion currently has an action
            return !owner.minion.isEnabled;
        }
        if (this.icon.isTravelling) {
            return true;
        }
        return false;
    }
    #endregion

    #region Overrides
    public void DisbandPartyKeepOwner() {
        while (characters.Count != 1) {
            for (int i = 0; i < characters.Count; i++) {
                Character currCharacter = characters[i];
                if (currCharacter.id != owner.id) {
                    RemoveCharacter(currCharacter);
                    break;
                }
            }
        }
    }
    /*
        Create a new icon for this character.
        Each character owns 1 icon.
            */
    public override void CreateIcon() {
        base.CreateIcon();
        GameObject characterIconGO = GameObject.Instantiate(CharacterManager.Instance.characterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);

        _icon = characterIconGO.GetComponent<CharacterAvatar>();
        _icon.Init(this);
        //_icon = characterIconGO.GetComponent<CharacterIcon>();
        //_icon.SetCharacter(this);
        //_icon.SetAnimator(CharacterManager.Instance.GetAnimatorByRole(mainCharacter.role.roleType));
        //PathfindingManager.Instance.AddAgent(_icon.aiPath);
        //PathfindingManager.Instance.AddAgent(_icon.pathfinder);

    }
    public override void RemoveListeners() {
        base.RemoveListeners();
        //Messenger.RemoveListener(Signals.DAY_ENDED, EverydayAction);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    #endregion

    #region Outside Handlers
    public void OnCharacterSnatched(Character snatchedCharacter) {
        if (snatchedCharacter.id == _owner.id) {
            //snatched character was the main character of this party, disband it
            DisbandPartyKeepOwner();
        }
    }
    public void OnCharacterDied(Character diedCharacter) {
        if (diedCharacter.id == _owner.id) {
            //character that died was the main character of this party, disband it
            DisbandPartyKeepOwner();
        }
    }
    #endregion
}
