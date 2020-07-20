using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Interrupt : ICrimeable {
        public INTERRUPT type { get; protected set; }
        public string name { get; protected set; }
        public int duration { get; protected set; }
        public bool isSimulateneous { get; protected set; }
        public bool doesStopCurrentAction { get; protected set; }
        public bool doesDropCurrentJob { get; protected set; }
        public string interruptIconString { get; protected set; }
        public bool isIntel { get; protected set; }
        public bool shouldAddLogs { get; protected set; } //Does this interrupt add logs to the involved characters
        protected Interrupt(INTERRUPT type) {
            this.type = type;
            this.name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(type.ToString());
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.No_Icon;
            shouldAddLogs = true;
        }

        #region Virtuals
        public virtual bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) { return false; }
        public virtual bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, 
            ActualGoapNode goapNode = null) { return false; }

        public virtual string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
            Interrupt interrupt, REACTION_STATUS status) { return string.Empty; }
        public virtual string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
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
        public string identifier { get; private set; }
        public Rumor rumor { get; private set; }
        public List<Character> awareCharacters { get; private set; }

        #region getters
        public string name => interrupt.name;
        public string classificationName => "News";
        public Log informationLog => effectLog;
        public bool isStealth => false;
        public bool isRumor => rumor != null;
        #endregion

        public InterruptHolder() {
            identifier = string.Empty;
            awareCharacters = new List<Character>();
        }

        #region General
        public void SetEffectLog(Log effectLog) {
            this.effectLog = effectLog;
        }
        public void SetIdentifier(string identifier) {
            this.identifier = identifier;
        }
        #endregion

        #region IReactable
        public string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
            REACTION_STATUS status) {
            return interrupt.ReactionToActor(actor, target, witness, interrupt, status);
        }

        public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
            REACTION_STATUS status) {
            return interrupt.ReactionToTarget(actor, target, witness, interrupt, status);
        }

        public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status) {
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

        #region Object Pool
        public void Initialize(Interrupt interrupt, Character actor, IPointOfInterest target, string identifier) {
            this.interrupt = interrupt;
            this.actor = actor;
            this.target = target;
            SetIdentifier(identifier);
        }
        public void Reset() {
            interrupt = null;
            actor = null;
            target = null;
            effectLog = null;
            rumor = null;
            identifier = string.Empty;
            awareCharacters.Clear();
        }
        #endregion
    }
}