using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shock : Emotion {

    public Shock() : base(EMOTION.Shock) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        witness.needsComponent.AdjustComfort(-10);
        if(status == REACTION_STATUS.WITNESSED) {
            if(UnityEngine.Random.Range(0, 100) < 30) {
                witness.combatComponent.Flight(target, "saw something shocking");
            }
        }
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}