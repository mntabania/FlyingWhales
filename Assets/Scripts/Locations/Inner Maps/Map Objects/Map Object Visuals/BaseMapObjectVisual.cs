using EZObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Base class to be used for the visuals of any objects that are Settlement Map Objects.
/// </summary>
public abstract class BaseMapObjectVisual : PooledObject, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] protected SpriteRenderer objectVisual;
    [SerializeField] protected SpriteRenderer hoverObject;
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
    
    #region Visuals
    public void SetRotation(float rotation) {
        this.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
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
    public void SetVisualAlpha(float alpha) {
        Color color = objectVisual.color;
        color.a = alpha;
        SetColor(color);
    }
    public void SetHoverObjectState(bool state) {
        if (isHoverObjectStateLocked) {
            return; //ignore change because hover state is locked
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
    public virtual void LookAt(Vector3 target, bool force = false) {

    }
    /// <summary>
    /// Rotate this marker to a specific angle.
    /// </summary>
    /// <param name="target">The angle this character must rotate to.</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public virtual void Rotate(Quaternion target, bool force = false) {

    }
    #endregion

    #region Inquiry
    /// <summary>
    /// Is this objects menu (CharacterInfoUI, TileObjectInfoUI) currently showing this objects info?
    /// </summary>
    /// <returns>True or false</returns>
    public abstract bool IsMapObjectMenuVisible();
    public bool IsInvisible() {
        if (ReferenceEquals(objectVisual, null) == false) {
            return Mathf.Approximately(objectVisual.color.a, 0f);    
        }
        return false;
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
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        if (objectVisual != null ) {
            SetVisualAlpha(255f / 255f);    
        }
    }
    void OnEnable() {
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    void OnDisable() {
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
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
}