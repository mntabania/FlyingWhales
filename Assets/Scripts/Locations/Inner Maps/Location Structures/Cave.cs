using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Cave : NaturalStructure {

        public const string Yield_Nothing = "Nothing";
        public const string Yield_Metal = "Metal";
        public const string Yield_Diamond = "Diamond";
        public const string Yield_Gold = "Gold";

        public WeightedDictionary<string> resourceYield { get; }
        /// <summary>
        /// Separate field for the occupied hex tiles of this cave.
        /// Since caves can occupy multiple hex tiles.
        /// </summary>
        public List<InnerMapHexTile> occupiedHexTiles { get; }
        public override InnerMapHexTile occupiedHexTile => occupiedHexTiles.Count > 0 ? occupiedHexTiles[0] : null;
        
        public Cave(Region location) : base(STRUCTURE_TYPE.CAVE, location) {
            resourceYield = GetRandomResourceYield();
            occupiedHexTiles = new List<InnerMapHexTile>();
        }

        public Cave(Region location, SaveDataLocationStructure data) : base(location, data) { }

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
                randomYield.AddElement(Yield_Metal, 20);
                randomYield.AddElement(Yield_Diamond, 2);
                randomYield.AddElement(Yield_Gold, 0);
            } else if (chosen == 1) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Metal, 0);
                randomYield.AddElement(Yield_Diamond, 0);
                randomYield.AddElement(Yield_Gold, 10);
            } else if (chosen == 2) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Metal, 0);
                randomYield.AddElement(Yield_Diamond, 10);
                randomYield.AddElement(Yield_Gold, 0);
            } else if (chosen == 3) {
                randomYield.AddElement(Yield_Nothing, 100);
                randomYield.AddElement(Yield_Metal, 30);
                randomYield.AddElement(Yield_Diamond, 0);
                randomYield.AddElement(Yield_Gold, 0);
            } else if (chosen == 4) {
                randomYield.AddElement(Yield_Nothing, 100);
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
                occupiedHexTiles.Contains(tile.collectionOwner.partOfHextile) == false) {
                occupiedHexTiles.Add(tile.collectionOwner.partOfHextile);
            }
        }
        protected override void OnTileRemovedFromStructure(LocationGridTile tile) {
            base.OnTileRemovedFromStructure(tile);
            tile.genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.MINE);
        }
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            worldPosition = position;
        }
    }
}