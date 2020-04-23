using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gratefulness : Emotion {

    public Gratefulness() : base(EMOTION.Gratefulness) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Gratefulness", 10);
            //temporary opinion debuff
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Gratefulness", 40);
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(24));
            SchedulingManager.Instance.AddEntry(dueDate,
                () => witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Gratefulness", -40), 
                this
            );
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}