using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

namespace Traits {
    public class Agitated : Status {
        //public override bool isSingleton => true;

        private float _addedAttackPercent;
        private float _addedHPPercent;
        
        public Agitated() {
            name = "Agitated";
            description = "Will attack neaby village.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                if (character is Ent ent) {
                    ent.EntAgitatedHandling();
                } else if (character is Mimic mimic) {
                    mimic.MimicAgitatedHandling();
                }
                if (character.marker) {
                    character.marker.BerserkedMarker();
                }
                //character.buffStatsBonus.originalAttack = Mathf.RoundToInt(character.combatComponent.unModifiedAttack * (PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE) / 100f));
                _addedAttackPercent = PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE);
                character.combatComponent.AdjustAttackPercentModifier(_addedAttackPercent);

                //character.buffStatsBonus.originalHP = Mathf.RoundToInt(character.combatComponent.unModifiedMaxHP * (PlayerSkillManager.Instance.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE) / 100f));
                _addedHPPercent = PlayerSkillManager.Instance.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE);
                character.combatComponent.AdjustMaxHPPercentModifier(_addedHPPercent);
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                if (character.marker) {
                    character.marker.BerserkedMarker();
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (character.marker) {
                    if (!character.traitContainer.HasTrait("Berserked")) {
                        character.marker.UnberserkedMarker();
                    }
                }
                character.combatComponent.AdjustAttackPercentModifier(-_addedAttackPercent);
                character.combatComponent.AdjustMaxHPPercentModifier(-_addedHPPercent);
                //character.buffStatsBonus.Reset();
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (character.marker) {
                    character.marker.BerserkedMarker();
                }
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (character.marker) {
                    if (!character.traitContainer.HasTrait("Berserked")) {
                        character.marker.UnberserkedMarker();
                    }
                }
            }
        }
        #endregion
    }
}