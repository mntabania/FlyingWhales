using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Traits {
    /// <summary>
    /// Class used to validate traits for all traitables
    /// </summary>
    public static class TraitValidator {

        public static bool CanAddTrait(ITraitable obj, Trait trait, ITraitContainer traitContainer) {
            //Cannot add trait if there is an existing trait that is mutually exclusive of the trait to be added
            if (trait.mutuallyExclusive != null) {
                for (int i = 0; i < trait.mutuallyExclusive.Length; i++) {
                    if (obj.traitContainer.HasTrait(trait.mutuallyExclusive[i])) {
                        return false;
                    }
                }
            }

            bool checkUniqueness = true;
            if(trait is Status status) {
                checkUniqueness = !status.isStacking;
            }

            //Cannot add trait if it is unique and the character already has that type of trait.
            if (checkUniqueness) {
                if (trait.IsUnique()) {
                    if (obj.traitContainer.HasTrait(trait.name)) {
                        return false;
                    }
                }
            }

            //if (trait.isStacking) {
            //    if (traitContainer.stacks.ContainsKey(trait)) {
            //        if (traitContainer.stacks[trait] >= trait.stackLimit) {
            //            return false;
            //        }
            //    }
            //} else {
            //    //Cannot add trait if it is unique and the character already has that type of trait.
            //    if (trait.IsUnique()) {
            //        Trait oldTrait = obj.traitContainer.GetNormalTrait<Trait>(trait.name);
            //        if (oldTrait != null) {
            //            return false;
            //        }
            //    }
            //}
            return true;
        }
        public static bool CanAddTrait(ITraitable obj, string traitName, ITraitContainer traitContainer) {
            Trait trait;
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                trait = TraitManager.Instance.CreateNewInstancedTraitClass(traitName);
            } else {
                Assert.IsTrue(TraitManager.Instance.allTraits.ContainsKey(traitName),
                    $"No key for trait {traitName}");
                trait = TraitManager.Instance.allTraits[traitName];
            }
            return CanAddTrait(obj, trait, traitContainer);
        }
    }
}

