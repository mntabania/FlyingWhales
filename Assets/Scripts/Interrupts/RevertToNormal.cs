using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class RevertToNormal : Interrupt {
        public RevertToNormal() : base(INTERRUPT.Revert_To_Normal) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.lycanData.RevertToNormal();
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            Character originalForm = actor.lycanData.originalForm;
            if (!witness.isLycanthrope) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, originalForm, status);
                // response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm);

                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(originalForm);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                    opinionLabel == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, originalForm, status);
                }
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, originalForm, status);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm, status);
                }
                CrimeManager.Instance.ReactToCrime(witness, originalForm, this, CRIME_TYPE.HEINOUS);
            }
            return response;
        }
        #endregion
    }
}
