using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureTemplateObjectData : MonoBehaviour {

    public TILE_OBJECT_TYPE tileObjectType;
    public SpriteRenderer spriteRenderer;

    public void SetVisualColor(Color p_color) {
        spriteRenderer.color = p_color;
    }
    public void SetSortingOrder(int p_sortingOrder) {
        spriteRenderer.sortingOrder = p_sortingOrder;
    }
}
