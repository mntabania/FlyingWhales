using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class BigTreeObject : TreeObject {
	public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);
	public BigTreeObject() {
		//advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(TILE_OBJECT_TYPE.BIG_TREE_OBJECT, false);
        AddAdvertisedAction(INTERACTION_TYPE.CHOP_WOOD);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        SetYield(InnerMapManager.Big_Tree_Yield);
		traitContainer.AddTrait(this, "Immovable");
	}
	public BigTreeObject(SaveDataTileObject data) {
		//advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.CHOP_WOOD);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.AddTrait(this, "Immovable");
	}

	public override string ToString() {
		return $"Big Tree {id.ToString()}";
	}
	protected override string GenerateName() { return "Big Tree"; }

	public static bool CanBePlacedOnTile(LocationGridTile tile) {
		if (tile.isOccupied) {
			return false;
		}
		if (tile.groundType == LocationGridTile.Ground_Type.Bone) {
			return false;
		}
		if (tile.structure != null && tile.structure.structureType.IsOpenSpace() == false) {
			return false;
		}
		if (tile.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)) {
			return false;
		}
		List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(new Point(2, 2), tile);
		int invalidOverlap = overlappedTiles.Count(t => t.hasDetail || t.objHere != null || t.tileType == LocationGridTile.Tile_Type.Wall); 
		//|| t.partOfCollection.canBeBuiltOnByNPC == false

		return invalidOverlap <= 0;
	}
}

