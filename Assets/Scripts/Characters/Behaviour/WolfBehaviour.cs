using System.Collections.Generic;
using Locations.Features;
using Traits;
using UtilityScripts;

public class WolfBehaviour : CharacterBehaviourComponent {
    
    public WolfBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        //if night time, create a job to hunt at a given hextile
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick(); 
        if (timeInWords == TIME_IN_WORDS.MORNING) {
            List<HexTile> choices = character.currentRegion.GetTilesWithFeature(TileFeatureDB.Game_Feature);
            if (choices.Count > 0) {
                HexTile tileWithGameFeature = CollectionUtilities.GetRandomElement(choices);
                Hunting hunting = new Hunting();
                hunting.SetTargetTile(tileWithGameFeature);
                character.traitContainer.AddTrait(character, hunting);
                return true;
            } else {
                return false;
            }
        }
        return false;
    }
}
