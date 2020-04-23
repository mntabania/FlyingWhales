using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disgust : Emotion {

    public Disgust() : base(EMOTION.Disgust) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Disgust", -10);
        }
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}