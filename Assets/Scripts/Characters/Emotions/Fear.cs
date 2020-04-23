using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fear : Emotion {

    public Fear() : base(EMOTION.Fear) {
        mutuallyExclusive = new string[] { "Threatened" };
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        witness.traitContainer.AddTrait(witness, "Spooked");
        //Fight or Flight, Flight
        witness.combatComponent.Flight(target, "saw something frightening");
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}