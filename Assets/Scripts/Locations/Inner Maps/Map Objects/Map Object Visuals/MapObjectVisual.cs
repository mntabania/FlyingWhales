using System.Collections;
using Inner_Maps;
using UnityEngine;

public abstract class MapObjectVisual<T> : BaseMapObjectVisual where T : IDamageable {
    public BaseCollisionTrigger<T> collisionTrigger { get; protected set; }

    public T obj { get; private set; }
    public virtual void Initialize(T obj) {
        selectable = obj as ISelectable;
        this.obj = obj;
        onHoverOverAction = () => OnPointerEnter(obj);
        onHoverExitAction = () => OnPointerExit(obj);
        onLeftClickAction = () => OnPointerLeftClick(obj);
        onRightClickAction = () => OnPointerRightClick(obj);
    }

    #region Visuals
    public abstract void UpdateTileObjectVisual(T obj);
    protected virtual void UpdateSortingOrders(T obj) {
        if (objectVisual != null) {
            objectVisual.sortingLayerName = "Area Maps";
            objectVisual.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder;    
        }
        if (hoverObject != null) {
            hoverObject.sortingLayerName = "Area Maps";
            hoverObject.sortingOrder = objectVisual.sortingOrder - 1;    
        }
    }
    #endregion

    #region Pointer Functions
    protected virtual void OnPointerEnter(T poi) {
        SetHoverObjectState(true);
    }
    protected virtual void OnPointerExit(T poi) {
        SetHoverObjectState(false);
    }
    protected virtual void OnPointerLeftClick(T poi) { }
    protected virtual void OnPointerRightClick(T poi) { }
    #endregion

    #region Collisions
    public abstract void UpdateCollidersState(T obj);
    /// <summary>
    /// Set this object to be invisible to characters.
    /// NOTE: Characters vision are at the Filtered Vision Collision layer, so they can only see objects in that layer.
    /// Other objects like the Tornado that is in the All Vision Collision Layer can still see objects that are not
    /// at the Filtered Vision Collision layer.
    /// </summary>
    protected void SetAsInvisibleToCharacters() {
//        GameManager.Instance.StartCoroutine(ChangeCollidersLayer("All Vision Collision")); //Move colliders to all vision collision so it can still be seen by objects that collide with everything
        collisionTrigger.gameObject.tag = InnerMapManager.InvisibleToCharacterTag;
    }
    /// <summary>
    /// Set this object to be visible to characters.
    /// NOTE: Characters vision are at the Filtered Vision Collision layer, so they can only see objects in that layer.
    /// Other objects like the Tornado that is in the All Vision Collision Layer can still see objects that are not
    /// at the Filtered Vision Collision layer.
    /// </summary>
    protected void SetAsVisibleToCharacters() {
//        GameManager.Instance.StartCoroutine(ChangeCollidersLayer("Filtered Vision Collision")); //Move colliders to filtered vision collision so it can be seen by characters and other objects in the all vision collision layer
        collisionTrigger.gameObject.tag = InnerMapManager.VisibleAllTag;
    }
    private IEnumerator ChangeCollidersLayer(string layerName) {
        collisionTrigger.SetCollidersState(false);
        yield return new WaitForFixedUpdate();
        collisionTrigger.SetColliderLayer(layerName);
        collisionTrigger.SetCollidersState(true);
    }
    #endregion
    
    #region General
    public bool IsNear(Vector3 pos) {
        return Vector3.Distance(transform.position, pos) <= 0.75f;
    }
    #endregion
}