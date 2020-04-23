using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Embarassment : Emotion {

    public Embarassment() : base(EMOTION.Embarassment) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        witness.needsComponent.AdjustComfort(-15);
        witness.traitContainer.AddTrait(witness, "Ashamed");
        //Fight or Flight, Flight
        witness.combatComponent.Flight(target, "saw something embarassing");
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}