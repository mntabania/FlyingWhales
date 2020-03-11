using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Interrupt : IReactable {
        public INTERRUPT interrupt { get; protected set; }
        public string name { get; protected set; }
        public int duration { get; protected set; }
        public bool isSimulateneous { get; protected set; }
        public bool doesStopCurrentAction { get; protected set; }
        public bool doesDropCurrentJob { get; protected set; }
        public string interruptIconString { get; protected set; }

        public Interrupt(INTERRUPT interrupt) {
            this.interrupt = interrupt;
            this.name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(interrupt.ToString());
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Virtuals
        public virtual bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) { return false; }
        public virtual bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) { return false; }
        public virtual string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) { return string.Empty; }
        public virtual string ReactionToTarget(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) { return string.Empty; }
        public virtual string ReactionOfTarget(Character actor, IPointOfInterest target, Interrupt interrupt,
            REACTION_STATUS status) { return string.Empty; }
        public virtual Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect");
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                return effectLog;
            }
            return null;
        }
        #endregion
    }
}