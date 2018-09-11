﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PlayerAbility {
    protected string _name;
    protected string _description;
    protected int _powerCost;
    protected int _threatGain;
    protected int _cooldown;

    protected int _cooldownCount;
    protected bool _isEnabled;

    public PlayerAbility() {
    }

    #region Virtuals
    public virtual void Activate(IInteractable interactable) {
        PayPowerCost(interactable);
        ThreatGain();
        GoOnCooldown();
    }
    #endregion

    #region Utilities
    public void GoOnCooldown() {
        _cooldownCount = 0;
        SetIsEnabled(false);
        Messenger.AddListener(Signals.HOUR_STARTED, Cooldown);
    }
    private void Cooldown() {
        _cooldownCount++;
        if(_cooldownCount >= _cooldown) {
            StopCooldownAndEnableAbility();
        }
    }
    private void StopCooldownAndEnableAbility() {
        Messenger.RemoveListener(Signals.HOUR_STARTED, Cooldown);
        SetIsEnabled(true);
    }
    public void SetIsEnabled(bool state) {
        _isEnabled = state;
    }
    private void PayPowerCost(IInteractable interactable) {
        if(interactable is Character) {
            PlayerManager.Instance.player.AdjustBlueMagic(-_powerCost);
        }else if (interactable is BaseLandmark) {
            PlayerManager.Instance.player.AdjustGreenMagic(-_powerCost);
        }else if (interactable is Monster) {
            PlayerManager.Instance.player.AdjustRedMagic(-_powerCost);
        }
    }
    private void ThreatGain() {
        PlayerManager.Instance.player.AdjustThreatLevel(_threatGain);
    }
    #endregion
}
