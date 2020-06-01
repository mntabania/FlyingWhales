using System.Collections.Generic;
using System.Linq;
using Locations.Features;
using Traits;
using UnityEngine;
using UtilityScripts;

public class WolfBehaviour : CharacterBehaviourComponent {
    
    public WolfBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (UtilityScripts.Utilities.IsEven(GameManager.Instance.Today().day) &&
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 && Random.Range(0, 2) == 1) {
            List<HexTile> choices = character.currentRegion.GetTilesWithFeature(TileFeatureDB.Game_Feature).OrderBy(x =>
                    Vector2.Distance(x.GetCenterLocationGridTile().centeredWorldLocation, character.worldPosition))
                .ToList();
            if (choices.Count > 0) {
                HexTile tileWithGameFeature = choices[0];
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
