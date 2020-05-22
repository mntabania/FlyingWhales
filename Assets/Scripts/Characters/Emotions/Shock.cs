using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shock : Emotion {

    public Shock() : base(EMOTION.Shock) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        witness.needsComponent.AdjustHappiness(-10);
        if(status == REACTION_STATUS.WITNESSED) {
            if(UnityEngine.Random.Range(0, 100) < 30) {
                witness.combatComponent.Flight(target, "saw something shocking");
            } else {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Surprised, witness);
            }
        } else {
            witness.interruptComponent.TriggerInterrupt(INTERRUPT.Surprised, witness);
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}