using System.Linq;
using UtilityScripts;

namespace Traits {
    public class Agoraphobic : Trait {
        public override bool isSingleton => true;

        public Agoraphobic() {
            name = "Agoraphobic";
            description = "Crowds? Oh no! If afflicted by the player, will produce a Chaos Orb each time it sees a crowd.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                character.traitComponent.SubscribeToAgoraphobiaLevelUpSignal();
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                ApplyLeavePartyEffect(character);
                ApplyAgoraphobicEffect(character);
                //CheckIfShouldListenToLevelUpEvent(character);
                character.traitComponent.SubscribeToAgoraphobiaLevelUpSignal();
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                //UnsubscribeToLevelUpEvent(character);
                character.traitComponent.UnsubscribeToAgoraphobiaLevelUpSignal();
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                // //Character targetCharacter = targetPOI as Character;
                // if (characterThatWillDoJob.traitContainer.HasTrait("Berserked")) {
                //     return false;
                // }
                // ApplyAgoraphobicEffect(characterThatWillDoJob);
                if (characterThatWillDoJob.traitComponent.hasAgoraphobicReactedThisTick) {
                    return false;
                }
                if (ApplyAgoraphobicEffect(characterThatWillDoJob)) {
                    characterThatWillDoJob.traitComponent.SetHasAgoraphobicReactedThisTick(true);
                }

                return true;
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            ApplyAgoraphobicEffect(character);
            return base.TriggerFlaw(character);
        }
        #endregion
        private void ApplyLeavePartyEffect(Character character) {
            if (character.HasAfflictedByPlayerWith(name)) {
                int level = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA).currentLevel;
                if (level >= 3) {
                    if (character.partyComponent.hasParty) {
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, character, "Agoraphobic");
                    }
                }
            }
        }
        private bool ApplyAgoraphobicEffect(Character character) {
            if (!character.limiterComponent.canWitness) { //!character.limiterComponent.canPerform || 
                return false;
            }
            if(!WillTriggerAgoraphobia(character)) {
                return false;
            }
            character.jobQueue.CancelAllJobs();
#if DEBUG_LOG
            string debugLog = $"{character.name} is agoraphobic and has a crowd in vision.";
#endif

            bool shouldAddAnxiousTrait = true;
            if (character.HasAfflictedByPlayerWith(name)) {
                int level = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA).currentLevel;
                shouldAddAnxiousTrait = level >= 1;
                if(level >= 3) {
                    if (character.partyComponent.hasParty) {
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, character, "Agoraphobic");
                    }
                }
                DispenseChaosOrbsForAffliction(character, 1);
            }
            if (shouldAddAnxiousTrait) {
#if DEBUG_LOG
                debugLog += $"\n{character.name} became anxious";
#endif
                character.traitContainer.AddTrait(character, "Anxious");
            }
            if (GameUtilities.RollChance(10)) {
#if DEBUG_LOG
                debugLog += $"\n{character.name} became catatonic";
#endif
                character.traitContainer.AddTrait(character, "Catatonic");
            } else if (GameUtilities.RollChance(15)) {
#if DEBUG_LOG
                debugLog += $"\n{character.name} became berserked";
#endif
                character.traitContainer.AddTrait(character, "Berserked");
            } else if (GameUtilities.RollChance(15)) {
#if DEBUG_LOG
                debugLog += $"\n{character.name} Had a seizure";
#endif
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, character);
            } else if (GameUtilities.RollChance(10) && (character.characterClass.className == "Druid" || character.characterClass.className == "Shaman" || character.characterClass.className == "Mage")) {
#if DEBUG_LOG
                debugLog += $"\n{character.name} Had a loss of control";
#endif
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, character);
            } else {
#if DEBUG_LOG
                debugLog += $"\n{character.name} is cowering.";
#endif
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: "Agoraphobic");
            }
#if DEBUG_LOG
            character.logComponent.PrintLogIfActive(debugLog);
#endif
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Agoraphobic", "on_see_first", null, LOG_TAG.Social);
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase(true);
            return true;
        }
        private bool WillTriggerAgoraphobia(Character character) {
            int crowd = 3;
            if (character.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA)) {
                crowd = PlayerSkillManager.Instance.GetAfflictionCrowdNumberPerLevel(PLAYER_SKILL_TYPE.AGORAPHOBIA);
            }

            int count = 0;
            if (character.marker.inVisionCharacters.Count >= crowd) {
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    if (!character.marker.inVisionCharacters[i].isDead) {
                        count++;
                    }
                }
            }
            return count >= crowd;
        }
    }
}

