using UnityEngine.Assertions;
namespace Inner_Maps {
    public class InnerMapHexTile {
        
        public LocationGridTileCollection[] gridTileCollections { get; private set; }
        public HexTile hexTileOwner { get; }
        /// <summary>
        /// Are there any structures occupying this hextile.
        /// </summary>
        public bool isOccupied { get; private set; }

        public InnerMapHexTile(HexTile _hexTileOwner) {
            hexTileOwner = _hexTileOwner;
        }
        
        public void SetGridTileCollections(LocationGridTileCollection[] _gridTileCollections) {
            Assert.IsTrue(_gridTileCollections.Length == 4, $"Set gridTileCollections of inner map hextile is not 4!");
            gridTileCollections = _gridTileCollections;
        }

        public void Occupy() {
            isOccupied = true;
        }
        public void Vacate() {
            isOccupied = false;
        }
        public void CheckIfVacated() {
            bool isVacated = true;
            for (int i = 0; i < gridTileCollections.Length; i++) {
                LocationGridTileCollection collection = gridTileCollections[i];
                for (int j = 0; j < collection.tilesInTerritory.Length; j++) {
                    LocationGridTile tile = collection.tilesInTerritory[j];
                    //if there is a tile that belongs to a structure that is not wilderness, and it occupies this,
                    //then this is not yet vacated.
                    if (tile.structure.structureType != STRUCTURE_TYPE.WILDERNESS 
                        && tile.structure.occupiedHexTile == this) {
                        isVacated = false;
                        break;
                    }
                }
            }
            if (isVacated) {
                Vacate();
            }
        }
    }
}