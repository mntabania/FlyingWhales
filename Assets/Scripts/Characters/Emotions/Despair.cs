using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despair : Emotion {

    public Despair() : base(EMOTION.Despair) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        witness.needsComponent.AdjustHope(-10);
        witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, witness, "feeling despair");
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}