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
            List<Character> webbedCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
            PopulateWebbedCharactersAtHome(webbedCharacters, character);
            if (webbedCharacters.Count > 0) {
                Character webbedCharacter = CollectionUtilities.GetRandomElement(webbedCharacters);
                ObjectPoolManager.Instance.ReturnCharactersListToPool(webbedCharacters);
                return character.jobComponent.TriggerEatAlive(webbedCharacter, out producedJob);
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(webbedCharacters);
        }
        return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, true);
    }
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
    }
    private void PopulateWebbedCharactersAtHome(List<Character> p_characterList, Character character) {
        if (character.homeStructure != null) {
            character.homeStructure.PopulateCharacterListThatIsWebbed(p_characterList);
        } else if (character.HasTerritory()) {
            character.territory.locationCharacterTracker.PopulateCharacterListInsideHexThatHasTrait(p_characterList, "Webbed");
        }
    }
}
