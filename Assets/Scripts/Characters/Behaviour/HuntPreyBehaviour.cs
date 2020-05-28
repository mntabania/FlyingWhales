using System.Collections.Generic;
using System.Linq;
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
            List<Animal> animals = hunting.targetTile.GetAllDeadAndAliveCharactersInsideHex<Animal>();
            if (animals != null) {
                animals = animals.Where(x => x.race != character.race).ToList();
                if (animals.Count > 0) {
                    List<Animal> deadAnimals = animals.Where(x => x.isDead).ToList();
                    if (deadAnimals.Count > 0) {
                        Animal deadAnimal = CollectionUtilities.GetRandomElement(deadAnimals);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_PREY,
                            INTERACTION_TYPE.EAT_CORPSE, deadAnimal, character);
                        character.jobQueue.AddJobInQueue(job);
                    } else {
                        //only make wolf assault because job to hunt prey will be cancelled after the target animal dies.
                        //eat corpse will be triggered above.
                        Animal animalToHunt = CollectionUtilities.GetRandomElement(animals);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_PREY,
                            INTERACTION_TYPE.ASSAULT, animalToHunt, character);
                        character.jobQueue.AddJobInQueue(job);
                    }
                }
            } else {
                character.traitContainer.RemoveTrait(character, "Hunting");
            }
        }
        return true;
    }
}
