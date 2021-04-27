using System;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Empowered : Status {

        public int modification { get; private set; }
        
        public override Type serializedData => typeof(SaveDataEmpowered);
        public Empowered() {
            name = "Empowered";
            description = "Increased Strength.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
        }

        #region Overrides
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataEmpowered saveDataEmpowered = p_saveDataTrait as SaveDataEmpowered;
            modification = saveDataEmpowered.modification;
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character targetCharacter) {
                float modificationPercent = PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.EMPOWER)/100f;
                modification = (int)(modificationPercent * targetCharacter.combatComponent.attack);
                targetCharacter.combatComponent.AdjustAttackModifier(modification);
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character targetCharacter) {
                targetCharacter.combatComponent.AdjustAttackModifier(-modification);
            }
        }
        #endregion
    }
}

#region Save Data
public class SaveDataEmpowered : SaveDataTrait {
    public int modification;
    public override void Save(Trait trait) {
        base.Save(trait);
        Empowered data = trait as Empowered;
        Assert.IsNotNull(data);
        modification = data.modification;
    }
}
#endregion