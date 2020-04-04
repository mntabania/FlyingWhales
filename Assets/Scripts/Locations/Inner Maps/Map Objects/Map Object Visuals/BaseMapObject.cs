using UnityEngine.Assertions;
using Traits;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

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
    protected System.Func<BaseMapObject, bool> _unbuiltObjectValidityChecker;
    
    
    #region Object State
    public void SetMapObjectState(MAP_OBJECT_STATE state, System.Func<BaseMapObject, bool> unbuiltObjectValidityChecker = null) {
        if (mapObjectState == state) {
            return; //ignore change
        }
        mapObjectState = state;
        _unbuiltObjectValidityChecker = unbuiltObjectValidityChecker;
        OnMapObjectStateChanged();
    }
    protected abstract void OnMapObjectStateChanged();
    #endregion

    #region Manipulation
    public void OnManipulatedBy(IObjectManipulator newManipulator) {
        IObjectManipulator previousManipulator = lastManipulatedBy;
        lastManipulatedBy = newManipulator;
        if (newManipulator is Player && (previousManipulator is Player) == false) {
            //if object was manipulated by the player, and it wasn't previously, vote to make it visible to characters
            baseMapObjectVisual.visionTrigger.VoteToMakeVisibleToCharacters();
        } else if (newManipulator is Character && previousManipulator is Player) {
            //if object was manipulated by a character, check if the previous manipulator was the player,
            //if it was, then vote to make this object invisible to characters.
            baseMapObjectVisual.visionTrigger.VoteToMakeInvisibleToCharacters();
        }    
        
        
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
        string data = $"\n\tLast Manipulated by: {lastManipulatedBy}";
        if (baseMapObjectVisual != null) {
            data += $"\n\tVision Votes: {baseMapObjectVisual.visionTrigger.filterVotes.ToString()}";    
        }
        return data;
    }
    #endregion
    
}
