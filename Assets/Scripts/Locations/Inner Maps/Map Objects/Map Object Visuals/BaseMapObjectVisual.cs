using EZObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Base class to be used for the visuals of any objects that are NPCSettlement Map Objects.
/// </summary>
public abstract class BaseMapObjectVisual : PooledObject, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] protected SpriteRenderer objectVisual;
    [SerializeField] protected SpriteRenderer hoverObject;
    [SerializeField] protected Collider2D clickCollider; //collider responsible for receiving pointer events

    [Header("HP")]
    public GameObject hpBarGO;
    public Image hpFill;
    public Image aspeedFill;

    public Transform statusIconsParent;

    private bool isHoverObjectStateLocked;
    protected System.Action onHoverOverAction;
    protected System.Action onHoverExitAction;
    protected System.Action onLeftClickAction;
    protected System.Action onRightClickAction;
    public GameObject gameObjectVisual => this.gameObject;
    public Sprite usedSprite => objectVisual.sprite;
    public ISelectable selectable { get; protected set; }
    public SpriteRenderer objectSpriteRenderer => objectVisual;
    public BaseVisionTrigger visionTrigger { get; protected set; }

    ///this is null by default. This is responsible for updating the pathfinding graph when a tileobject that should be unapassable is placed
    /// <see cref="LocationGridTileGUS.Initialize"/>,
    /// this should also destroyed when the object is removed. <see cref="LocationGridTileGUS.Destroy"/>
    private LocationGridTileGUS graphUpdateScene { get; set; } 
    
    #region Initialization
    protected void Initialize(ISelectable selectable) {
        this.selectable = selectable;
        UpdateClickableColliderState();
    }
    #endregion
    
    #region Visuals
    public virtual Sprite GetSeizeSprite(IPointOfInterest poi) {
        return objectVisual.sprite;
    }
    public void SetRotation(float rotation) {
        Quaternion quaternion = Quaternion.Euler(0f, 0f, rotation);
        objectVisual.transform.localRotation = quaternion;
        if (hoverObject != null) {
            hoverObject.transform.localRotation = quaternion;    
        }
    }
    public void SetVisual(Sprite sprite) {
        objectVisual.sprite = sprite;
        hoverObject.sprite = sprite;
    }
    public void SetColor(Color color) {
        objectVisual.color = color;
    }
    public void SetActiveState(bool state) {
        this.gameObject.SetActive(state);
    }
    public virtual void SetVisualAlpha(float alpha) {
        Color color = objectVisual.color;
        color.a = alpha;
        SetColor(color);
    }
    public void SetHoverObjectState(bool state) {
        if (isHoverObjectStateLocked) {
            return; //ignore change because hover state is locked
        }
        if (PlayerManager.Instance.player.IsPerformingPlayerAction()) {
            return; //player is currently performing an action, do not highlight.
        }
        if (hoverObject.gameObject.activeSelf == state) {
            return; //ignore change
        }
        hoverObject.gameObject.SetActive(state);
    }
    public void LockHoverObject() {
        isHoverObjectStateLocked = true;
    }
    public void UnlockHoverObject() {
        isHoverObjectStateLocked = false;
    }
    public StatusIcon AddStatusIcon(string statusName) {
        GameObject statusGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(
            TraitManager.Instance.traitIconPrefab.name, Vector3.zero, Quaternion.identity, statusIconsParent);
        StatusIcon icon = statusGO.GetComponent<StatusIcon>();
        icon.SetIcon(TraitManager.Instance.GetTraitIcon(statusName));
        return icon;
    }
    public abstract void ApplyFurnitureSettings(FurnitureSetting furnitureSetting);
    /// <summary>
    /// Make this marker look at a specific point (In World Space).
    /// </summary>
    /// <param name="target">The target point in world space</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public virtual void LookAt(Vector3 target, bool force = false) { }
    /// <summary>
    /// Rotate this marker to a specific angle.
    /// </summary>
    /// <param name="target">The angle this character must rotate to.</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public virtual void Rotate(Quaternion target, bool force = false) { }
    public void SetMaterial(Material material) {
        objectVisual.material = material;
    }
    #endregion

    #region Inquiry
    public bool IsInvisibleToPlayer() {
        if (ReferenceEquals(objectVisual, null) == false) {
            return Mathf.Approximately(objectVisual.color.a, 0f);    
        }
        return true;
    }
    #endregion 

    #region Pointer Functions
    public void ExecuteClickAction(PointerEventData.InputButton button) {
        if (button == PointerEventData.InputButton.Left) {
            onLeftClickAction?.Invoke();
        }else if (button == PointerEventData.InputButton.Right) {
            onRightClickAction?.Invoke();
        }
    }
    public void OnPointerEnter(PointerEventData eventData) {
        ExecuteHoverEnterAction();
    }
    public void OnPointerExit(PointerEventData eventData) {
        ExecuteHoverExitAction();
    }
    public void ExecuteHoverEnterAction() {
        onHoverOverAction?.Invoke();
    }
    public void ExecuteHoverExitAction() {
        onHoverExitAction?.Invoke();
    }
    private void UpdateClickableColliderState() {
        if (clickCollider != null) {
            clickCollider.enabled = selectable.CanBeSelected();    
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        if (objectVisual != null ) {
            SetVisualAlpha(255f / 255f);    
        }
        if (hpBarGO) {
            HideHPBar();
        }
        if (visionTrigger) {
            visionTrigger.Reset();    
        }
    }
    void OnEnable() {
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    void OnDisable() {
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    public override void BeforeDestroyActions() {
        base.BeforeDestroyActions();
        DestroyExistingGUS();
    }
    #endregion

    #region Tweening
    private void OnGamePaused(bool state) {
        if (state) {
            transform.DOPause();
        } else {
            transform.DOPlay();
        }
    }
    public bool IsTweening() {
        return DOTween.IsTweening(this.transform);
    }
    public void TweenTo(Transform _target, float duration, System.Action _onReachTargetAction) {
        var position = _target.position;
        Tweener tween = transform.DOMove(position, duration).SetEase(Ease.Linear).SetAutoKill(false).OnComplete(_onReachTargetAction.Invoke);
        tween.OnUpdate (() => tween.ChangeEndValue (_target.position, true));
        if (GameManager.Instance.isPaused) {
            transform.DOPause();
        } else {
            transform.DOPlay();
        }
    }
    public void OnReachTarget() {
        DOTween.Kill(this.transform);
    }
    #endregion

    #region Placement
    public virtual void PlaceObjectAt(LocationGridTile tile) {
        Transform thisTransform = transform;
        thisTransform.SetParent(tile.parentMap.structureParent);
        Vector3 worldPos = tile.centeredWorldLocation;
        thisTransform.position = worldPos;
    }
    #endregion

    #region Combat
    public void ShowHPBar(IPointOfInterest poi) {
        hpBarGO.SetActive(true);
        UpdateHP(poi);
    }
    public void HideHPBar() {
        hpBarGO.SetActive(false);
    }
    public void UpdateHP(IPointOfInterest poi) {
        if (hpBarGO.activeSelf) {
            hpFill.fillAmount = (float) poi.currentHP / poi.maxHP;
        }
    }
    public void QuickShowHPBar(IPointOfInterest poi) {
        StartCoroutine(QuickShowHPBarCoroutine(poi));
    }
    private IEnumerator QuickShowHPBarCoroutine(IPointOfInterest poi) {
        ShowHPBar(poi);
        yield return new WaitForSeconds(2f);
        if (!(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && (poi as Character).combatComponent.isInCombat)) {
            HideHPBar();
        }
    }
    #endregion
    
    #region Graph Updates
    public void InitializeGUS(Vector2 offset, Vector2 size) {
        if (graphUpdateScene == null) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocationGridTileGUS", Vector3.zero, Quaternion.identity, transform);
            LocationGridTileGUS gus = go.GetComponent<LocationGridTileGUS>();
            graphUpdateScene = gus;
        }
        graphUpdateScene.Initialize(offset, size);
    }
    public void DestroyExistingGUS() {
        if (graphUpdateScene == null) return;
        graphUpdateScene.Destroy();
        graphUpdateScene = null;
    }
    public void ApplyGraphUpdate() {
        graphUpdateScene.Apply();
    }
    #endregion
}