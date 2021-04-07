using Crime_System;
using Traits;
using UtilityScripts;
using System.Collections.Generic;

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
            Messenger.Broadcast(CharacterSignals.LYCANTHROPE_SHED_WOLF_PELT, interruptHolder.actor);
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);

            if (status == REACTION_STATUS.INFORMED) {
                actor.lycanData?.AddAwareCharacter(witness);
            }
            return response;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
            if (status == REACTION_STATUS.INFORMED) {
                CrimeType crimeTypeObj = CrimeManager.Instance.GetCrimeType(interrupt.crimeType);
                CRIME_SEVERITY severity = CRIME_SEVERITY.None;
                if (crimeTypeObj != null) {
                    severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, interrupt.crimeType);
                }
                if (severity == CRIME_SEVERITY.Heinous) {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                    if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Despair);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(actor)) {
                        reactions.Add(EMOTION.Despair);
                    }
                    if (witness.traitContainer.HasTrait("Coward")) {
                        reactions.Add(EMOTION.Fear);
                    } else {
                        reactions.Add(EMOTION.Threatened);
                    }
                } else {
                    if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            reactions.Add(EMOTION.Arousal);
                        } else {
                            reactions.Add(EMOTION.Approval);
                        }
                    } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                        reactions.Add(EMOTION.Threatened);
                    }
                }
            }
        }
        public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.Werewolf;
        }
        #endregion
    }
}
