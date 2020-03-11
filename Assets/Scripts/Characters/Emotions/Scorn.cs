using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorn : Emotion {

    public Scorn() : base(EMOTION.Scorn) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (UnityEngine.Random.Range(0, 2) == 0) {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
            } else {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
            }
        }
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}