﻿using Inner_Maps;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Traits;

public abstract class MapObject<T> : BaseMapObject where T: IDamageable {
    public BaseVisionTrigger visionTrigger {
        get {
            Assert.IsNotNull(mapVisual, $"{this.ToString()} had a problem getting its collision trigger! MapVisual: {mapVisual?.name ?? "null"}.");
            return mapVisual.visionTrigger;            
        }
    } 
    ///this is set in each inheritors implementation of <see cref="MapObject{T}.CreateMapObjectVisual"/>
    public virtual MapObjectVisual<T> mapVisual { get; protected set; }
    public override BaseMapObjectVisual baseMapObjectVisual => mapVisual;

    #region Initialization
    protected abstract void CreateMapObjectVisual();
    public virtual void InitializeMapObject(T obj) {
        CreateMapObjectVisual();
        mapVisual.Initialize(obj);
        InitializeVisionTrigger(obj);
        if (obj is TileObject tileObject) {
            tileObject.hiddenComponent.OnSetHiddenState();
            Assert.IsNotNull(tileObject.traitContainer, $"Trait Container of {tileObject.name} {tileObject.id.ToString()} {tileObject.tileObjectType.ToString()} is null!");
            List<Trait> traitOverrideFunctions = tileObject.traitContainer.GetTraitOverrideFunctions(TraitManager.Initiate_Map_Visual_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnInitiateMapObjectVisual(tileObject);
                }
            }
        }
    }
    #endregion

    #region Placement
    protected void PlaceMapObjectAt(LocationGridTile tile) {
        mapVisual.PlaceObjectAt(tile);
        visionTrigger.gameObject.SetActive(true);
    }
    #endregion

    #region Visuals
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        mapVisual = null;
    }
    #endregion

    #region Collision
    private void InitializeVisionTrigger(T obj) {
        visionTrigger.Initialize(obj);
    }
    #endregion
}
