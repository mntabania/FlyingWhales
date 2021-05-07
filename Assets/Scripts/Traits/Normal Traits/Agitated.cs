using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

namespace Traits {
    public class Agitated : Status {
        public override bool isSingleton => true;
        
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
                if (character.marker) {
                    character.marker.BerserkedMarker();
                }
                character.buffStatsBonus.originalAttack = Mathf.RoundToInt(character.combatComponent.unModifiedAttack * (PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE) / 100f));
                character.combatComponent.AdjustAttackModifier(character.buffStatsBonus.originalAttack);

                character.buffStatsBonus.originalHP = Mathf.RoundToInt(character.combatComponent.unModifiedMaxHP * (PlayerSkillManager.Instance.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE) / 100f));
                character.combatComponent.AdjustMaxHPModifier(character.buffStatsBonus.originalHP);
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
                character.combatComponent.AdjustMaxHPModifier(-character.buffStatsBonus.originalHP);
                character.combatComponent.AdjustAttackModifier(-character.buffStatsBonus.originalAttack);
                character.buffStatsBonus.Reset();
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