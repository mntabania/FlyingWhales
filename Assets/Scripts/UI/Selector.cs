using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;

public class Selector : MonoBehaviour {

    public static Selector Instance;
    
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float selectionTiming;
    [SerializeField] private Vector2 from;
    [SerializeField] private Vector2 to;

    private ISelectable _selected;
    
    private void Awake() {
        Instance = this;
        gameObject.SetActive(false);
    }
    public void Select(ISelectable selectable, Transform parent = null) {
        gameObject.SetActive(true);
        _selected = selectable;
        this.transform.position = _selected.worldPosition;
        this.transform.localRotation = Quaternion.identity;
        
        Vector2 fromSize = from;
        fromSize.x *= selectable.selectableSize.x;
        fromSize.y *= selectable.selectableSize.y;
        _spriteRenderer.size = fromSize;

        Vector2 toSize = to;
        toSize.x *= selectable.selectableSize.x;
        toSize.y *= selectable.selectableSize.y;

        this.transform.SetParent(parent != null ? parent : InnerMapManager.Instance.transform);

        DOTween.To(() => _spriteRenderer.size, x => _spriteRenderer.size = x, toSize, selectionTiming);
    }
    public void Deselect() {
        gameObject.SetActive(false);
        _selected = null;
    }
}
