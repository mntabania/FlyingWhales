namespace Traits {
    public class AbominationGerm : Status {

        private GameDate expectedExpiryDate;
        
        public AbominationGerm() {
            name = "Abomination Germ";
            description = "This is infected with a virus!";
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
            expectedExpiryDate = GameManager.Instance.Today().AddTicks(ticksDuration);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            //to check if trait was removed via expiry, check that this trait was not removed by someone,
            //and the date today is the same as the expected expiry date.
            if (removedBy == null && GameManager.Instance.Today().IsSameDate(expectedExpiryDate) 
                && removedFrom is Character character) {
                //if trait expired normally trigger abomination death interrupt
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Abomination_Death, character);
            }
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Abomination Germ", gainedFromDoing: goapNode);
            }
        }
        #endregion
    }
}