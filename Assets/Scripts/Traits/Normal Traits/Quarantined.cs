using System.Collections.Generic;
using Characters.Components;
using Inner_Maps.Location_Structures;
namespace Traits {
    public class Quarantined : Status, CharacterEventDispatcher.ITraitListener, CharacterEventDispatcher.ICarryListener, CharacterEventDispatcher.ILocationListener {
        public override bool isSingleton => true;
        
        public Quarantined() {
            name = "Quarantined";
            description = "Not allowed to move around.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(96);
            hindersMovement = true;
            hindersPerform = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                character.eventDispatcher.SubscribeToCharacterLostTrait(this);
                character.eventDispatcher.SubscribeToCharacterCarried(this);
                character.eventDispatcher.SubscribeToCharacterLeftStructure(this);
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.eventDispatcher.SubscribeToCharacterLostTrait(this);
                character.eventDispatcher.SubscribeToCharacterCarried(this);
                character.eventDispatcher.SubscribeToCharacterLeftStructure(this);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
                character.eventDispatcher.UnsubscribeToCharacterCarried(this);
                character.eventDispatcher.UnsubscribeToCharacterLeftStructure(this);
                // character.tileObjectLocation?.RemoveUser(character);
            }
        }
        public override bool OnDeath(Character p_character) {
            p_character.traitContainer.RemoveTrait(p_character, this);
            return base.OnDeath(p_character);
        }
        public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
            if (p_lostTrait is Plagued) {
                //remove quarantined from character whenever it loses plagued
                p_character.traitContainer.RemoveTrait(p_character, this);
            }
        }
        public void OnCharacterCarried(Character p_character, Character p_carriedBy) {
            if (p_character.tileObjectLocation is BedClinic) {
                p_character.tileObjectLocation?.RemoveUser(p_character); //whenever this character has been carried, then remove it from it's current bed, if it is in one.    
            }
        }
        public void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure) {
            if (p_leftStructure != null && p_leftStructure.structureType == STRUCTURE_TYPE.APOTHECARY) {
                //if character left an apothecary then it is no longer quarantined
                p_character.traitContainer.RemoveTrait(p_character, this);
            }
        }
    }
}

