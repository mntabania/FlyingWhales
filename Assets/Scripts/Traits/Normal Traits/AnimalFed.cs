namespace Traits {
    public class AnimalFed : Status {
        
        public AnimalFed() {
            name = "Animal Fed";
            description = "Recently ate: Animal Meat. Increases piercing by 50%";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            isStacking = true;
            stackLimit = 1;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
        }
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character targetCharacter) {
                //increase piercing by 50%
                targetCharacter.piercingAndResistancesComponent.AdjustPiercingMultiplier(50f);
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character targetCharacter) {
                targetCharacter.piercingAndResistancesComponent.AdjustPiercingMultiplier(-50f);
            }
        }
        #endregion
    }
}