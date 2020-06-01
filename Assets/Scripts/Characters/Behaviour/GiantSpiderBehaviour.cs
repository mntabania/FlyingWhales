using System;
using System.Collections.Generic;
using System.Linq;
using UtilityScripts;

public class GiantSpiderBehaviour : CharacterBehaviourComponent {

    public GiantSpiderBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            if (character.homeStructure != null && UnityEngine.Random.Range(0, 100) < 20) {
                List<Character> characterChoices = character.currentRegion.charactersAtLocation
                    .Where(c => c.isNormalCharacter && c.canMove).ToList();
                if (characterChoices.Count > 0) {
                    Character chosenCharacter = CollectionUtilities.GetRandomElement(characterChoices);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT,
                        INTERACTION_TYPE.DROP, chosenCharacter, character);
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {character.homeStructure});
                    character.jobQueue.AddJobInQueue(job);
                    return true;
                }
            }
        }
        return false;
    }
}
