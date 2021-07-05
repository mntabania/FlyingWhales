using EZObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilityScripts;

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
    public Transform particleEffectParent;

    public Transform statusIconsParent;

    private bool isHoverObjectStateLocked;
    protected System.Action onHoverOverAction;
    protected System.Action onHoverExitAction;
    protected System.Action onLeftClickAction;
    protected System.Action onRightClickAction;
    protected System.Action onMiddleClickAction;
    public GameObject gameObjectVisual => this.gameObject;
    public Sprite usedSprite => objectVisual.sprite;
    //public Quaternion rotation => objectVisual.transform.localRotation;
    public ISelectable selectable { get; protected set; }
    public SpriteRenderer objectSpriteRenderer => objectVisual;
    public BaseVisionTrigger visionTrigger { get; protected set; }

    ///this is null by default. This is responsible for updating the pathfinding graph when a tileobject that should be unapassable is placed
    /// <see cref="LocationGridTileGUS.Initialize"/>,
    /// this should also destroyed when the object is removed. <see cref="LocationGridTileGUS.Destroy"/>
    private LocationGridTileGUS graphUpdateScene { get; set; } 
    public string usedSpriteName { get; private set; }
    public Quaternion rotation { get; private set; }

    #region Initialization
    protected void Initialize(ISelectable selectable) {
        this.selectable = selectable;
        UpdateClickableColliderState();    }
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
        this.rotation = quaternion;
    }
    public void SetRotation(Quaternion rotation) {
        objectVisual.transform.localRotation = rotation;
        if (hoverObject != null) {
            hoverObject.transform.localRotation = rotation;    
        }
        this.rotation = rotation;
    }
    public virtual void SetVisual(Sprite sprite) {
        objectVisual.sprite = sprite;
        hoverObject.sprite = sprite;
        if (sprite != null) {
            usedSpriteName = sprite.name;
        } else {
            usedSpriteName = string.Empty;
        }
    }
    private void SetColor(Color color) {
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
    protected void SetHoverObjectState(bool state) {
        if (isHoverObjectStateLocked) {
            return; //ignore change because hover state is locked
        }
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && PlayerManager.Instance.player.IsPerformingPlayerAction()) {
            return; //player is currently performing an action, do not highlight.
        }
        if (hoverObject.gameObject.activeSelf == state) {
            return; //ignore change
        }
        hoverObject.gameObject.SetActive(state);
    }
    protected void LockHoverObject() {
        isHoverObjectStateLocked = true;
    }
    protected void UnlockHoverObject() {
        isHoverObjectStateLocked = false;
    }
    public StatusIcon AddStatusIcon(string statusName) {
        GameObject statusGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(
            TraitManager.Instance.traitIconPrefab.name, Vector3.zero, Quaternion.identity, statusIconsParent);
        StatusIcon icon = statusGO.GetComponent<StatusIcon>();
        icon.SetIcon(TraitManager.Instance.GetTraitIcon(statusName));
        return icon;
    }
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
        } else if (button == PointerEventData.InputButton.Right) {
            onRightClickAction?.Invoke();
        } else if (button == PointerEventData.InputButton.Middle) {
            onMiddleClickAction?.Invoke();
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
        isHoverObjectStateLocked = false;
        if (objectVisual != null ) {
            SetVisualAlpha(1f);    
        }
        if (hpBarGO) {
            HideHPBar();
        }
        if (visionTrigger) {
            visionTrigger.Reset();    
        }
        if (clickCollider != null) {
            clickCollider.enabled = true;
        }
        selectable = null;
        DestroyAllStatusIcons();
        DestroyAllParticleEffects();
        SetMaterial(InnerMapManager.Instance.assetManager.defaultObjectMaterial);
        DOTween.Kill(this.transform);
    }
    private void DestroyAllStatusIcons() {
        if (statusIconsParent != null) {
            Transform[] gos = GameUtilities.GetComponentsInDirectChildren<Transform>(statusIconsParent.gameObject);
            if (gos != null) {
                for (int i = 0; i < gos.Length; i++) {
                    ObjectPoolManager.Instance.DestroyObject(gos[i].gameObject);
                }
            }
        }
    }
    protected virtual void DestroyAllParticleEffects() {
        if (particleEffectParent != null) {
            //When a map visual object is object pooled, all particles must be destroyed so that when it is used again there will no residual particle effects that will linger
            Transform[] particleGOs = GameUtilities.GetComponentsInDirectChildren<Transform>(particleEffectParent.gameObject);
            if (particleGOs != null) {
                for (int i = 0; i < particleGOs.Length; i++) {
                    ObjectPoolManager.Instance.DestroyObject(particleGOs[i].gameObject);
                }
            }
        }
    }
    // void OnEnable() {
    //     Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
    // }
    // void OnDisable() {
    //     Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
    // }
    public override void BeforeDestroyActions() {
        base.BeforeDestroyActions();
        DestroyExistingGUS();
    }
    #endregion

    #region Tweening
    // private void OnGamePaused(bool state) {
    //     if (state) {
    //         transform.DOPause();
    //     } else {
    //         transform.DOPlay();
    //     }
    // }
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
        this.rotation = objectVisual.transform.localRotation;
    }
    public virtual void SetWorldPosition(Vector3 worldPosition) {
        transform.position = worldPosition;
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
        if (gameObject.activeSelf) {
            StartCoroutine(QuickShowHPBarCoroutine(poi));
        }
    }
    private IEnumerator QuickShowHPBarCoroutine(IPointOfInterest poi) {
        ShowHPBar(poi);
        yield return GameUtilities.waitFor2Seconds;
        if (!(poi is Character character && character.combatComponent.isInCombat)) {
            HideHPBar();
        }
    }
    #endregion
    
    #region Graph Updates
    public void InitializeGUS(Vector2 offset, Vector2 size, [NotNull]LocationGridTile tile) {
        if (graphUpdateScene == null) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocationGridTileGUS", Vector3.zero, Quaternion.identity, transform);
            LocationGridTileGUS gus = go.GetComponent<LocationGridTileGUS>();
            graphUpdateScene = gus;
        }
        graphUpdateScene.Initialize(offset, size, tile.parentMap);
    }
    public void DestroyExistingGUS() {
        if (graphUpdateScene == null) return;
        graphUpdateScene.Destroy();
        graphUpdateScene = null;
    }
    public void ApplyGraphUpdate() {
        graphUpdateScene.InstantApply();
    }
    #endregion
}