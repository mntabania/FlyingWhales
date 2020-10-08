using Inner_Maps;
using Pathfinding;
using Pathfinding.Serialization;
using UnityEngine;
namespace PathFinding {
	// Inherit our new graph from the base graph type
	[JsonOptIn]
	// Make sure the class is not stripped out when using code stripping (see https://docs.unity3d.com/Manual/ManagedCodeStripping.html)
	[Pathfinding.Util.Preserve]
    public class RuinarchGridGraph : GridGraph {
	    
		public override void RecalculateCell (int x, int z, bool resetPenalties = true, bool resetTags = true) {
	        base.RecalculateCell(x, z, resetPenalties, resetTags);
			var node = nodes[z*width + x];
			node.Walkable = true;
			if (collision.Check((Vector3)node.position)) {
				bool isPartOfSettlement = false;
				uint settlementTagToUse = 0;
				for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
					InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
					Vector3 pos = (Vector3)node.position;
					LocationGridTile tile = map.GetTileFromWorldPos(pos); 
					if (tile != null) {
						if (tile.collectionOwner.isPartOfParentRegionMap && tile.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null && 
						    tile.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.locationType != LOCATION_TYPE.DUNGEON && 
						    tile.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.owner != null) {
							isPartOfSettlement = true;
							settlementTagToUse = tile.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.owner.pathfindingTag;
						}
						break;
					}
				}
				if (isPartOfSettlement) {
					node.Tag = settlementTagToUse;
				} else {
					node.Tag = 0;    
				}
			} else {
				//obstacle
				node.Tag = 1;
			}
		}
    }
}