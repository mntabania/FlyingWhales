using System;
namespace Traits {
    public class Petrasol : Trait {
        public override bool isSingleton => true;

        public Petrasol() {
            name = "Petrasol";
            description = "Turns to stone when hit by sunlight.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        //public override void OnAddTrait(ITraitable addedTo) {
        //    base.OnAddTrait(addedTo);
        //    if (addedTo is Character character) {
        //        character.behaviourComponent.AddBehaviourComponent(typeof(SubterraneanBehaviour));
        //    }
        //}
        //public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        //    base.OnRemoveTrait(removedFrom, removedBy);
        //    if (removedFrom is Character character) {
        //        character.behaviourComponent.RemoveBehaviourComponent(typeof(SubterraneanBehaviour));
        //    }
        //}
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            if(traitable is Character character) {
                CheckBecomeStone(character);
            }
        }
        #endregion

        private void CheckBecomeStone(Character character) {
            if (!character.currentStructure.isInterior) {
                TIME_IN_WORDS timeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick(null);
                if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT || timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                    character.traitContainer.RemoveTrait(character, "Stoned");
                } else {
                    if (!character.traitContainer.HasTrait("Stoned")) {
                        character.traitContainer.AddTrait(character, "Stoned");
                    }
                }
            }
        }
    }
}