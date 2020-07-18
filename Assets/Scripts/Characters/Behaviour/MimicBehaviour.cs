using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class MimicBehaviour : CharacterBehaviourComponent {
    private readonly WeightedDictionary<string> _actionWeights;
    
    public MimicBehaviour() {
        priority = 9;
        _actionWeights = new WeightedDictionary<string>();
        _actionWeights.AddElement("Roam", 50);
        _actionWeights.AddElement("Stand", 20);
        _actionWeights.AddElement("Revert", 5);
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is Ent";

        string chosenAction = _actionWeights.PickRandomElementGivenWeights();
        if (chosenAction == "Roam") {
            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
        } else if (chosenAction == "Stand") {
            return character.jobComponent.TriggerStandStill(out producedJob);
        } else {
            producedJob = null;
            Mimic mimic = character as Mimic;
            mimic.SetIsTreasureChest(true);
            LocationGridTile tile = character.gridTileLocation;
            character.DisableMarker();
            TreasureChest chest =
                InnerMapManager.Instance.CreateNewTileObject<TreasureChest>(TILE_OBJECT_TYPE.TREASURE_CHEST); 
            tile.structure.AddPOI(chest, tile);
            chest.SetObjectInside(mimic);
            return true;
        }
        producedJob = null;
        return false;
    }
}