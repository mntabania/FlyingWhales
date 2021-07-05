using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SeizeCharacterData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SEIZE_CHARACTER;
    public override string name => "Seize Villager";
    public override string description => "This Action can be used to take a Villager and then transfer it to an unoccupied tile.";
    public SeizeCharacterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        PlayerManager.Instance.player.seizeComponent.SeizePOI(targetPOI);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if(targetCharacter.race == RACE.TRITON) {
                return false;
            }
            if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
                return false;
            }
            if (targetCharacter.interruptComponent.isInterrupted) {
                if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed ||
                    targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                    //do not allow characters being tortured or brainwashed to be seized
                    return false;
                }
            }
            return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && 
                   targetCharacter.hasMarker && targetCharacter.grave == null;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.race == RACE.TRITON) {
            reasons += "Tritons cannot be seized,";
        }
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Characters being drained cannot be seized.";
        }
        return reasons;
    }
    #endregion
}