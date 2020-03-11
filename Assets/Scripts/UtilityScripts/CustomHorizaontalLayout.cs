using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHorizaontalLayout : MonoBehaviour {

    [SerializeField] private RectTransform[] children;
    [SerializeField] private float spacing;

    [ExecuteInEditMode]
    private void OnEnable() {
        UpdateChildren();
    }

    [ExecuteInEditMode]
    [ContextMenu("Execute")]
    public void Execute() {
        if (children == null) {
            return;
        }
        Vector2 nextObjectPos = Vector2.zero;
        for (int i = 0; i < children.Length; i++) {
            RectTransform currChild = children[i];
            currChild.localPosition = nextObjectPos;
            nextObjectPos.x += currChild.sizeDelta.x + spacing;
        }
    }

    [ExecuteInEditMode]
    [ContextMenu("Update Children")]
    public void UpdateChildren() {
        children = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<RectTransform>(this.gameObject);
    }

}
