﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

[System.Serializable]
public class CharacterAction {
    protected ObjectState _state;
    protected ActionFilter[] _filters;
    protected bool _needsSpecificTarget;
    [SerializeField] protected CharacterActionData _actionData;

    #region getters/setters
    public ACTION_TYPE actionType {
        get { return _actionData.actionType; }
    }
    public ActionFilter[] filters {
        get { return _filters; }
    }
    public ObjectState state {
        get { return _state; }
    }
    public CharacterActionData actionData {
        get { return _actionData; }
    }
    public bool needsSpecificTarget {
        get { return _needsSpecificTarget; }
    }
    #endregion

    public CharacterAction(ObjectState state, ACTION_TYPE actionType) {
        _state = state;
        _needsSpecificTarget = false;
        _actionData.actionType = actionType;
        _actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
    }

    #region Virtuals
    public virtual void OnChooseAction() {

    }
    public virtual void PerformAction(Character character) {

    }
    public virtual void ActionSuccess() {
        if (_actionData.successFunction != null) {
            _actionData.successFunction.Invoke(_state.obj);
        }
    }
    public virtual void ActionFail() {
        if (_actionData.failFunction != null) {
            _actionData.failFunction.Invoke(_state.obj);
        }
    }
    public virtual CharacterAction Clone(ObjectState state) {
        CharacterAction clone = new CharacterAction(state, actionType);
        SetCommonData(clone);
        return clone;
    }
    public virtual bool CanBeDone() {
        return true;
    }
    public virtual void EndAction(Character character) {
        character.actionData.EndAction();
    }
    #endregion

    #region Filters
    public void SetFilters(ActionFilter[] filters) {
        _filters = filters;
    }
    public virtual bool MeetsRequirements(ECS.Character character, BaseLandmark landmark) {
        if (filters != null) {
            for (int i = 0; i < filters.Length; i++) {
                ActionFilter currFilter = filters[i];
                if (!currFilter.MeetsRequirements(character, landmark)) {
                    return false; //does not meet a requirement
                }
            }
        }
        return true; //meets all requirements
    }
    #endregion

    #region Utilities
    public void SetActionData(CharacterActionData data) {
        _actionData = data;
    }
    public void SetObjectState(ObjectState state) {
        _state = state;
    }
    public void GenerateName() {
        _actionData.actionName = Utilities.NormalizeStringUpperCaseFirstLetters(actionType.ToString());
    }
    public void GiveReward(NEEDS need, Character character) {
        switch (need) {
            case NEEDS.FULLNESS:
            character.role.AdjustFullness(_actionData.providedFullness);
            break;
            case NEEDS.ENERGY:
            character.role.AdjustEnergy(_actionData.providedEnergy);
            break;
            case NEEDS.JOY:
            character.role.AdjustJoy(_actionData.providedJoy);
            break;
            case NEEDS.PRESTIGE:
            character.role.AdjustPrestige(_actionData.providedPrestige);
            break;
        }
    }
    public void SetCommonData(CharacterAction action) {
        if (this._filters != null) {
            action._filters = new ActionFilter[this._filters.Length];
            for (int i = 0; i < this._filters.Length; i++) {
                action._filters[i] = this._filters[i];
            }
        }
        action._actionData = this._actionData;
    }
    #endregion

    #region Advertisement
    public float GetTotalAdvertisementValue(Character character) {
        return GetFoodAdvertisementValue(character) + GetEnergyAdvertisementValue(character) + GetJoyAdvertisementValue(character) + GetPrestigeAdvertisementValue(character);
    }
    private float GetAdvertisementValue(int currentNeed, int advertisedNeed) {
        //1, -1
        if(advertisedNeed != 0) {
            float x = (float) currentNeed;
            float y = x + ((float) advertisedNeed * 80f);
            if(y > 1000f) {
                y = 1000f;
            }
            float result = 1000f / y;
            if (y > 0) {
               result = (1000f / x) - (1000f / y);
            }
            //Add quest modifier
            return result;
        }
        return 0f;
    }
    private float GetFoodAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.fullness, _actionData.advertisedFullness);
    }
    private float GetEnergyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.energy, _actionData.advertisedEnergy);
    }
    private float GetJoyAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.joy, _actionData.advertisedJoy);
    }
    private float GetPrestigeAdvertisementValue(Character character) {
        return GetAdvertisementValue(character.role.prestige, _actionData.advertisedPrestige);
    }
    #endregion

    #region Logs
    public virtual string GetArriveActionString() {
        return LocalizationManager.Instance.GetLocalizedValue("CharacterActions", this.GetType().ToString(), "arrive_action");
    }
    public virtual string GetLeaveActionString() {
        return LocalizationManager.Instance.GetLocalizedValue("CharacterActions", this.GetType().ToString(), "leave_action");
    }
    #endregion
}
