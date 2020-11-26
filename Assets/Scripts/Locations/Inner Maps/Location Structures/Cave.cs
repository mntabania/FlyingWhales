using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Cave : NaturalStructure {

        public const string Yield_Nothing = "Nothing";
        public const string Yield_Metal = "Metal";
        public const string Yield_Diamond = "Diamond";
        public const string Yield_Gold = "Gold";
        public const string Yield_Stone = "Stone";

        public WeightedDictionary<string> resourceYield { get; }
        /// <summary>
        /// Separate field for the occupied hex tiles of this cave.
        /// Since caves can occupy multiple hex tiles.
        /// </summary>
        public List<InnerMapHexTile> caveHexTiles { get; }
        public override InnerMapHexTile occupiedHexTile => caveHexTiles.Count > 0 ? caveHexTiles[0] : null;

        #region getters
        public override System.Type serializedData => typeof(SaveDataCave);
        #endregion

        public Cave(Region location) : base(STRUCTURE_TYPE.CAVE, location) {
            resourceYield = GetRandomResourceYield();
            caveHexTiles = new List<InnerMapHexTile>();
        }

        public Cave(Region location, SaveDataCave data) : base(location, data) {
            resourceYield = GetRandomResourceYield();
            caveHexTiles = new List<InnerMapHexTile>();
        }

        #region Loading
        public void LoadOccupiedHexTiles(SaveDataCave saveDataCave) {
            for (int i = 0; i < saveDataCave.occupiedHextiles.Count; i++) {
                string hexTileID = saveDataCave.occupiedHextiles[i];
                if (!string.IsNullOrEmpty(hexTileID)) {
                    HexTile hexTile = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(hexTileID);
                    caveHexTiles.Add(hexTile.innerMapHexTile);
                }
            }
        }
        #endregion

        private WeightedDictionary<string> GetRandomResourceYield() {
            WeightedDictionary<string> randomYield = new WeightedDictionary<string>();
            
            WeightedDictionary<int> chances = new WeightedDictionary<int>();
            chances.AddElement(0, 20);
            chances.AddElement(1, 20);
            chances.AddElement(2, 20);
            chances.AddElement(3, 20);
            chances.AddElement(4, 20);
            int chosen = chances.PickRandomElementGivenWeights();
            if (chosen == 0) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Stone, 50);
                randomYield.AddElement(Yield_Metal, 20);
                randomYield.AddElement(Yield_Diamond, 2);
                randomYield.AddElement(Yield_Gold, 0);
            } else if (chosen == 1) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Stone, 50);
                randomYield.AddElement(Yield_Metal, 0);
                randomYield.AddElement(Yield_Diamond, 0);
                randomYield.AddElement(Yield_Gold, 10);
            } else if (chosen == 2) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Stone, 50);
                randomYield.AddElement(Yield_Metal, 0);
                randomYield.AddElement(Yield_Diamond, 10);
                randomYield.AddElement(Yield_Gold, 0);
            } else if (chosen == 3) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Stone, 50);
                randomYield.AddElement(Yield_Metal, 30);
                randomYield.AddElement(Yield_Diamond, 0);
                randomYield.AddElement(Yield_Gold, 0);
            } else if (chosen == 4) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Stone, 50);
                randomYield.AddElement(Yield_Metal, 20);
                randomYield.AddElement(Yield_Diamond, 0);
                randomYield.AddElement(Yield_Gold, 2);
            }
            return randomYield;
        }
        protected override void OnTileAddedToStructure(LocationGridTile tile) {
            base.OnTileAddedToStructure(tile);
            tile.genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.MINE);
            if (tile.collectionOwner.isPartOfParentRegionMap && 
                caveHexTiles.Contains(tile.collectionOwner.partOfHextile) == false) {
                caveHexTiles.Add(tile.collectionOwner.partOfHextile);
            }
        }
        protected override void OnTileRemovedFromStructure(LocationGridTile tile) {
            base.OnTileRemovedFromStructure(tile);
            tile.genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.MINE);
        }
        public override void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != region.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (region.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(region);
            }
            if (occupiedHexTile != null) {
                float centerX = 0f;
                float centerY = 0f;
                for (int i = 0; i < occupiedHexTiles.Count; i++) {
                    HexTile hexTile = occupiedHexTiles[i];
                    Vector2 worldLocation = hexTile.GetCenterLocationGridTile().centeredWorldLocation;
                    centerX += worldLocation.x;
                    centerY += worldLocation.y;
                }
                Vector2 finalPos = new Vector2(centerX / occupiedHexTiles.Count, centerY / occupiedHexTiles.Count);
                InnerMapCameraMove.Instance.CenterCameraOn(finalPos);
            }
        }
        public override void ShowSelectorOnStructure() {
            if (occupiedHexTile != null) {
                Selector.Instance.Select(occupiedHexTile.hexTileOwner);
            }
        }
        public override bool HasTileOnHexTile(HexTile hexTile) {
            return occupiedHexTile != null && caveHexTiles.Contains(hexTile.innerMapHexTile);
        }
    }
}

#region Save Data
public class SaveDataCave : SaveDataNaturalStructure {
    public List<string> occupiedHextiles;
    public override void Save(LocationStructure structure) {
        base.Save(structure);
        Cave cave = structure as Cave;
        occupiedHextiles = new List<string>();
        for (int i = 0; i < cave.caveHexTiles.Count; i++) {
            InnerMapHexTile innerMapHexTile = cave.caveHexTiles[i];
            occupiedHextiles.Add(innerMapHexTile.hexTileOwner.persistentID);
        }
    }
}
#endregion