using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Traits {
    public class Agitated : Status {

        private float m_addedAtk = 0f;
        private float m_addedMaxHP = 0f;
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
                m_addedAtk = PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE);
                character.combatComponent.AddAttackBaseOnPercentage(m_addedAtk);

                float m_addedMaxHP = character.maxHP * (PlayerSkillManager.Instance.GetAdditionalHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE) / 100);
                character.combatComponent.AdjustMaxHPModifier((int)m_addedMaxHP);
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
                character.combatComponent.AdjustMaxHPModifier((int)m_addedMaxHP * -1);
                character.combatComponent.SubtractAttackBaseOnPercentage(m_addedAtk);
                m_addedAtk = 0f;
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