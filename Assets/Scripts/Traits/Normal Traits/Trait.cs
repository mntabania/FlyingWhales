using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interrupts;
using UnityEngine;
using UnityEngine.Assertions;
namespace Traits {
    [System.Serializable]
    public class Trait : IMoodModifier, ISavable{
        //Non-changeable values
        public string name;
        public string description;
        public string thoughtText;
        public TRAIT_TYPE type;
        public TRAIT_EFFECT effect;
        public List<INTERACTION_TYPE> advertisedInteractions;
        public int ticksDuration; //Zero (0) means Permanent
        public int moodEffect;
        public bool isHidden;
        public string[] mutuallyExclusive; //list of traits that this trait cannot be with.
        public bool canBeTriggered;
        public ELEMENTAL_TYPE elementalType;

        /// <summary>
        /// Persistent ID of this trait. NOTE: Only instanced traits use this.
        /// </summary>
        public string persistentID { get; private set; }
        public OBJECT_TYPE objectType => OBJECT_TYPE.Trait;
        public List<Character> responsibleCharacters { get; protected set; }
        public ActualGoapNode gainedFromDoing { get; protected set; } //what action was this poi involved in that gave it this trait.
        public List<string> traitOverrideFunctionIdentifiers { get; protected set; }

        #region Getters
        public virtual Type serializedData => typeof(SaveDataTrait);
        public Character responsibleCharacter => responsibleCharacters?.FirstOrDefault();
        public string moodModificationDescription => name;
        public int moodModifier => moodEffect;
        public string descriptionInUI => GetDescriptionInUI();
        public virtual bool isPersistent => false; //should this trait persist through all a character's alter egos
        public virtual bool isSingleton => false;
        #endregion
        
        #region Initialization
        public void InitializeInstancedTrait() {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
            DatabaseManager.Instance.traitDatabase.RegisterTrait(this);
        }
        public virtual void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            persistentID = saveDataTrait.persistentID;
            Assert.IsFalse(string.IsNullOrEmpty(persistentID), $"Trait {saveDataTrait.name} does not have a persistent ID!");
            DatabaseManager.Instance.traitDatabase.RegisterTrait(this);
        }
        public virtual void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            if (!string.IsNullOrEmpty(saveDataTrait.gainedFromDoing)) {
                gainedFromDoing = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(saveDataTrait.gainedFromDoing);    
            }
        }
        #endregion

        #region Mood Effects
        public void ApplyMoodEffects(ITraitable addedTo, GameDate expiryDate) {
            if(addedTo is Character character) {
                if (moodEffect != 0) {
                    character.moodComponent.AddMoodEffect(moodEffect, this, expiryDate);    
                }
            }
        }
        public void UnapplyMoodEffects(ITraitable removedFrom) {
            if (removedFrom is Character character) {
                if (moodEffect != 0) {
                    character.moodComponent.RemoveMoodEffect(-moodEffect, this);
                }
            }
        }
        #endregion
        
        #region Virtuals
        public virtual void OnAddTrait(ITraitable addedTo) {
            if(addedTo is Character) {
                Character character = addedTo as Character;
                // character.moodComponent.AddMoodEffect(moodEffect, this);
                if (elementalType != ELEMENTAL_TYPE.Normal) {
                    character.combatComponent.SetElementalType(elementalType);
                }
            }
        }
        public virtual void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                // character.moodComponent.RemoveMoodEffect(-moodEffect, this);
                if (elementalType != ELEMENTAL_TYPE.Normal) {
                    character.combatComponent.UpdateElementalType();
                }
            }
            if (TraitManager.Instance.IsInstancedTrait(name)) {
                DatabaseManager.Instance.traitDatabase.UnRegisterTrait(this);    
            }
        }
        /// <summary>
        /// Load version of <see cref="OnAddTrait"/>. Content of this function will almost always be the same as OnAddTrait.
        /// Only difference is stuff that we do not want to apply when loading this trait. (i.e. opinion changes, needs changes, stat changes, etc.)
        /// </summary>
        /// <param name="addTo"></param>
        public virtual void LoadTraitOnLoadTraitContainer(ITraitable addTo) { }
        public virtual void OnRemoveStatusBySchedule(ITraitable removedFrom) { }
        public virtual string GetToolTipText() { return string.Empty; }
        //public virtual bool IsUnique() { return true; }
        /// <summary>
        /// Only used for characters, since traits aren't removed when a character dies.
        /// This function will be called to ensure that any unneeded resources in traits can be freed up when a character dies.
        /// <see cref="Character.Death"/>
        /// </summary>
        /// <param name="character">The character that died.</param>
        /// <returns>If this trait was removed or not.</returns>
        public virtual bool OnDeath(Character character) { return false; }
        /// <summary>
        /// Used to return necessary actions when a character with this trait
        /// returns to life.
        /// </summary>
        /// <param name="character">The character that returned to life.</param>
        //public virtual void OnReturnToLife(Character character) { } //Removed temporarily since this is not being used
        public virtual string GetTestingData(ITraitable traitable = null) {
            return string.Empty;
        }
        public virtual bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the target's traits?
        public virtual bool OnCollideWith(IPointOfInterest collidedWith, IPointOfInterest owner) { return false; }
        public virtual void OnEnterGridTile(IPointOfInterest poiWhoEntered, IPointOfInterest owner) { }
        public virtual void OnInitiateMapObjectVisual(ITraitable traitable) { }
        public virtual void OnDestroyMapObjectVisual(ITraitable traitable) { }
        public virtual bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the his/her own traits, considering the target?
        public virtual void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character) { }
        protected virtual void OnChangeLevel() { }
        public virtual void OnOwnerInitiallyPlaced(Character owner) { }
        public virtual bool PerTickOwnerMovement() { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
        public virtual bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
        public virtual void OnBeforeStartFlee(ITraitable traitable) { }
        public virtual void OnAfterExitingCombat(ITraitable traitable) { }
        //Returns the string of the log key that's supposed to be logged
        public virtual string TriggerFlaw(Character character) {
            if (character.trapStructure.IsTrapped()) {
                //clear all trap structures when triggering flaw
                character.trapStructure.ResetAllTrapStructures();
            }
            if (character.trapStructure.IsTrappedInHex()) {
                character.trapStructure.ResetAllTrapHexes();
            }
            return "flaw_effect";
        }
        /// <summary>
        /// This checks if this flaw can be triggered. This checks both the requirements of the individual traits,
        /// and the mana cost. This is responsible for enabling/disabling the trigger flaw buttton.
        /// </summary>
        /// <param name="character">The character whose flaw will be triggered</param>
        /// <returns>true or false</returns>
        public virtual bool CanFlawBeTriggered(Character character) {
            //return true;
            int manaCost = EditableValuesManager.Instance.triggerFlawManaCost;

            return canBeTriggered && PlayerManager.Instance.player.mana >= manaCost
                && character.canPerform
                //&& !character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER) //disabled characters cannot be triggered
                && !character.traitContainer.HasTrait("Blessed")
                && !character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld; //characters travelling outside cannot be triggered
        }
        public virtual string GetRequirementDescription(Character character) {
            return "Mana cost of triggering this flaw's negative effect depends on the character's mood. The darker the mood, the cheaper the cost.";
        }
        public virtual List<string> GetCannotTriggerFlawReasons(Character character) {
            List<string> reasons = new List<string>();
            if (!canBeTriggered) {
                reasons.Add("It is not a flaw or it has no flaw effect.");
            }
            if (PlayerManager.Instance.player.mana < EditableValuesManager.Instance.triggerFlawManaCost) {
                reasons.Add("You do not have enough mana.");
            }
            if (character.traitContainer.HasTrait("Blessed")) {
                reasons.Add("Blessed characters cannot be targeted by Trigger Flaw.");
            }
            if (!character.canPerform) {
                reasons.Add("Characters that cannot perform cannot be targeted by Trigger Flaw.");
            }
            return reasons;

        }
        public virtual void OnTickStarted(ITraitable traitable) { }
        public virtual void OnTickEnded(ITraitable traitable) { }
        public virtual void OnHourStarted(ITraitable traitable) { }
        public virtual string GetNameInUI(ITraitable traitable) {
            return name;
        }
        protected virtual string GetDescriptionInUI() { return description; }
        #endregion

        #region Utilities
        public virtual string GetTriggerFlawEffectDescription(Character character, string key) {
            if (LocalizationManager.Instance.HasLocalizedValue("Trait", name, key)) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, key, null, LOG_TAG.Player);
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.FinalizeText();
                return log.logText;
            }
            return string.Empty;
        }
        public void SetGainedFromDoing(ActualGoapNode action) {
            gainedFromDoing = action;
        }
        public virtual void AddCharacterResponsibleForTrait(Character character) {
            if (character != null) {
                if (responsibleCharacters == null) {
                    responsibleCharacters = new List<Character>();
                }
                if (!responsibleCharacters.Contains(character)) {
                    responsibleCharacters.Add(character);
                }
            }
        }
        public bool IsResponsibleForTrait(Character character) {
            if (responsibleCharacter == character) {
                return true;
            } else if (responsibleCharacters != null) {
                return responsibleCharacters.Contains(character);
            }
            return false;
        }
        protected bool TryTransferJob(JobQueueItem currentJob, Character characterThatWillDoJob) {
            if (currentJob.originalOwner.ownerType == JOB_OWNER.SETTLEMENT || currentJob.originalOwner.ownerType == JOB_OWNER.FACTION) {
                bool canBeTransfered = false;
                Character assignedCharacter = currentJob.assignedCharacter;
                if (assignedCharacter != null && assignedCharacter.currentActionNode.action != null
                    && assignedCharacter.currentJob != null && assignedCharacter.currentJob == currentJob) {
                    if (assignedCharacter != characterThatWillDoJob) {
                        canBeTransfered = !assignedCharacter.marker.IsPOIInVision(assignedCharacter.currentActionNode.poiTarget);
                    }
                } else {
                    canBeTransfered = true;
                }
                if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                    currentJob.CancelJob(shouldDoAfterEffect: false);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob);
                    return true;
                }
            }
            return false;
        }
        public Trait GetBase() {
            return this;
        }
        public void AddTraitOverrideFunctionIdentifier(string identifier) {
            if(traitOverrideFunctionIdentifiers == null) {
                traitOverrideFunctionIdentifiers = new List<string>();
            }
            if (!traitOverrideFunctionIdentifiers.Contains(identifier)) {
                traitOverrideFunctionIdentifiers.Add(identifier);
            }
        }
        #endregion

        #region Actions
        /// <summary>
        /// If this trait modifies any costs of an action, put it here.
        /// </summary>
        /// <param name="action">The type of action.</param>
        /// <param name="actor"></param>
        /// <param name="poiTarget"></param>
        /// <param name="otherData"></param>
        /// <param name="cost">The cost to be modified.</param>
        public virtual void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost) { }
        public virtual void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref List<GoapEffect> effects) { }
        public virtual void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) { }
        public virtual void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) { }
        public virtual void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) { }
        #endregion

        
    }
}


[System.Serializable]
public class TraitEffect {
    public STAT stat;
    public float amount;
    public bool isPercentage;
    public TRAIT_REQUIREMENT_CHECKER checker;
    public TRAIT_REQUIREMENT_TARGET target;
    public DAMAGE_IDENTIFIER damageIdentifier; //Only used during combat
    public string description;

    public bool hasRequirement;
    public bool isNot;
    public TRAIT_REQUIREMENT requirementType;
    public TRAIT_REQUIREMENT_SEPARATOR requirementSeparator;
    public List<string> requirements;
}