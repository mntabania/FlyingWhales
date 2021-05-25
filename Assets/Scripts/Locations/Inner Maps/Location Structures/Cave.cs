using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Cave : NaturalStructure {

        private static WeightedDictionary<CONCRETE_RESOURCES> _resourceWeights;

        public CONCRETE_RESOURCES producedResource { get; }
        public List<LocationGridTile> stoneSpots { get; }
        public List<LocationGridTile> oreSpots { get; }
        
        
        #region getters
        public override System.Type serializedData => typeof(SaveDataCave);
        #endregion

        public Cave(Region location) : base(STRUCTURE_TYPE.CAVE, location) {
            if (_resourceWeights == null) {
                _resourceWeights = new WeightedDictionary<CONCRETE_RESOURCES>();
                _resourceWeights.AddElement(CONCRETE_RESOURCES.Copper, 100);
                _resourceWeights.AddElement(CONCRETE_RESOURCES.Iron, 80);
                _resourceWeights.AddElement(CONCRETE_RESOURCES.Mithril, 40);
                _resourceWeights.AddElement(CONCRETE_RESOURCES.Orichalcum, 20);
            }
            producedResource = _resourceWeights.PickRandomElementGivenWeights();
            stoneSpots = new List<LocationGridTile>();
            oreSpots = new List<LocationGridTile>();
        }

        public Cave(Region location, SaveDataNaturalStructure data) : base(location, data) {
            SaveDataCave saveDataCave = data as SaveDataCave;
            producedResource = saveDataCave.producedResource;
            oreSpots = new List<LocationGridTile>();
            stoneSpots = new List<LocationGridTile>();
        }
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataCave saveDataCave = saveDataLocationStructure as SaveDataCave;
            
            for (int i = 0; i < saveDataCave.oreSpots.Length; i++) {
                TileLocationSave saveData = saveDataCave.oreSpots[i];
                LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveData);
                oreSpots.Add(tile);
            }
            
            for (int i = 0; i < saveDataCave.stoneSpots.Length; i++) {
                TileLocationSave saveData = saveDataCave.stoneSpots[i];
                LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveData);
                stoneSpots.Add(tile);
            }
        }
        protected override void OnTileAddedToStructure(LocationGridTile tile) {
            base.OnTileAddedToStructure(tile);
            tile.SetElevation(ELEVATION.MOUNTAIN);
            tile.tileObjectComponent.genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.MINE);
            //if (!caveAreas.Contains(tile.area)) {
            //    caveAreas.Add(tile.area);
            //}
        }
        protected override void OnTileRemovedFromStructure(LocationGridTile tile) {
            base.OnTileRemovedFromStructure(tile);
            tile.tileObjectComponent.genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.MINE);
        }
        public override void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != region.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (region.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(region);
            }
            if (occupiedArea != null) {
                float centerX = 0f;
                float centerY = 0f;
                for (int i = 0; i < occupiedAreas.Keys.Count; i++) {
                    Area area = occupiedAreas.Keys.ElementAt(i);
                    Vector2 worldLocation = area.gridTileComponent.centerGridTile.centeredWorldLocation;
                    centerX += worldLocation.x;
                    centerY += worldLocation.y;
                }
                Vector2 finalPos = new Vector2(centerX / occupiedAreas.Count, centerY / occupiedAreas.Count);
                InnerMapCameraMove.Instance.CenterCameraOn(finalPos);
            }
        }
        public override void ShowSelectorOnStructure() { }

        #region Mining Spots
        public void AddStoneSpot(LocationGridTile p_tile) {
            if (!stoneSpots.Contains(p_tile)) {
                stoneSpots.Add(p_tile);
                //create rock at location
                TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK);
                AddPOI(tileObject, p_tile);
            }
            
        }
        public void AddOreSpot(LocationGridTile p_tile) {
            if (!oreSpots.Contains(p_tile)) {
                oreSpots.Add(p_tile);
                //create ore vein at location
                Ore tileObject = InnerMapManager.Instance.CreateNewTileObject<Ore>(TILE_OBJECT_TYPE.ORE);
                tileObject.SetProvidedMetal(producedResource);
                AddPOI(tileObject, p_tile);    
            }
        }
        #endregion

        #region Testing
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            info = $"{info}\nProduced Resource: {producedResource.ToString()}";
            info = $"{info}\nStone Spots: {stoneSpots.ComafyList()}";
            info = $"{info}\nOre Spots: {oreSpots.ComafyList()}";
            return info;
        }
        #endregion
        
    }
}

#region Save Data
public class SaveDataCave : SaveDataNaturalStructure {
    public CONCRETE_RESOURCES producedResource;
    public TileLocationSave[] stoneSpots;
    public TileLocationSave[] oreSpots;
    public override void Save(LocationStructure structure) {
        base.Save(structure);
        Cave cave = structure as Cave;
        producedResource = cave.producedResource;
        
        oreSpots = new TileLocationSave[cave.oreSpots.Count];
        for (int i = 0; i < oreSpots.Length; i++) {
            LocationGridTile oreSpot = cave.oreSpots[i];
            oreSpots[i] = new TileLocationSave(oreSpot);
        }
        
        stoneSpots = new TileLocationSave[cave.stoneSpots.Count];
        for (int i = 0; i < stoneSpots.Length; i++) {
            LocationGridTile stoneSpot = cave.stoneSpots[i];
            stoneSpots[i] = new TileLocationSave(stoneSpot);
        }
    }
}
#endregion