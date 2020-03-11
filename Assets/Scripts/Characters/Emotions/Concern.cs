using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Concern : Emotion {

    public Concern() : base(EMOTION.Concern) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        witness.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Concerned, target);
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}