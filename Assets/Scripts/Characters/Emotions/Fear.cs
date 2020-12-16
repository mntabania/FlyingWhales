﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fear : Emotion {

    public Fear() : base(EMOTION.Fear) {
        mutuallyExclusive = new string[] { "Threatened" };
        responses = new[] {"Afraid"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null, string reason = "") {
        witness.traitContainer.AddTrait(witness, "Spooked", target as Character);
        //Fight or Flight, Flight
        witness.combatComponent.Flight(target, "saw something frightening");
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}