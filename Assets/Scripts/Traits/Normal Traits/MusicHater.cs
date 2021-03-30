using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class MusicHater : Trait {
        private Character owner;

        public MusicHater() {
            name = "Music Hater";
            description = "Has an irrational hate for music.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            mutuallyExclusive = new string[] { "Music Lover" };
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost) {
            if (action == INTERACTION_TYPE.SING) {
                cost += 2000;
            } else if (action == INTERACTION_TYPE.PLAY_GUITAR) {
                cost += 2000;
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Guitar guitar) {
                if (guitar.IsOwnedBy(characterThatWillDoJob)) {
                    return characterThatWillDoJob.jobComponent.TriggerDestroy(targetPOI);
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                CheckIfShouldListenToLevelUpEvent(character);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                UnsubscribeToLevelUpEvent(character);
                owner = null;
            }
            Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnCharacterStartedPerformingAction);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                CheckIfShouldListenToLevelUpEvent(character);
            }
        }
        protected override void OnAfflictionLeveledUp(SkillData p_skillData, PlayerSkillData p_playerSkillData) {
            base.OnAfflictionLeveledUp(p_skillData, p_playerSkillData);
            if (p_playerSkillData.afflictionUpgradeData.HasAddedBehaviourForLevel(AFFLICTION_SPECIFIC_BEHAVIOUR.Knockout_Singers_Guitar_Players, p_skillData.currentLevel)) {
                Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnCharacterStartedPerformingAction);
            }
        }
        #endregion

        #region Listeners
        private void OnCharacterStartedPerformingAction(ActualGoapNode p_action) {
            if (owner.hasMarker && (p_action.goapType == INTERACTION_TYPE.SING || p_action.goapType == INTERACTION_TYPE.PLAY_GUITAR) && 
                owner.marker.IsPOIInVision(p_action.actor)) {
                if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.MUSIC_HATER, AFFLICTION_SPECIFIC_BEHAVIOUR.Murder_Singers_Guitar_Players)) {
                    owner.combatComponent.Fight(p_action.actor, CombatManager.Music_Hater, isLethal: true);
                } else if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.MUSIC_HATER, AFFLICTION_SPECIFIC_BEHAVIOUR.Knockout_Singers_Guitar_Players)) {
                    owner.combatComponent.Fight(p_action.actor, CombatManager.Music_Hater, isLethal: false);
                }
            }
        }
        #endregion
    }
}

