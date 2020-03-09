using UnityEngine.Assertions;
using Traits;
using System.Collections.Generic;

/// <summary>
/// Base class for anything in the npcSettlement map that can be damaged and has a physical object to be shown.
/// </summary>
public abstract class BaseMapObject {
    
    public MAP_OBJECT_STATE mapObjectState { get; private set; }
    /// <summary>
    /// The last manipulator that interacted with this object.
    /// </summary>
    public IObjectManipulator lastManipulatedBy { get; private set; }
    public abstract BaseMapObjectVisual baseMapObjectVisual { get; } 

    #region Object State
    public void SetMapObjectState(MAP_OBJECT_STATE state) {
        if (mapObjectState == state) {
            return; //ignore change
        }
        mapObjectState = state;
        OnMapObjectStateChanged();
    }
    protected abstract void OnMapObjectStateChanged();
    #endregion

    #region Manipulation
    public void OnManipulatedBy(IObjectManipulator manipulator) {
        lastManipulatedBy = manipulator;
    }
    #endregion

    #region Visuals
    protected void DisableGameObject() {
        baseMapObjectVisual.SetActiveState(false);
    }
    protected void EnableGameObject() {
        baseMapObjectVisual.SetActiveState(true);
    }
    public virtual void DestroyMapVisualGameObject() {
        Assert.IsNotNull(baseMapObjectVisual, $"Trying to destroy map visual of {this.ToString()} but map visual is null!");
        if(baseMapObjectVisual.selectable is TileObject tileObject) {
            List<Trait> traitOverrideFunctions = tileObject.traitContainer.GetTraitOverrideFunctions(TraitManager.Destroy_Map_Visual_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnDestroyMapObjectVisual(tileObject);
                }
            }
        }
        ObjectPoolManager.Instance.DestroyObject(baseMapObjectVisual);
    }
    #endregion

    #region Testing
    public virtual string GetAdditionalTestingData() {
        return $"\n\tLast Manipulated by: {lastManipulatedBy}";
    }
    #endregion

    
}
