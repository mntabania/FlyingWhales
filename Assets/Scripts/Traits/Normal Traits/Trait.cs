using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interrupts;
using Object_Pools;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps;
using UnityEngine.Localization.Settings;

namespace Traits {
    [System.Serializable]
    public class Trait : IMoodModifier, ISavable, IContextMenuItem {
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
        public List<RESISTANCE> resistancesType;
        public List<float> resistancesValue;
        public List<string> traitOverrideFunctionIdentifiers { get; protected set; }

        public virtual string localizedName => LocalizationSettings.StringDatabase.GetLocalizedString("TraitsNameAndDescription_Table", name);
        public virtual string localizedDescription => $"{LocalizationSettings.StringDatabase.GetLocalizedString("TraitsNameAndDescription_Table", name + "_Description")}";

        /// <summary>
        /// Persistent ID of this trait. NOTE: Only instanced traits use this.
        /// </summary>
        public string persistentID { get; private set; }
        public OBJECT_TYPE objectType => OBJECT_TYPE.Trait;
        public List<Character> responsibleCharacters { get; protected set; }
        //public ActualGoapNode gainedFromDoing { get; protected set; } //what action was this poi involved in that gave it this trait.
        public INTERACTION_TYPE gainedFromDoingType { get; protected set; }
        public bool isGainedFromDoingStealth { get; protected set; }
        //public PlayerDamageAccumulator playerDamageAccumulator { get; private set; }

        #region Getters
        public virtual Type serializedData => typeof(SaveDataTrait);
        public Character responsibleCharacter => responsibleCharacters?.FirstOrDefault();
        public string modifierName => name;
        public int moodModifier => moodEffect;
        public string descriptionInUI => GetDescriptionInUI();
        public virtual bool isPersistent => false; //should this trait persist through all a character's alter egos
        public virtual bool isSingleton => false;
        public Sprite contextMenuIcon => null;
        public string contextMenuName => name;
        public int contextMenuColumn => 1;
        public List<IContextMenuItem> subMenus => null;
        /// <summary>
        /// Does this trait affect the characters name icon?
        /// Examples: Cultist, Necromancer
        /// </summary>
        public virtual bool affectsNameIcon => false;
        //public bool isPlayerDamage => playerDamageAccumulator != null;
        #endregion
        
        #region Initialization
        public void InitializeInstancedTrait() {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
            DatabaseManager.Instance.traitDatabase.RegisterTrait(this);
        }
        public virtual void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            persistentID = saveDataTrait.persistentID;
            gainedFromDoingType = saveDataTrait.gainedFromDoingType;
            isGainedFromDoingStealth = saveDataTrait.isGainedFromDoingStealth;
            Assert.IsFalse(string.IsNullOrEmpty(persistentID), $"Trait {saveDataTrait.name} does not have a persistent ID!");
            DatabaseManager.Instance.traitDatabase.RegisterTrait(this);
        }
        public virtual void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            //if (!string.IsNullOrEmpty(p_saveDataTrait.gainedFromDoing)) {
            //    gainedFromDoing = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(p_saveDataTrait.gainedFromDoing);    
            //}
            if (p_saveDataTrait.responsibleCharacters != null) {
                responsibleCharacters = SaveUtilities.ConvertIDListToCharacters(p_saveDataTrait.responsibleCharacters);    
            }
        }
        #endregion

        #region Mood Effects
        public void ApplyMoodEffects(ITraitable addedTo, GameDate expiryDate, Character characterResponsible) {
            if(addedTo is Character character) {
                if (moodEffect != 0) {
                    character.moodComponent.AddMoodEffect(moodEffect, this, expiryDate, characterResponsible);    
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
                    if (!character.equipmentComponent.HasEquips()) {
                        character.combatComponent.SetElementalType(elementalType);
                    }
                    character.combatComponent.elementalStatusWaitingList.Add(elementalType);
                }
            }
        }
        public virtual void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                // character.moodComponent.RemoveMoodEffect(-moodEffect, this);
                if (elementalType != ELEMENTAL_TYPE.Normal) {
                    character.combatComponent.elementalStatusWaitingList.Remove(elementalType);
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
            string data = string.Empty;
            if (responsibleCharacters != null) {
                data = $"Responsible Characters: {responsibleCharacters.ComafyList()}\n";
            }
            return data;
        }
        public virtual bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the target's traits?
        public virtual bool OnCollideWith(IPointOfInterest collidedWith, IPointOfInterest owner) { return false; }
        public virtual void OnEnterGridTile(IPointOfInterest poiWhoEntered, IPointOfInterest owner) { }
        public virtual void OnInitiateMapObjectVisual(ITraitable traitable) { }
        public virtual void OnDestroyMapObjectVisual(ITraitable traitable) { }
        public virtual bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the his/her own traits, considering the target?
        public virtual void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character) { }
        public virtual void OnOwnerInitiallyPlaced(Character owner) { }
        public virtual bool PerTickWhileStationaryOrUnoccupied(Character p_character) { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
        public virtual bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
        public virtual void OnBeforeStartFlee(ITraitable traitable) { }
        public virtual void OnAfterExitingCombat(ITraitable traitable) { }
        //Returns the string of the log key that's supposed to be logged
        public virtual string TriggerFlaw(Character character) {
            if (character.trapStructure.IsTrapped()) {
                //clear all trap structures when triggering flaw
                character.trapStructure.ResetAllTrapStructures();
            }
            if (character.trapStructure.IsTrappedInArea()) {
                character.trapStructure.ResetTrapArea();
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
                && character.limiterComponent.canPerform
                && !character.traitContainer.IsBlessed()
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
            if (character.traitContainer.IsBlessed()) {
                reasons.Add("Blessed characters cannot be targeted by Trigger Flaw.");
            }
            if (!character.limiterComponent.canPerform) {
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
        public virtual void AfterDeath(Character character) { }
        #endregion

        #region Utilities
        public virtual string GetTriggerFlawEffectDescription(Character character, string key) {
            if (LocalizationManager.Instance.HasLocalizedValue("Trait", name, key)) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, key, null, LOG_TAG.Player);
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.FinalizeText();
                string logText = log.logText;
                LogPool.Release(log);
                return logText;
            }
            return string.Empty;
        }
        public void SetGainedFromDoingAction(INTERACTION_TYPE p_actionType, bool p_actionStealth) {
            gainedFromDoingType = p_actionType;
            isGainedFromDoingStealth = p_actionStealth;
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
        public void ClearResponsibleCharacters() {
            if (responsibleCharacters != null) {
                responsibleCharacters.Clear();
            }
        }
        public bool IsResponsibleForTrait(Character character) {
            if(character == null) {
                return false;
            }
            if (responsibleCharacter == character) {
                return true;
            }
            if (responsibleCharacters != null) {
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
                    currentJob.CancelJob();
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
        public bool IsNeeds() {
            return name == "Refreshed" || name == "Exhausted" || name == "Tired"
                || name == "Entertained" || name == "Bored" || name == "Sulking"
                || name == "Full" || name == "Hungry" || name == "Starving"
                || name == "Sprightly" || name == "Spent" || name == "Drained";
        }
        public PLAYER_SKILL_TYPE GetAfflictionSkillType() {
            return PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(name);
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
        //public virtual void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref List<GoapEffect> effects) { }
        public virtual void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) { }
        public virtual void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) { }
        public virtual void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved) { }
        #endregion

        #region IContextMenuItem Implementation
        public void OnPickAction() {
            if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character targetCharacter) {
                if(UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction playerAction) {
                    if (playerAction.type == PLAYER_SKILL_TYPE.TRIGGER_FLAW) {
                        ActivateTriggerFlawConfirmation(targetCharacter);
                    } else if (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF) {
                        (playerAction as RemoveBuffData).ActivateRemoveBuff(name, targetCharacter);
                    } else if (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW) {
                        (playerAction as RemoveFlawData).ActivateRemoveFlaw(name, targetCharacter);
                    }
                }
            }
        }
        public bool CanBePickedRegardlessOfCooldown() {
            if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character targetCharacter) {
                if (UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction playerAction) {
                    if (playerAction.type == PLAYER_SKILL_TYPE.TRIGGER_FLAW) {
                        return CanFlawBeTriggered(targetCharacter);
                    } else if (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF || playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW) {
                        return !targetCharacter.isDead;
                    }
                }
            }
            return true;
        }
        public bool IsInCooldown() {
            return false;
        }
        public float GetCoverFillAmount() {
            return 0f;
        }
        public int GetCurrentRemainingCooldownTicks() {
            return 0;
        }
        private void ActivateTriggerFlawConfirmation(Character p_character) {
            string traitName = name;
            Trait trait = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
            string question;
            string effect;
            if (p_character.isInfoUnlocked) {
                question = "Are you sure you want to trigger " + traitName + "?";
                effect = $"<b>Effect</b>: {trait.GetTriggerFlawEffectDescription(p_character, "flaw_effect")}";
            } else {
                question = "Are you sure you want to trigger ?????" + "?";
                effect = $"<b>Effect</b>: ?????";
            }
            
            string manaCost = $"{PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}";

            UIManager.Instance.ShowTriggerFlawConfirmation(question, effect, manaCost, () => TriggerFlawData.ActivateTriggerFlaw(trait, p_character), layer: 26, showCover: true, pauseAndResume: true);
        }
        public int GetManaCost() {
            if (UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction playerAction) {
                if (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF || playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW) {
                    return 0;
                }
            }
            return PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost;
        }
        #endregion
        
        #region IMoodModifier Implementation
        public Log GetMoodEffectFlavorText(Character p_characterResponsible) {
            if (LocalizationManager.Instance.HasLocalizedValue("Trait", name, "mood_effect")) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, "mood_effect");
                if (p_characterResponsible != null) {
                    log.AddToFillers(p_characterResponsible, p_characterResponsible.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);    
                } else {
                    //if no character was passed assume that the player is the one that applied the status
                    log.AddToFillers(null, "Demons", LOG_IDENTIFIER.ACTIVE_CHARACTER);
                }
                return log;
            }
            return default;
        }
        #endregion

        #region Chaos Orbs
        public void DispenseChaosOrbsForAffliction(Character p_character, int amount) {
            LocationGridTile gridTile = p_character.gridTileLocation;
            if (p_character.isDead) {
                gridTile = p_character.deathTilePosition;
            }
            if (gridTile != null) {
#if DEBUG_LOG
                Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [" + name + "/Affliction] - [" + amount + "]");
#endif
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTile.centeredWorldLocation, amount, gridTile.parentMap);
            }
        }
        #endregion

        #region Reactions
        public virtual void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog) { }
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