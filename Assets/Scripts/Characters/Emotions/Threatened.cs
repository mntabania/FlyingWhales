using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Threatened : Emotion {

    public Threatened() : base(EMOTION.Threatened) {
        mutuallyExclusive = new string[] { "Fear" };
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null, string reason = "") {
        //Fight or Flight
        if (witness.isNormalCharacter && target is Character targetCharacter) {
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Threatened", -8, "was threatened");
        }
        if(witness.marker && witness.marker.IsPOIInVision(target)) {
            bool attackBecauseOfCrime = false;
            if (goapNode != null && goapNode.crimeType != CRIME_TYPE.None && goapNode.crimeType != CRIME_TYPE.Unset) {
                attackBecauseOfCrime = true;
            }
            witness.combatComponent.FightOrFlight(target, CombatManager.Threatened, willAttackBecauseOfCrime: attackBecauseOfCrime);
                //if (witness.moodComponent.moodState == MOOD_STATE.Critical) {
                //    witness.combatComponent.FightOrFlight(target, CombatManager.Threatened);
                //} else if (witness.moodComponent.moodState == MOOD_STATE.Bad) {
                //    if (UnityEngine.Random.Range(0, 2) == 0) {
                //        witness.combatComponent.FightOrFlight(target, CombatManager.Threatened);
                //    } else {
                //        witness.combatComponent.Flight(target, "got threatened");
                //    }
                //} else {
                //    witness.combatComponent.Flight(target, "got threatened");
                //}
        }
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}