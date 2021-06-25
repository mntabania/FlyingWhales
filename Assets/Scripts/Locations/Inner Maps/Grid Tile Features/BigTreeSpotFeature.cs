using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Grid_Tile_Features {
    public class BigTreeSpotFeature : GridTileFeature {
        
        private List<LocationGridTile> _unoccupiedSpots;

        #region getters
        public List<LocationGridTile> unoccupiedSpots => _unoccupiedSpots;
        public override System.Type serializedData => typeof(SaveDataBigTreeSpotFeature);
        #endregion
        
        public BigTreeSpotFeature() {
            _unoccupiedSpots = new List<LocationGridTile>();
        }
        public BigTreeSpotFeature(SaveDataBigTreeSpotFeature p_data) : base(p_data) {
            _unoccupiedSpots = new List<LocationGridTile>();
        }
        
        public override void Initialize() {
            Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
            Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        
        #region Loading
        public override void LoadReferences(SaveDataGridTileFeature p_data) {
            base.LoadReferences(p_data);
            SaveDataBigTreeSpotFeature data = p_data as SaveDataBigTreeSpotFeature;
            for (int i = 0; i < data.unoccupiedTiles.Length; i++) {
                TileLocationSave tileLocationSave = data.unoccupiedTiles[i];
                LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
                _unoccupiedSpots.Add(tile);
            }
        }
        #endregion
        
        #region Listeners
        private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom) {
            if (p_tileObject.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT && tilesWithFeature.Contains(p_removedFrom) &&
                !_unoccupiedSpots.Contains(p_removedFrom)) {
                _unoccupiedSpots.Add(p_removedFrom);
            }
        }
        #endregion

        #region Tile Managemenet
        public override bool RemoveTile(LocationGridTile p_tile) {
            if (base.RemoveTile(p_tile)) {
                _unoccupiedSpots.Remove(p_tile);
                return true;
            }
            return false;
        }
        #endregion
        
        private void OnDayStarted() {
            for (int i = 0; i < _unoccupiedSpots.Count; i++) {
                LocationGridTile tile = _unoccupiedSpots[i];
                if (tile.tileObjectComponent.objHere == null && tile.structure is Wilderness && InnerMapManager.Instance.CanBigTreeBePlacedOnTile(tile)) {
                    if (GameUtilities.RollChance(30)) {
                        //Spawn tree.
                        TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
                        tile.structure.AddPOI(tileObject, tile);
                    }        
                }
            }
            _unoccupiedSpots.Clear();
        }
    }
}

#region Save Data
public class SaveDataBigTreeSpotFeature : SaveDataGridTileFeature {
    public TileLocationSave[] unoccupiedTiles;
    public override void Save(GridTileFeature p_data) {
        base.Save(p_data);
        BigTreeSpotFeature data = p_data as BigTreeSpotFeature;
        Assert.IsNotNull(data);
        unoccupiedTiles = new TileLocationSave[data.unoccupiedSpots.Count];
        for (int i = 0; i < data.unoccupiedSpots.Count; i++) {
            LocationGridTile tile = data.unoccupiedSpots[i];
            unoccupiedTiles[i] = new TileLocationSave(tile);
        }
    }
    public override GridTileFeature Load() {
        return new BigTreeSpotFeature(this);
    }
}
#endregion