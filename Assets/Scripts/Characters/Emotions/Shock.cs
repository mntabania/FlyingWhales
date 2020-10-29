using System.Collections;
using System.Collections.Generic;
using Interrupts;
using UnityEngine;

public class Shock : Emotion {

    public Shock() : base(EMOTION.Shock) {
        responses = new[] {"Shocked"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status, ActualGoapNode goapNode = null, string reason = "") {
        witness.needsComponent.AdjustHappiness(-10);
        string reasonToUse = string.IsNullOrEmpty(reason) ? Surprised.Witness_Reason : reason;
        if(status == REACTION_STATUS.WITNESSED) {
            if(UnityEngine.Random.Range(0, 100) < 30) {
                witness.combatComponent.Flight(target, "shocked");
            } else {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Surprised, target, reason: reasonToUse, identifier: "Shocked");
            }
        } else {
            witness.interruptComponent.TriggerInterrupt(INTERRUPT.Surprised, target, reason: reasonToUse, identifier: "Shocked");
        }
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}