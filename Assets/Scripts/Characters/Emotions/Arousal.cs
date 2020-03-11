using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arousal : Emotion {

    public Arousal() : base(EMOTION.Arousal) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Arousal", 4);
        }
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}