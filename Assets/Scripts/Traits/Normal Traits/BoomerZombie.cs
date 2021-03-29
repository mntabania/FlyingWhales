using UnityEngine.Assertions;
using UnityEngine;
using System;
using Traits;
using UtilityScripts;
using Random = UnityEngine.Random;

namespace Traits {
    public class BoomerZombie : FiniteZombie {

        public BoomerZombie() {
            name = "Boomer Zombie";
            description = "Boomer Zombie";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }

        #region Override
        //public override void OnAddTrait(ITraitable addedTo) {
        //    base.OnAddTrait(addedTo);
        //    for (int i = 0; i < 5; i++) {
        //        addedTo.traitContainer.AddTrait(addedTo, "Poisoned", bypassElementalChance: true, overrideDuration: 0);
        //    }
        //}
        public override bool OnDeath(Character character) {
            if (character.traitContainer.HasTrait("Poisoned")) {
                Poisoned poisoned = character.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
                int poisonStacks = character.traitContainer.GetStacks("Poisoned");
                character.traitContainer.RemoveStatusAndStacks(character, "Poisoned");
                if (character.gridTileLocation != null) {
                    CombatManager.Instance.PoisonExplosion(character, character.gridTileLocation, poisonStacks, character, 2, poisoned.isPlayerSource);
                }
                character.SetDestroyMarkerOnDeath(true);
            }
            return base.OnDeath(character);
        }
        #endregion
    }
}