using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class MimicBehaviour : BaseMonsterBehaviour {
    private readonly WeightedDictionary<string> _actionWeights;
    
    public MimicBehaviour() {
        priority = 9;
        _actionWeights = new WeightedDictionary<string>();
        _actionWeights.AddElement("Roam", 50); //50
        _actionWeights.AddElement("Stand", 20); //20
        _actionWeights.AddElement("Revert", 5); //5
    }
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n-{character.name} is Mimic";
#endif
        string chosenAction = _actionWeights.PickRandomElementGivenWeights();
        if (character.currentStructure is Kennel && chosenAction == "Revert") {
            chosenAction = "Stand";
        }

        // chosenAction = "Revert";
        
        if (chosenAction == "Roam") {
            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
        } else if (chosenAction == "Stand") {
            return character.jobComponent.TriggerStandStill(out producedJob);
        } else {
            producedJob = null;
            Mimic mimic = character as Mimic;
            mimic.SetIsTreasureChest(true);
            LocationGridTile tile = character.gridTileLocation;
            // character.DisableMarker();
            character.marker.SetVisualState(false);
            TreasureChest chest = InnerMapManager.Instance.CreateNewTileObject<TreasureChest>(TILE_OBJECT_TYPE.TREASURE_CHEST); 
            tile.structure.AddPOI(chest, tile);
            chest.SetObjectInside(mimic);
            return true;
        }
    }
}