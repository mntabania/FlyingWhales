using System.Linq;
using UtilityScripts;

namespace Traits {
    public class Agoraphobic : Trait {
        public override bool isSingleton => true;

        public bool hasReactedThisTick;
        public Agoraphobic() {
            name = "Agoraphobic";
            description = "Crowds? Oh no!";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            hasReactedThisTick = false;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                ApplyAgoraphobicEffect(character);
                character.traitComponent.SubscribeToAgoraphobiaLevelUpSignal();
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
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
                if (hasReactedThisTick) {
                    return false;
                }
                if (ApplyAgoraphobicEffect(characterThatWillDoJob)) {
                    hasReactedThisTick = true;
                    GameDate date = GameManager.Instance.Today();
                    date.AddTicks(1);
                    SchedulingManager.Instance.AddEntry(date, () => hasReactedThisTick = false, this);
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

        private bool ApplyAgoraphobicEffect(Character character) {
            if (!character.limiterComponent.canWitness) { //!character.limiterComponent.canPerform || 
                return false;
            }
            if(!WillTriggerAgoraphobia(character)) {
                return false;
            }
            character.jobQueue.CancelAllJobs();
            string debugLog = $"{character.name} Is agoraphobic and has a crowd in vision. Character became anxious.";

            bool shouldAddAnxiousTrait = true;
            if (character.HasAfflictedByPlayerWith(name)) {
                shouldAddAnxiousTrait = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA).currentLevel >= 1;
            }
            if (shouldAddAnxiousTrait) {
                character.traitContainer.AddTrait(character, "Anxious");
            }
            if (GameUtilities.RollChance(10)) {
                debugLog += $"{character.name} became catatonic";
                character.traitContainer.AddTrait(character, "Catatonic");
            } else if (GameUtilities.RollChance(15)) {
                debugLog += $"{character.name} became berserked";
                character.traitContainer.AddTrait(character, "Berserked");
            } else if (GameUtilities.RollChance(15)) {
                debugLog += $"{character.name} Had a seizure";
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, character);
            } else if (GameUtilities.RollChance(10) && (character.characterClass.className == "Druid" || character.characterClass.className == "Shaman" || character.characterClass.className == "Mage")) {
                debugLog += $"{character.name} Had a loss of control";
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, character);
            } else {
                debugLog += $"{character.name} became anxious and is cowering.";
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: "Agoraphobic");
            }
            character.logComponent.PrintLogIfActive(debugLog);
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

