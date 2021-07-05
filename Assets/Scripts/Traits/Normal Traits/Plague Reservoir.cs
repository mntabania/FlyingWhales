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
                //Made it so that characters with plague reservoir will NOT get hungry/sad as much as normal villagers
                //since we expect that Plague Reservoir characters do not usually have many sources of food.
                //Related cards:
                // - https://trello.com/c/qDZICPc0/2956-ratmen-have-different-happiness-and-fullness-reduction-per-tick 
                // - https://trello.com/c/nBDrc2vM/4889-ratmen-healing-per-tick
                float fullnessDecreaseRateEffect = EditableValuesManager.Instance.baseFullnessDecreaseRate / 2f;
                float happinessDecreaseRateEffect = EditableValuesManager.Instance.baseHappinessDecreaseRate / 2f;
                character.needsComponent.AdjustFullnessDecreaseRate(-fullnessDecreaseRateEffect);
                character.needsComponent.AdjustHappinessDecreaseRate(-happinessDecreaseRateEffect);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                float fullnessDecreaseRateEffect = EditableValuesManager.Instance.baseFullnessDecreaseRate / 2f;
                float happinessDecreaseRateEffect = EditableValuesManager.Instance.baseHappinessDecreaseRate / 2f;
                character.needsComponent.AdjustFullnessDecreaseRate(fullnessDecreaseRateEffect);
                character.needsComponent.AdjustHappinessDecreaseRate(happinessDecreaseRateEffect);
            }
        }
        #endregion
    }
}