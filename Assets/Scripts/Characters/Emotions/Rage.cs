﻿
public class Rage : Emotion {

    public Rage() : base(EMOTION.Rage) {
        responses = new[] {"Enraged"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        if(target is Character targetCharacter) {
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Base", -25);
            // //temporary opinion debuff
            // //witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Anger", -40);
            // GameDate dueDate = GameManager.Instance.Today();
            // dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(8));
            // SchedulingManager.Instance.AddEntry(dueDate,
            //     () => witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Anger", 40), 
            //     this
            // );
            //
            witness.traitContainer.AddTrait(witness, "Angry", targetCharacter);
            //int chance = UnityEngine.Random.Range(0, 2);
            //if(chance == 0) {
            //    witness.jobComponent.CreateKnockoutJob(targetCharacter);
            //} else {
            //    witness.jobComponent.CreateKillJob(targetCharacter);
            //}
            if (witness.marker && witness.marker.inVisionCharacters.Contains(targetCharacter)) {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Angry, targetCharacter);
            }
        } else if (target is TileObject tileObject) {
            witness.combatComponent.Fight(tileObject, CombatManager.Rage);
            if (witness.marker && witness.marker.inVisionTileObjects.Contains(tileObject)) {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Angry, tileObject);
            }
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}

