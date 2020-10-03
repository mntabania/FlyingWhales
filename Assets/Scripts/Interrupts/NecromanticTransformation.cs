using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class NecromanticTransformation : Interrupt {
        public NecromanticTransformation() : base(INTERRUPT.Necromantic_Transformation) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Player, LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            return interruptHolder.target.traitContainer.AddTrait(interruptHolder.target, "Necromancer");
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                string adjective = "treacherous";
                if (actor.traitContainer.HasTrait("Evil")) {
                    adjective = "evil";
                }
                Log effectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(null, adjective, LOG_IDENTIFIER.STRING_1);
                return effectLog;
            }
            return default;
        }
        #endregion
    }
}