﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakenDesire : PlayerAbility {

    public AwakenDesire() : base(ABILITY_TYPE.CHARACTER) {
        _name = "Awaken Hidden Desire";
        _description = "Awaken a hidden desire of character";
        _powerCost = 35;
        _threatGain = 10;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (!CanBeActivated(interactable)) {
            return;
        }
        interactable.hiddenDesire.Awaken();
        base.Activate(interactable);
    }
    #endregion
}
