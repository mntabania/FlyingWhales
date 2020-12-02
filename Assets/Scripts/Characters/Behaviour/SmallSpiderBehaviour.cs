using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using Random = UnityEngine.Random;

public class SmallSpiderBehaviour : BaseMonsterBehaviour {

    public SmallSpiderBehaviour() {
        priority = 9;
    }
    
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (GameUtilities.RollChance(30)) {
            //Try and eat a webbed character at this spiders home cave
            List<Character> webbedCharacters = GetWebbedCharactersAtHome(character);
            if (webbedCharacters != null && webbedCharacters.Count > 0) {
                Character webbedCharacter = CollectionUtilities.GetRandomElement(webbedCharacters);
                return character.jobComponent.TriggerEatAlive(webbedCharacter, out producedJob);
            }    
        }
        return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, true);
    }
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
    }
    private List<Character> GetWebbedCharactersAtHome(Character character) {
        if (character.homeStructure != null) {
            return character.homeStructure.GetCharactersThatMeetCriteria(c => c.traitContainer.HasTrait("Webbed"));
        } else if (character.HasTerritory()) {
            List<Character> characters = character.territory.GetAllCharactersInsideHexThatMeetCriteria<Character>(c => c.traitContainer.HasTrait("Webbed"));
            return characters;
        }
        return null;
    }
}
