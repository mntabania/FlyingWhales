using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sadness : Emotion {

    public Sadness() : base(EMOTION.Sadness) {
        responses = new[] {"Sad"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        witness.needsComponent.AdjustHappiness(-10);
        witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, target, "feeling sad");
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}