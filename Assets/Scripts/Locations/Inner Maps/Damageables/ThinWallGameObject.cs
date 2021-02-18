using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering.Universal;

public class ThinWallGameObject : TileObjectGameObject {

    private SpriteRenderer[] _spriteRenderers;
    private BoxCollider2D _unpassableCollider;
    
    public SpriteRenderer[] spriteRenderers {
        get {
            if (_spriteRenderers == null) {
                _spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(); 
            }
            return _spriteRenderers;
        }
    }
    private void Awake() {
        _spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>();
        visionTrigger = transform.GetComponentInChildren<WallObjectVisionTrigger>();
        _unpassableCollider = objectVisual.transform.GetComponentInChildren<BoxCollider2D>();
        particleEffectParent = objectVisual.transform;
        visionTrigger.gameObject.SetActive(false);
        // if (IsOffsetDefault(_unpassableCollider.offset)) {
        //     Vector2 offset = _unpassableCollider.offset;
        //     // if (name.Contains("Left")) {
        //     //     offset.x += 0.12f;
        //     // }
        //     // else if (name.Contains("Right")) {
        //     //     offset.x -= 0.12f;
        //     // } else 
        //     // if (name.Contains("Bottom")) {
        //     //     offset.y += 0.12f;
        //     // }
        //     // else 
        //     if (name.Contains("Top")) {
        //         offset.y -= 0.12f;
        //     }
        //     _unpassableCollider.offset = offset;    
        // }
    }
    private bool IsOffsetDefault(Vector2 p_offset) {
        return p_offset == Vector2.zero;
    }

    //public override void Initialize(TileObject tileObject) {
    //    visionTrigger.Initialize(tileObject);
    //    visionTrigger.gameObject.SetActive(true);
    //    UpdateWallAssets(tileObject as ThinWall);
    //}
    //public override void Initialize(ThinWall obj) {
    //    visionTrigger.Initialize(obj);
    //    visionTrigger.gameObject.SetActive(true);
    //    UpdateWallAssets(obj);
    //}

    /// <summary>
    /// Update wall assets based on the structure wall object.
    /// This considers the objects resource as well as if it is damaged or not.
    /// </summary>
    /// <param name="structureWallObject">The structure wall object.</param>
    public void UpdateWallAssets(ThinWall structureWallObject) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            //update the sprite given the wall objects material, and if it is damaged or not.
            Assert.IsFalse(structureWallObject.madeOf == RESOURCE.FOOD, $"{structureWallObject.name} has food as it's wall!");
            string orientation;
            if (spriteRenderer.sprite.name.Contains("vertical")) {
                orientation = "vertical";
            } else if (spriteRenderer.sprite.name.Contains("horizontal")) {
                orientation = "horizontal";
            } else {
                orientation = "corner";
            }
            WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(structureWallObject.madeOf, orientation);
            if (structureWallObject.currentHP == structureWallObject.maxHP) {
                spriteRenderer.sprite = wallAsset.undamaged;
            } else {
                spriteRenderer.sprite = wallAsset.damaged;
            }
        }
    }
    public void ResetWallAssets(RESOURCE resource) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            string orientation;
            if (spriteRenderer.sprite.name.Contains("vertical")) {
                orientation = "vertical";
            } else if (spriteRenderer.sprite.name.Contains("horizontal")) {
                orientation = "horizontal";
            } else {
                orientation = "corner";
            }
            WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(resource, orientation);
            spriteRenderer.sprite = wallAsset.undamaged;
        }
    }
    /// <summary>
    /// Update wall asset based only on the resource that the wall is made of.
    /// </summary>
    /// <param name="madeOf">The resource this wall is made of</param>
    public void UpdateWallAssets(RESOURCE madeOf) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(madeOf, spriteRenderer.sprite.name);
            spriteRenderer.sprite = wallAsset.undamaged;
        }
    }
    public void UpdateSortingOrders(int sortingOrder) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            if (spriteRenderer.sprite.name.Contains("corner")) {
                //all corners should be 1 layer above walls
                spriteRenderer.sortingOrder = sortingOrder + 1;
            } else {
                spriteRenderer.sortingOrder = sortingOrder;
            }

        }
    }
    public void UpdateWallState(ThinWall structureWallObject) {
        if (structureWallObject.currentHP == 0) {
            //wall is destroyed disable gameobject
            this.gameObject.SetActive(false);
        } else {
            //wall is not destroyed enable game object
            this.gameObject.SetActive(true);
        }
    }
    public void SetWallColor(Color color) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            spriteRenderer.color = color;
        }
    }
    public void SetUnpassableColliderState(bool state) {
        _unpassableCollider.enabled = state;
    }

    public override void Reset() {
        base.Reset();
        _unpassableCollider.enabled = true;
        visionTrigger.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public override void UpdateTileObjectVisual(TileObject obj) { }
}
