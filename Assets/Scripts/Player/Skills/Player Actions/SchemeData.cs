using System.Collections;
using System.Collections.Generic;
using System;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SchemeData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SCHEME;
    public override string name => "Scheme";
    public override string description => $"Scheme";

    public SchemeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //TODO: On Hover Right Click action activated here? Show all schemes
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.faction != null && targetCharacter.faction.isPlayerFaction) {
                //Characters already part of player faction cannot be targeted by any schemes
                return false;
            }
            return targetCharacter.isDead == false;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is Character character) {
            if (!character.isNormalCharacter || character.isConsideredRatman) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    #endregion

    protected void ProcessScheme(Character targetCharacter, Action onSuccess, Action onFail, float successRate) {
        if (GameUtilities.RollChance(successRate)) {
            onSuccess?.Invoke();
        } else {
            onFail.Invoke();
        }
    }
}