using System;
namespace Traits {
    public class PlagueReservoir : Trait {
        
        public override bool isSingleton => true;
        
        public PlagueReservoir() {
            name = "Plague Reservoir";
            description = "Immune to a Plague's effect but can spread it.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo.traitContainer.HasTrait("Plagued")) {
                addedTo.traitContainer.RemoveTrait(addedTo, "Plagued");
            }
            addedTo.traitContainer.AddTrait(addedTo, "Plagued");
            if(addedTo is Character character) {
                character.needsComponent.AdjustFullnessDecreaseRate(-0.17f);
                character.needsComponent.AdjustHappinessDecreaseRate(-0.13f);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.needsComponent.AdjustFullnessDecreaseRate(0.17f);
                character.needsComponent.AdjustHappinessDecreaseRate(0.13f);
            }
        }
        #endregion
    }
}