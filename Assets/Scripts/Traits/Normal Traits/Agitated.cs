using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Traits {
    public class Agitated : Status {

        private SkillData m_skillData;
        private PlayerSkillData m_playerSkillData;
        private float m_addedAtk = 0f;
        public override bool isSingleton => true;
        
        public Agitated() {
            name = "Agitated";
            description = "Will attack neaby village.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            m_skillData = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.AGITATE);
            m_playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.AGITATE);
            m_skillData.currentLevel = 1;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                if (character.marker) {
                    character.marker.BerserkedMarker();
                }
                m_addedAtk = m_playerSkillData.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(m_skillData.currentLevel);
                character.combatComponent.AddAttackBaseOnPercentage(m_addedAtk);

                float hpAmount = character.maxHP * m_playerSkillData.skillUpgradeData.GetAdditionalHpPercentagePerLevelBaseOnLevel(m_skillData.currentLevel);
                character.AdjustHP((int)hpAmount, ELEMENTAL_TYPE.Normal);
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
                character.combatComponent.AddAttackBaseOnPercentage(m_addedAtk);
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