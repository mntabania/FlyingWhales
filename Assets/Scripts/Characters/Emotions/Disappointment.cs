using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disappointment : Emotion {

    public Disappointment() : base(EMOTION.Disappointment) {
        responses = new[] {"Disappointed"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Disappointment", -4);
            witness.traitContainer.AddTrait(witness, "Annoyed");
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}