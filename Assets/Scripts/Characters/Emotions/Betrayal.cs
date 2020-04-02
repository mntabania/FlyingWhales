using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Betrayal : Emotion {

    public Betrayal() : base(EMOTION.Betrayal) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Base", -60);
            witness.traitContainer.AddTrait(witness, "Betrayed");
            if (UnityEngine.Random.Range(0, 100) < 50) {
                int chance = UnityEngine.Random.Range(0, 3);
                if (chance == 0) {
                    witness.jobComponent.CreateKnockoutJob(targetCharacter);
                } else if (chance == 1) {
                    witness.jobComponent.CreateKillJob(targetCharacter);
                } else if (chance == 2) {
                    witness.jobComponent.CreateUndermineJob(targetCharacter, "normal");
                }
            }
        }
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}
