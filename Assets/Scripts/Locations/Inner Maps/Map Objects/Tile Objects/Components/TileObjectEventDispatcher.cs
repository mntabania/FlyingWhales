public class TileObjectEventDispatcher {
    public interface IDestroyedListener {
        void OnTileObjectDestroyed(TileObject p_tileObject);
    }

    private System.Action<TileObject> _tileObjectDestroyed;
        
    #region Death
    public void SubscribeToTileObjectDestroyed(IDestroyedListener p_listener) {
        _tileObjectDestroyed += p_listener.OnTileObjectDestroyed;
    }
    public void UnsubscribeToTileObjectDestroyed(IDestroyedListener p_listener) {
        _tileObjectDestroyed -= p_listener.OnTileObjectDestroyed;
    }
    public void ExecuteTileObjectDestroyed(TileObject p_tileObject) {
        _tileObjectDestroyed?.Invoke(p_tileObject);
    }
    #endregion;
}
