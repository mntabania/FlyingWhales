using Crime_System;
using Traits;
using UtilityScripts;

namespace Interrupts {
    public class BecomeLycanthrope : Interrupt {
        public BecomeLycanthrope() : base(INTERRUPT.Become_Lycanthrope) {
            duration = 0;
            interruptIconString = GoapActionStateDB.No_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            LycanthropeData lycanthropeData = new LycanthropeData(interruptHolder.actor);
            // interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Lycanthrope");
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
            interruptHolder.actor.UnobtainItem(TILE_OBJECT_TYPE.WEREWOLF_PELT);
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);

            if (status == REACTION_STATUS.INFORMED) {
                actor.lycanData?.AddAwareCharacter(witness);
                CrimeType crimeTypeObj = CrimeManager.Instance.GetCrimeType(interrupt.crimeType);
                CRIME_SEVERITY severity = CRIME_SEVERITY.None;
                if (crimeTypeObj != null) {
                    severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, interrupt.crimeType);
                }
                if (severity == CRIME_SEVERITY.Heinous) {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                    if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(actor)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status);
                    }
                    if (witness.traitContainer.HasTrait("Coward")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status);
                    }
                } else {
                    if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status);
                        } else {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status);
                        }
                    } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status);
                    }
                }
            }
            return response;
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.Werewolf;
        }
        #endregion
    }
}
