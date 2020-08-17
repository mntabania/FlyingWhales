using System.Collections.Generic;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class ZooCell : StructureRoom {
        public ZooCell(List<LocationGridTile> tilesInRoom) : base("Zoo Cell", tilesInRoom) {
            selectableSize = new Vector2(selectableSize.x + 0.5f, selectableSize.y + 0.5f);
        }
    }
}