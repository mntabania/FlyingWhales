using Characters.Components;
namespace Traits {
    public class Quarantined : Status, CharacterEventDispatcher.ITraitListener {
        public override bool isSingleton => true;
        
        public Quarantined() {
            name = "Quarantined";
            description = "Not allowed to move around.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(96);
            hindersMovement = true;
            hindersPerform = true;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                character.eventDispatcher.SubscribeToCharacterLostTrait(this);
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.eventDispatcher.SubscribeToCharacterLostTrait(this);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
            }
        }
        public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
            if (p_lostTrait is Plagued) {
                //remove quarantined from character whenever it loses plagued
                p_character.traitContainer.RemoveTrait(p_character, this);
            }
        }
    }
}

