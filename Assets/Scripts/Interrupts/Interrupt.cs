using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Interrupt : ICrimeable {
        public INTERRUPT interrupt { get; protected set; }
        public string name { get; protected set; }
        public int duration { get; protected set; }
        public bool isSimulateneous { get; protected set; }
        public bool doesStopCurrentAction { get; protected set; }
        public bool doesDropCurrentJob { get; protected set; }
        public string interruptIconString { get; protected set; }
        public bool isIntel { get; protected set; }

        public Interrupt(INTERRUPT interrupt) {
            this.interrupt = interrupt;
            this.name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(interrupt.ToString());
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Virtuals
        public virtual bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) { return false; }
        public virtual bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) { return false; }
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
        public virtual Log CreateEffectLog(Character actor, IPointOfInterest target, string key) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, key)) {
                Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, key);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                return effectLog;
            }
            return null;
        }
        #endregion
    }

    public class InterruptHolder : IReactable, IRumorable {
        public Interrupt interrupt { get; private set; }
        public Character actor { get; private set; }
        public IPointOfInterest target { get; private set; }
        public Log effectLog { get; private set; }
        public Rumor rumor { get; private set; }
        public List<Character> awareCharacters { get; private set; }

        #region getters
        public string name => interrupt.name;
        public string typeName => "Interrupt";
        public Log informationLog => effectLog;
        public bool isStealth => false;
        public bool isRumor => rumor != null;
        #endregion

        public InterruptHolder(Interrupt interrupt, Character actor, IPointOfInterest target, Log effectLog) {
            this.interrupt = interrupt;
            this.actor = actor;
            this.target = target;
            this.effectLog = effectLog;
            awareCharacters = new List<Character>();
        }

        #region IReactable
        public string ReactionToActor(Character witness, REACTION_STATUS status) {
            return interrupt.ReactionToActor(witness, actor, target, interrupt, status);
        }

        public string ReactionToTarget(Character witness, REACTION_STATUS status) {
            return interrupt.ReactionToTarget(witness, actor, target, interrupt, status);
        }

        public string ReactionOfTarget(REACTION_STATUS status) {
            return interrupt.ReactionOfTarget(actor, target, interrupt, status);
        }
        public REACTABLE_EFFECT GetReactableEffect(Character witness) {
            return REACTABLE_EFFECT.Neutral;
        }
        public void AddAwareCharacter(Character character) {
            awareCharacters.Add(character);
        }
        #endregion

        #region IRumorable
        public void SetAsRumor(Rumor newRumor) {
            if (rumor != newRumor) {
                rumor = newRumor;
            }
        }
        #endregion
    }
}