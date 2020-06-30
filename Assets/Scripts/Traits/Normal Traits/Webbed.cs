namespace Traits {
    public class Webbed : Status {
        public override bool isSingleton => true;

        public Webbed() {
            name = "Webbed";
            description = "This is Webbed.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            isHidden = true;
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                //add webbed visual to character
                character.marker.ShowAdditionalEffect(CharacterManager.Instance.webbedEffect);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                //removed webbed visual from character
                character.marker.HideAdditionalEffect();
            }
        }
    }
}