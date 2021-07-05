using Inner_Maps.Location_Structures;

public class LocationStructureEventDispatcher {
    public interface IDestroyedListener {
        void OnStructureDestroyed(LocationStructure p_structure);
    }
    private System.Action<LocationStructure> _structureDestroyed;
    
    #region Destroyed
    public void SubscribeToStructureDestroyed(IDestroyedListener p_listener) {
        _structureDestroyed += p_listener.OnStructureDestroyed;
    }
    public void UnsubscribeToStructureDestroyed(IDestroyedListener p_listener) {
        _structureDestroyed -= p_listener.OnStructureDestroyed;
    }
    public void ExecuteStructureDestroyed(LocationStructure p_structure) {
        _structureDestroyed?.Invoke(p_structure);
    }
    #endregion
}
