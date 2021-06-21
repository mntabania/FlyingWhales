using System.Collections.Generic;
namespace Traits {
    public class AbominationGerm : Status {

        private IPointOfInterest _owner;
        
        public AbominationGerm() {
            name = "Abomination Germ";
            description = "There is a mysterious germ growing inside...";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
            moodEffect = -5;
            isStacking = true;
            stackLimit = 1;
            stackModifier = 1f;
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_After_Effect_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is IPointOfInterest pointOfInterest) {
                _owner = pointOfInterest;
                if (pointOfInterest is TileObject) {
                    ticksDuration = 0; //lasts indefinitely on objects.
                }
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is IPointOfInterest pointOfInterest) {
                _owner = pointOfInterest;
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            _owner = null;
        }
        public override void OnRemoveStatusBySchedule(ITraitable removedFrom) {
            base.OnRemoveStatusBySchedule(removedFrom);
            if (removedFrom is Character character && character.isNormalCharacter) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Abomination_Death, character);
            }
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, actor, target, category, ref isRemoved);
            if (category == ACTION_CATEGORY.CONSUME && target == _owner) {
                actor.traitContainer.AddTrait(actor, "Abomination Germ");
                target.traitContainer.RemoveTrait(target, "Abomination Germ");
            }
        }
        #endregion
    }
}