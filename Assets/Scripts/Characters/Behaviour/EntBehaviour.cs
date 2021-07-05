using Inner_Maps;
using Inner_Maps.Location_Structures;

public class EntBehaviour : BaseMonsterBehaviour {
    private readonly WeightedDictionary<string> _actionWeights;
    
	public EntBehaviour() {
		priority = 9;
		_actionWeights = new WeightedDictionary<string>();
        _actionWeights.AddElement("Roam", 50); //50
        _actionWeights.AddElement("Stand", 20); //20
        _actionWeights.AddElement("Revert", 5); //5
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n-{character.name} is Ent";
#endif
        string chosenAction = _actionWeights.PickRandomElementGivenWeights();
        if (character.currentStructure is Kennel && chosenAction == "Revert") {
            chosenAction = "Stand";
        }

        if (chosenAction == "Roam") {
            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
        } else if (chosenAction == "Stand") {
            return character.jobComponent.TriggerStandStill(out producedJob);
        } else {
            producedJob = null;
            Ent ent = character as Ent;
            ent.SetIsTree(true);
            LocationGridTile tile = character.gridTileLocation;
            character.marker.SetVisualState(false);
            TreeObject treeObject = InnerMapManager.Instance.CreateNewTileObject<TreeObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT); 
            tile.structure.AddPOI(treeObject, tile);
            treeObject.SetOccupyingEnt(ent);
            return true;
        }
    }
}
