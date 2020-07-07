
using UtilityScripts;

public class CultistBehaviour : CharacterBehaviourComponent {

    public CultistBehaviour() {
        priority = 18;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        int chance = 0;
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            chance = 20;
        } else if (timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            chance = 35;
        }

        int roll = UnityEngine.Random.Range(0, 100);
        log += $"\nWill try to do cultist action. Chance is {chance.ToString()}. Roll is {roll.ToString()}";
        
        if (roll < chance) {
            return TryCreateCultistJob(character, ref log, out producedJob);
        }
        producedJob = null;
        return false;
    }

    public bool TryCreateCultistJob(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT) == false 
            && character.homeStructure?.GetTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT) == null) {
            log += $"\n{character.name} has no cultist kit available. Will create obtain personal item job.";
            bool success = character.jobComponent.TryCreateObtainPersonalItemJob("Cultist Kit", out producedJob);
            if (success) {
                producedJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).constructionCost });
            }
            return success;
        } else {
            if (GameUtilities.RollChance(30) && character.jobComponent.TryGetValidSabotageNeighbourTarget(out character)) {
                log += $"\n{character.name} has cultist kit available. Will create sabotage neighbour job.";
                return character.jobComponent.TryCreateSabotageNeighbourJob(character, out producedJob);    
            } else {
                return character.jobComponent.TryCreateDarkRitualJob(out producedJob);
            }
        }
    }
}
