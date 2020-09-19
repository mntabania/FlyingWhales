using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Interrupts;
using Logs;
namespace Interrupts {
    public class Interrupt {
        public INTERRUPT type { get; protected set; }
        public string name { get; protected set; }
        public int duration { get; protected set; }
        public bool isSimulateneous { get; protected set; }
        public bool doesStopCurrentAction { get; protected set; }
        public bool doesDropCurrentJob { get; protected set; }
        public string interruptIconString { get; protected set; }
        public bool isIntel { get; protected set; }
        public bool shouldAddLogs { get; protected set; } //Does this interrupt add logs to the involved characters
        public bool shouldShowNotif { get; protected set; }
        public LOG_TAG[] logTags { get; protected set; }

        protected Interrupt(INTERRUPT type) {
            this.type = type;
            this.name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(type.ToString());
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.No_Icon;
            shouldAddLogs = true;
            shouldShowNotif = true;
        }

        #region Virtuals
        public virtual bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) { return false; }
        public virtual bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) { return false; }

        public virtual string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) { return string.Empty; }
        public virtual string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) { return string.Empty; }
        public virtual string ReactionOfTarget(Character actor, IPointOfInterest target, InterruptHolder interrupt, REACTION_STATUS status) { return string.Empty; }
        public virtual Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                return effectLog;
            }
            return default;
        }
        public virtual Log CreateEffectLog(Character actor, IPointOfInterest target, string key) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, key)) {
                Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, key, null, logTags);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                return effectLog;
            }
            return default;
        }
        public virtual void AddAdditionalFillersToThoughtLog(Log log, Character actor){ }
        public virtual CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime) {
            return CRIME_TYPE.None;
        }
        #endregion
    }

    public class InterruptHolder : IRumorable, ICrimeable, ISavable {
        public string persistentID { get; private set; }
        public Interrupt interrupt { get; private set; }
        public Character actor { get; private set; }
        public IPointOfInterest target { get; private set; }
        public Character disguisedActor { get; private set; }
        public Character disguisedTarget { get; private set; }
        public Log effectLog { get; private set; }
        public string identifier { get; private set; }
        public Rumor rumor { get; private set; }
        public List<Character> awareCharacters { get; private set; }
        public string reason { get; private set; }
        public CRIME_TYPE crimeType { get; private set; }

        #region getters
        public string name => interrupt.name;
        public string classificationName => "News";
        public Log informationLog => effectLog;
        public bool isStealth => false;
        public bool isRumor => rumor != null;
        public RUMOR_TYPE rumorType => RUMOR_TYPE.Interrupt;
        public CRIMABLE_TYPE crimableType => CRIMABLE_TYPE.Interrupt;
        public OBJECT_TYPE objectType => OBJECT_TYPE.Interrupt;
        public System.Type serializedData => typeof(SaveDataInterruptHolder);
        public LOG_TAG[] logTags => interrupt.logTags;
        #endregion

        public InterruptHolder() {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
            identifier = string.Empty;
            awareCharacters = new List<Character>();
        }
        public InterruptHolder(SaveDataInterruptHolder data) {
            awareCharacters = new List<Character>();
            persistentID = data.persistentID;
            interrupt = InteractionManager.Instance.GetInterruptData(data.interruptType);
            identifier = data.identifier;
            reason = data.reason;
            crimeType = data.crimeType;
        }

        #region General
        public void SetEffectLog(Log effectLog) {
            this.effectLog = effectLog;
        }
        public void SetIdentifier(string identifier) {
            this.identifier = identifier;
        }
        public void SetDisguisedActor(Character disguised) {
            disguisedActor = disguised;
        }
        public void SetDisguisedTarget(Character disguised) {
            disguisedTarget = disguised;
        }
        public void SetReason(string reason) {
            this.reason = reason;
        }
        #endregion

        #region IReactable
        public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
            return interrupt.ReactionToActor(actor, target, witness, this, status);
        }

        public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
            REACTION_STATUS status) {
            return interrupt.ReactionToTarget(actor, target, witness, this, status);
        }

        public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status) {
            return interrupt.ReactionOfTarget(actor, target, this, status);
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
                if (rumor != null) {
                    rumor.SetRumorable(this);
                }
            }
        }
        #endregion

        #region Crime
        public void SetCrimeType() {
            if (crimeType == CRIME_TYPE.Unset) {
                Character actor = this.actor;
                IPointOfInterest poiTarget = target;
                if (this.actor.reactionComponent.disguisedCharacter != null) {
                    actor = this.actor.reactionComponent.disguisedCharacter;
                }
                if (target is Character targetCharacter && targetCharacter.reactionComponent.disguisedCharacter != null) {
                    poiTarget = targetCharacter.reactionComponent.disguisedCharacter;
                }
                crimeType = interrupt.GetCrimeType(actor, target, this);
            }
        }
        #endregion

        #region Object Pool
        public void Initialize(Interrupt interrupt, Character actor, IPointOfInterest target, string identifier, string reason) {
            this.interrupt = interrupt;
            this.actor = actor;
            this.target = target;

            //Whenever a disguised character is being set as actor/target, assign na disguised actor/target
            disguisedActor = actor.reactionComponent.disguisedCharacter;
            if (target is Character targetCharacter) {
                disguisedTarget = targetCharacter.reactionComponent.disguisedCharacter;
            }

            SetIdentifier(identifier);
            SetReason(reason);
        }
        public void Reset() {
            interrupt = null;
            actor = null;
            target = null;
            disguisedActor = null;
            disguisedTarget = null;
            effectLog = default;
            rumor = null;
            identifier = string.Empty;
            crimeType = CRIME_TYPE.Unset;
            awareCharacters.Clear();
        }
        #endregion

        #region Testing
        public override string ToString() {
            return $"Interrupt: {interrupt?.type.ToString() ?? "None"}. Actor: {actor?.name ?? "None"}";
        }
        #endregion

        #region Loading
        public void LoadReferences(SaveDataInterruptHolder data) {
            actor = CharacterManager.Instance.GetCharacterByPersistentID(data.actorID);
            if (!string.IsNullOrEmpty(data.targetID)) {
                if (data.targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    target = CharacterManager.Instance.GetCharacterByPersistentID(data.targetID);
                } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    target = InnerMapManager.Instance.GetTileObjectByPersistentID(data.targetID);
                }    
            }
            disguisedActor = null;
            disguisedTarget = null;
            if (!string.IsNullOrEmpty(data.disguisedActorID)) {
                disguisedActor = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedActorID);
            }
            if (!string.IsNullOrEmpty(data.disguisedTargetID)) {
                disguisedTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedTargetID);
            }

            effectLog = default;
            if (data.effectLog.hasValue) {
                effectLog = data.effectLog;
            }

            if (data.awareCharacterIDs != null) {
                if (data.awareCharacterIDs.Count > 0) {
                    for (int i = 0; i < data.awareCharacterIDs.Count; i++) {
                        Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.awareCharacterIDs[i]);
                        awareCharacters.Add(character);
                    }
                }    
            }

            if (data.hasRumor) {
                rumor = data.rumor.Load();
                rumor.SetRumorable(this);
            }
        }
        #endregion
    }
}


[System.Serializable]
public class SaveDataInterruptHolder : SaveData<InterruptHolder>, ISavableCounterpart {
    public string persistentID { get; set; }
    public INTERRUPT interruptType;
    public string actorID;
    public string targetID;
    public POINT_OF_INTEREST_TYPE targetPOIType;
    public string disguisedActorID;
    public string disguisedTargetID;
    public Log effectLog;
    public string identifier;
    public SaveDataRumor rumor;
    public bool hasRumor;
    public List<string> awareCharacterIDs;
    public string reason;
    public CRIME_TYPE crimeType;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Interrupt;
    #endregion

    #region Overrides
    public override void Save(InterruptHolder data) {
        persistentID = data.persistentID;
        interruptType = data.interrupt.type;
        actorID = data.actor.persistentID;
        if (data.target != null) {
            targetID = data.target.persistentID;
            targetPOIType = data.target.poiType;    
        }
        identifier = data.identifier;
        reason = data.reason;
        crimeType = data.crimeType;

        disguisedActorID = string.Empty;
        disguisedTargetID = string.Empty;
        if (data.disguisedActor != null) {
            disguisedActorID = data.disguisedActor.persistentID;
        }
        if (data.disguisedTarget != null) {
            disguisedTargetID = data.disguisedTarget.persistentID;
        }

        effectLog = default;
        if (data.effectLog.hasValue) {
            effectLog = data.effectLog;
        }
        // if (data.effectLog != null) {
        //     effectLog = data.effectLog.persistentID;
        //     SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.effectLog);
        // }

        if (data.rumor != null) {
            hasRumor = true;
            rumor = new SaveDataRumor();
            rumor.Save(data.rumor);
        }

        awareCharacterIDs = new List<string>();
        if (data.awareCharacters != null && data.awareCharacters.Count > 0) {
            for (int i = 0; i < data.awareCharacters.Count; i++) {
                awareCharacterIDs.Add(data.awareCharacters[i].persistentID);
            }
        }
    }

    public override InterruptHolder Load() {
        InterruptHolder interrupt = new InterruptHolder(this);
        return interrupt;
    }
    #endregion
}