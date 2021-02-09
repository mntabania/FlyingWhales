namespace Inner_Maps {
    public class LocationGridTileArray {

        private readonly LocationGridTile[] _tiles;
        private int _latestIndex;

        #region getters
        public LocationGridTile[] tiles => _tiles;
        #endregion
        
        public LocationGridTileArray(int p_size) {
            _tiles = new LocationGridTile[p_size];
            _latestIndex = 0;
        }

        public void AddTileToArray(LocationGridTile p_tile) {
            _tiles[_latestIndex] = p_tile;
            _latestIndex++;
        }
    }
}