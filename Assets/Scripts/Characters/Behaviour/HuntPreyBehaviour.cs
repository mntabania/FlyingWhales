using System.Collections.Generic;
using System.Linq;
using Traits;
using UtilityScripts;

public class HuntPreyBehaviour : CharacterBehaviourComponent {

    public HuntPreyBehaviour() {
        priority = 10;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        Hunting hunting = character.traitContainer.GetTraitOrStatus<Hunting>("Hunting");
        if (hunting != null) {
            List<Character> animals = RuinarchListPool<Character>.Claim();
            hunting.targetArea.locationCharacterTracker.PopulateAnimalsListThatCharacterCanReachInsideHexThatIsNotTheSameRaceAs(character, animals, character.race);
            if (animals.Count > 0) {
                List<Character> deadAnimals = RuinarchListPool<Character>.Claim();
                for (int i = 0; i < animals.Count; i++) {
                    Character c = animals[i];
                    if (c.isDead && c.hasMarker && c.gridTileLocation != null) {
                        deadAnimals.Add(c);
                    }
                }
                if (deadAnimals.Count > 0) {
                    Character deadAnimal = CollectionUtilities.GetRandomElement(deadAnimals);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_PREY,
                        INTERACTION_TYPE.EAT_CORPSE, deadAnimal, character);
                    // job.SetCancelOnDeath(false);
                    producedJob = job;
                } else {
                    //only make wolf assault because job to hunt prey will be cancelled after the target animal dies.
                    //eat corpse will be triggered above.
                    Character animalToHunt = CollectionUtilities.GetRandomElement(animals);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_PREY,
                        INTERACTION_TYPE.ASSAULT, animalToHunt, character);
                    producedJob = job;
                }
                RuinarchListPool<Character>.Release(deadAnimals);
            } else {
                character.traitContainer.RemoveTrait(character, "Hunting");
            }
            RuinarchListPool<Character>.Release(animals);
        }
        return true;
    }
}
