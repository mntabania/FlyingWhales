using Traits;

public class TileObjectEventDispatcher {
    public interface IDestroyedListener {
        void OnTileObjectDestroyed(TileObject p_tileObject);
    }
    public interface ITraitListener {
        void OnTileObjectGainedTrait(TileObject p_tileObject, Trait p_trait);
        void OnTileObjectLostTrait(TileObject p_tileObject, Trait p_trait);
    }

    private System.Action<TileObject> _tileObjectDestroyed;
    private System.Action<TileObject, Trait> _tileObjectGainedTrait;
    private System.Action<TileObject, Trait> _tileObjectLostTrait;
        
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
    #endregion

    #region Traits
    public void SubscribeToTileObjectGainedTrait(ITraitListener p_listener) {
        _tileObjectGainedTrait += p_listener.OnTileObjectGainedTrait;
    }
    public void UnsubscribeToTileObjectGainedTrait(ITraitListener p_listener) {
        _tileObjectGainedTrait -= p_listener.OnTileObjectGainedTrait;
    }
    public void ExecuteTileObjectGainedTrait(TileObject p_tileObject, Trait p_trait) {
        _tileObjectGainedTrait?.Invoke(p_tileObject, p_trait);
    }
    public void SubscribeToTileObjectLostTrait(ITraitListener p_listener) {
        _tileObjectLostTrait += p_listener.OnTileObjectLostTrait;
    }
    public void UnsubscribeToTileObjectLostTrait(ITraitListener p_listener) {
        _tileObjectLostTrait -= p_listener.OnTileObjectLostTrait;
    }
    public void ExecuteTileObjectLostTrait(TileObject p_tileObject, Trait p_trait) {
        _tileObjectLostTrait?.Invoke(p_tileObject, p_trait);
    }
    #endregion
}
