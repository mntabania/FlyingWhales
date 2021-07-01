namespace Traits {
    public class FishFed : Status {
        
        public FishFed() {
            name = "Fish Fed";
            description = "Recently ate: Fish. Doubles all secondary resistances";
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
                //Poison, Electric, Ice
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Poison, 2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Electric, 2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Ice, 2);
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character targetCharacter) {
                //Poison, Electric, Ice
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Poison, -2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Electric, -2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Ice, -2);
            }
        }
        #endregion
    }
}