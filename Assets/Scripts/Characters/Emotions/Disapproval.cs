using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disapproval : Emotion {

    public Disapproval() : base(EMOTION.Disapproval) {
        responses = new[] {"Disapproving"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null, string reason = "") {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Disapproval", -9);
            if(status == REACTION_STATUS.WITNESSED && (goapNode == null || !goapNode.isAssumption)) {
                if (!targetCharacter.combatComponent.isInCombat && !targetCharacter.interruptComponent.isInterrupted) {
                    GoapActionState actionState = goapNode?.currentState;
                    bool shouldStop = goapNode == null || (actionState != null && actionState.duration > 0 && goapNode.currentStateDuration < (actionState.duration - 1));
                    if (shouldStop) {
                        witness.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, targetCharacter, actionThatTriggered: goapNode);
                    }
                }
            }
        }
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}