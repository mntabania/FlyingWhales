using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Threatened : Emotion {

    public Threatened() : base(EMOTION.Threatened) {
        mutuallyExclusive = new string[] { "Fear" };
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        //Fight or Flight
        witness.combatComponent.FightOrFlight(target, CombatManager.Threatened);
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}