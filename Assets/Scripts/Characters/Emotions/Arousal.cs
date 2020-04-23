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
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Arousal", 5);
            //temporary opinion debuff
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Arousal", 65);
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
            SchedulingManager.Instance.AddEntry(dueDate,
                () => witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Arousal", -65), 
                this
            );
        }
        return base.ProcessEmotion(witness, target, status);
    }
    #endregion
}