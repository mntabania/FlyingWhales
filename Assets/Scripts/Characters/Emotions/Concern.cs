using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Concern : Emotion {

    public Concern() : base(EMOTION.Concern) {
        responses = new[] {"Concerned"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        witness.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Concerned, target);
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}