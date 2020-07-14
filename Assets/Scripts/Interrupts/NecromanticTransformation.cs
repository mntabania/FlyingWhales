using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class NecromanticTransformation : Interrupt {
        public NecromanticTransformation() : base(INTERRUPT.Necromantic_Transformation) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            return target.traitContainer.AddTrait(target, "Necromancer");
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                string adjective = "treacherous";
                if (actor.traitContainer.HasTrait("Evil")) {
                    adjective = "evil";
                }
                Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect");
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(null, adjective, LOG_IDENTIFIER.STRING_1);
                return effectLog;
            }
            return null;
        }
        #endregion
    }
}