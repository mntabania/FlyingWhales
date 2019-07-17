﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseDead : PlayerJobAction {

    public RaiseDead() : base(INTERVENTION_ABILITY.RAISE_DEAD) {
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        Character target;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else if (targetPOI is Tombstone) {
            target = (targetPOI as Tombstone).character;
        } else {
            return;
        }
        base.ActivateAction(assignedCharacter, target);
        target.RaiseFromDeath();

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_raise_dead");
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        return targetCharacter.isDead && targetCharacter.IsInOwnParty();
    }
    protected override bool CanPerformActionTowards(Character character, IPointOfInterest targetPOI) {
        return targetPOI is Tombstone || (targetPOI is Character && (targetPOI as Character).IsInOwnParty()) ;
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character) && !(targetPOI is Tombstone)) {
            return false;
        }
        Character targetCharacter;
        if (targetPOI is Character) {
            targetCharacter = targetPOI as Character;
        } else {
            targetCharacter = (targetPOI as Tombstone).character;
        }
        return targetCharacter.isDead && targetCharacter.IsInOwnParty();
    }
}
