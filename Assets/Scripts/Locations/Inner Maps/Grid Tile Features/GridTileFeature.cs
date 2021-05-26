using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;
namespace Inner_Maps.Grid_Tile_Features {
    public abstract class GridTileFeature {

        protected List<LocationGridTile> _tilesWithFeature;

        #region getters
        public List<LocationGridTile> tilesWithFeature => _tilesWithFeature;
        public virtual System.Type serializedData => typeof(SaveDataGridTileFeature);
        #endregion
        
        public GridTileFeature() {
            _tilesWithFeature = new List<LocationGridTile>();
        }
        public GridTileFeature(SaveDataGridTileFeature p_data) : this() { }

        #region Loading
        public virtual void LoadReferences(SaveDataGridTileFeature p_data) {
            for (int i = 0; i < p_data.tiles.Length; i++) {
                TileLocationSave tileLocationSave = p_data.tiles[i];
                LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
                _tilesWithFeature.Add(tile);
            }
        }
        #endregion

        #region Initialization
        public abstract void Initialize();
        #endregion
        
        #region Tile Management
        public void AddTile(LocationGridTile p_tile) {
            if (!_tilesWithFeature.Contains(p_tile)) {
                _tilesWithFeature.Add(p_tile);
            }
        }
        public virtual bool RemoveTile(LocationGridTile p_tile) {
            return _tilesWithFeature.Remove(p_tile);
        }
        #endregion
    }
}

#region Save Data
public abstract class SaveDataGridTileFeature : SaveData<GridTileFeature> {
    public TileLocationSave[] tiles;
    public override void Save(GridTileFeature data) {
        base.Save(data);
        tiles = new TileLocationSave[data.tilesWithFeature.Count];
        for (int i = 0; i < data.tilesWithFeature.Count; i++) {
            LocationGridTile tile = data.tilesWithFeature[i];
            tiles[i] = new TileLocationSave(tile);
        }
    }
}
#endregion