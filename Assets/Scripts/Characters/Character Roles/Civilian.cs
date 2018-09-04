﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Civilian : CharacterRole {

    public Civilian(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.CIVILIAN;

        SetFullness(100);
        SetEnergy(100);
        SetFun(60);
        //SetPrestige(40);
        //SetSanity(100);
        //UpdateSafety();
        UpdateHappiness();

        //_character.onDailyAction += StartDepletion;
    }

    #region Overrides
    public override void DeathRole() {
        base.DeathRole();
        Messenger.RemoveListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
    }
    public override void ChangedRole() {
        base.ChangedRole();
        Messenger.RemoveListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
    }
    public override void OnAssignRole() {
        base.OnAssignRole();
        Messenger.AddListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
    }
    #endregion

    private void OnStructureChangedState(StructureObj structure, ObjectState state) {
        if(structure == _character.homeStructure && state.stateName == "Ruined") {
            _character.LookForNewHomeStructure();
        }
    }

    private void StartDepletion() {
        DepleteFullness();
        DepleteEnergy();
        DepleteFun();
        //DepleteSanity();
        //DepletePrestige();
    }
}
