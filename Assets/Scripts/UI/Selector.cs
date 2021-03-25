using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;

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

        Vector3 position = _selected.worldPosition;
        if (selectable is ManMadeStructure manmadeStructure) {
            if (!UtilityScripts.Utilities.IsEven(manmadeStructure.structureObj.size.x)) {
                position.x += 0.5f;
                //if (position.x >= 0) {
                //    position.x += 0.5f;
                //} else {
                //    position.x -= 0.5f;
                //}
            }
            if (!UtilityScripts.Utilities.IsEven(manmadeStructure.structureObj.size.y)) {
                position.y += 0.5f;
                //if (position.y >= 0) {
                //    position.y += 0.5f;
                //} else {
                //    position.y -= 0.5f;
                //}
            }
        }

        transform.SetPositionAndRotation(position, Quaternion.Euler(0f,0f,0f));
        
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
    public bool IsSelected(ISelectable p_selectable) {
        return _selected == p_selectable;
    }
    public void Deselect() {
        gameObject.SetActive(false);
        _selected = null;
    }
}
