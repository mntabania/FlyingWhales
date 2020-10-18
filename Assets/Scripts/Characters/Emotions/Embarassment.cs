using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Embarassment : Emotion {

    public Embarassment() : base(EMOTION.Embarassment) {
        responses = new[] {"Embarrassed"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        witness.needsComponent.AdjustHope(-5);
        witness.traitContainer.AddTrait(witness, "Ashamed");
        
        if (witness.marker.IsPOIInVision(target)) {
            //If source of of Embarrassment is within vision, will trigger Flight
            witness.combatComponent.Flight(target, "felt embarrassed");    
        }
        
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}