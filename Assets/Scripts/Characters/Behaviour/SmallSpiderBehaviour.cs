using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using Random = UnityEngine.Random;

public class SmallSpiderBehaviour : CharacterBehaviourComponent {

    public SmallSpiderBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
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

    private List<Character> GetWebbedCharactersAtHome(Character character) {
        if (character.homeStructure != null) {
            return character.homeStructure.GetCharactersThatMeetCriteria(c => c.traitContainer.HasTrait("Webbed"));
        } else if (character.territorries != null && character.territorries.Count > 0) {
            List<Character> characters = null;
            for (int i = 0; i < character.territorries.Count; i++) {
                HexTile territory = character.territorries[i];
                List<Character> validCharacters =
                    territory.GetAllCharactersInsideHexThatMeetCriteria(c => c.traitContainer.HasTrait("Webbed"));
                if (validCharacters != null) {
                    if (characters == null) {
                        characters = new List<Character>();
                    }
                    characters.AddRange(validCharacters);
                }
            }
            return characters;
        }
        return null;
    }
}
