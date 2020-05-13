using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Inner_Maps.Location_Structures {
    public class TortureChamberStructureObject : LocationStructureObject {
        [Header("Torture Chamber")]
        [SerializeField] private Vector3Int entranceCoordinates;
        
        public LocationGridTile entrance { get; private set; }
        public void SetEntrance(InnerTileMap innerTileMap) {
            entrance = ConvertLocalPointInStructureToTile(entranceCoordinates, innerTileMap);
        }
    }
}