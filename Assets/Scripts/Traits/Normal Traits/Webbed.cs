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
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character && character.marker) {
                //add webbed visual to character
                character.marker.ShowAdditionalEffect(CharacterManager.Instance.webbedEffect);
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                if (character.hasMarker) {
                    //add webbed visual to character
                    character.marker.ShowAdditionalEffect(CharacterManager.Instance.webbedEffect);    
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (character.hasMarker) {
                    //removed webbed visual from character
                    character.marker.HideAdditionalEffect();    
                }
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (character.hasMarker) {
                    character.marker.ShowAdditionalEffect(CharacterManager.Instance.webbedEffect);    
                }
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (character.hasMarker) {
                    character.marker.HideAdditionalEffect();    
                }
            }
        }
    }
}