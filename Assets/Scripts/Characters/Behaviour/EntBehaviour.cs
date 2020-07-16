using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class EntBehaviour : CharacterBehaviourComponent {
    private WeightedDictionary<string> _actionWeights;
    
	public EntBehaviour() {
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
            Ent ent = character as Ent;
            ent.SetIsTree(true);
            LocationGridTile tile = character.gridTileLocation;
            character.DisableMarker();
            TreeObject treeObject =
                InnerMapManager.Instance.CreateNewTileObject<TreeObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT); 
            tile.structure.AddPOI(treeObject, tile);
            treeObject.SetOccupyingEnt(ent);
            return true;
        }
        producedJob = null;
        return false;
    }
}
