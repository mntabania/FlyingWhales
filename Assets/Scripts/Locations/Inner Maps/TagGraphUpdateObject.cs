using Pathfinding;
using UnityEngine;
namespace Inner_Maps {
    /// <summary>
    /// Graph update object that will set all unwalkable nodes as part of the obstacle tag instead.
    /// </summary>
    public class TagGraphUpdateObject : GraphUpdateObject {

        public TagGraphUpdateObject(Bounds bounds) : base (bounds) { }
        
        public override void Apply(GraphNode node) {
            base.Apply(node);
            if (!node.Walkable) {
                node.Tag = 1;
                node.Walkable = true;
            } else {
                bool isPartOfSettlement = false;
                uint settlementTagToUse = 0;
                for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
                    InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
                    Vector3 pos = (Vector3)node.position;
                    LocationGridTile tile = map.GetTileFromWorldPos(pos); 
                    if (tile != null) {
                        if (tile.area.settlementOnArea != null && 
                            tile.area.settlementOnArea.locationType != LOCATION_TYPE.DUNGEON && 
                            tile.area.settlementOnArea.owner != null) {
                            isPartOfSettlement = true;
                            settlementTagToUse = tile.area.settlementOnArea.owner.pathfindingTag;
                        }
                        break;
                    }
                }
                if (isPartOfSettlement) {
                    node.Tag = settlementTagToUse;
                } else {
                    node.Tag = 0;    
                }
            }
        }
    }
}