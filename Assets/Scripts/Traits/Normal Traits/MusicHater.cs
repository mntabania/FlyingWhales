using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class MusicHater : Trait {
        public override bool isSingleton => true;
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
        #endregion

        public void ReactToMusicPerformer(Character p_witness, Character p_actor) {
            if (p_witness.HasAfflictedByPlayerWith(this)) {
                if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.MUSIC_HATER, AFFLICTION_SPECIFIC_BEHAVIOUR.Murder_Singers_Guitar_Players)) {
                    p_witness.combatComponent.Fight(p_actor, CombatManager.Music_Hater_Murder, isLethal: true);
                } else if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.MUSIC_HATER, AFFLICTION_SPECIFIC_BEHAVIOUR.Knockout_Singers_Guitar_Players)) {
                    p_witness.combatComponent.Fight(p_actor, CombatManager.Music_Hater_Knockout, isLethal: false);
                }    
            }
        }
    }
}

