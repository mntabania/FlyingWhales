using Inner_Maps;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Traits;

public abstract class MapObject<T> : BaseMapObject where T: IDamageable {
    public BaseCollisionTrigger<T> collisionTrigger {
        get {
            Assert.IsNotNull(mapVisual, $"{this.ToString()} had a problem getting its collision trigger! MapVisual: {mapVisual?.name ?? "null"}.");
            return mapVisual.collisionTrigger;            
        }
    } 
    ///this is set in each inheritors implementation of <see cref="MapObject{T}.CreateMapObjectVisual"/>
    public virtual MapObjectVisual<T> mapVisual { get; protected set; }
    public override BaseMapObjectVisual baseMapObjectVisual => mapVisual;

    #region Initialization
    protected abstract void CreateMapObjectVisual();
    public void InitializeMapObject(T obj) {
        CreateMapObjectVisual();
        mapVisual.Initialize(obj);
        InitializeCollisionTrigger(obj);
        if (mapVisual.selectable is TileObject tileObject) {
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
        collisionTrigger.gameObject.SetActive(true);
    }
    #endregion

    #region Visuals
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        mapVisual = null;
    }
    #endregion

    #region Collision
    private void InitializeCollisionTrigger(T obj) {
        collisionTrigger.Initialize(obj);
        mapVisual.UpdateCollidersState(obj);
    }
    #endregion
}
