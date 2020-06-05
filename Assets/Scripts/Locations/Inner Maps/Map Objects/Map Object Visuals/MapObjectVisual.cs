using Inner_Maps;
using UnityEngine;

public abstract class MapObjectVisual<T> : BaseMapObjectVisual where T : IDamageable {
    protected T obj { get; private set; }
    
    public virtual void Initialize(T obj) {
        base.Initialize(obj as ISelectable);
        this.obj = obj;
        onHoverOverAction = () => OnPointerEnter(obj);
        onHoverExitAction = () => OnPointerExit(obj);
        onLeftClickAction = () => OnPointerLeftClick(obj);
        onRightClickAction = () => OnPointerRightClick(obj);
    }

    #region Visuals
    public abstract void UpdateTileObjectVisual(T obj);
    public virtual void UpdateSortingOrders(T obj) {
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
    protected virtual void OnPointerEnter(T character) {
        SetHoverObjectState(true);
    }
    protected virtual void OnPointerExit(T poi) {
        SetHoverObjectState(false);
    }
    protected virtual void OnPointerLeftClick(T poi) { }
    protected virtual void OnPointerRightClick(T poi) { }
    #endregion

    #region General
    public bool IsNear(Vector3 pos) {
        return Vector3.Distance(transform.position, pos) <= 0.75f;
    }
    #endregion
}