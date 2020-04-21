using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Approval : Emotion {

    public Approval() : base(EMOTION.Approval) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        if (target is Character targetCharacter) {
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Approval", 8);
        }
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}
