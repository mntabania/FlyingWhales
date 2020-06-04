using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anger : Emotion {

    public Anger() : base(EMOTION.Anger) {
        responses = new[] {"Angry"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        if(target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Anger", -50);
            //temporary opinion debuff
            //witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Anger", -40);
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(8));
            SchedulingManager.Instance.AddEntry(dueDate,
                () => witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Anger", 40), 
                this
            );
            
            witness.traitContainer.AddTrait(witness, "Angry");
            //if(UnityEngine.Random.Range(0, 100) < 25) {
            //    int chance = UnityEngine.Random.Range(0, 3);
            //    if(chance == 0) {
            //        witness.jobComponent.CreateKnockoutJob(targetCharacter);
            //    }else if (chance == 1) {
            //        witness.jobComponent.CreateKillJob(targetCharacter);
            //    } else if (chance == 2) {
            //        witness.jobComponent.CreateUndermineJob(targetCharacter, "normal");
            //    }
            //}
        } else if (target is TileObject tileObject) {
            if (UnityEngine.Random.Range(0, 100) < 25) {
                witness.combatComponent.Fight(tileObject, CombatManager.Anger);
            }
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}
