using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disapproval : Emotion {

    public Disapproval() : base(EMOTION.Disapproval) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Disapproval", -4);
            if(status == REACTION_STATUS.WITNESSED) {
                targetCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, witness);
            }
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}