namespace Traits {
    public class Tended : Status {
        public override bool isSingleton => true;

        public Tended() {
            name = "Tended";
            description = "This is Tended.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Crops crops) {
                crops.SetGrowthRate(2);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Crops crops) {
                crops.SetGrowthRate(1);
            }
        }
        #endregion
    }
}