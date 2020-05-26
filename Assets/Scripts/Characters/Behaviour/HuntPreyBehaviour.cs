using System.Collections.Generic;
using Locations.Features;
using Traits;
using UtilityScripts;

public class HuntPreyBehaviour : CharacterBehaviourComponent {

    public HuntPreyBehaviour() {
        priority = 10;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        Hunting hunting = character.traitContainer.GetNormalTrait<Hunting>("Hunting");
        if (hunting != null) {
            List<Animal> animals = hunting.targetTile.featureComponent.GetFeature<GameFeature>().ownedAnimals;
            if (animals != null && animals.Count > 0) {
                Animal animalToHunt = CollectionUtilities.GetRandomElement(animals);
                character.combatComponent.Fight(animalToHunt, "Hunting");
            } else {
                character.traitContainer.RemoveTrait(character, "Hunting");
            }
        }
        return true;
    }
}
