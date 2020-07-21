using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering.Universal;

public class WallVisual : MapObjectVisual<StructureWallObject> {

    private SpriteRenderer[] _spriteRenderers;

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
        visionTrigger.gameObject.SetActive(false);
    }

    public override void Initialize(StructureWallObject obj) {
        visionTrigger.Initialize(obj);
        visionTrigger.gameObject.SetActive(true);
        UpdateWallAssets(obj);
    }
    /// <summary>
    /// Update wall assets based on the structure wall object.
    /// This considers the objects resource as well as if it is damaged or not.
    /// </summary>
    /// <param name="structureWallObject">The structure wall object.</param>
    public void UpdateWallAssets(StructureWallObject structureWallObject) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            //update the sprite given the wall objects material, and if it is damaged or not.
            Assert.IsFalse(structureWallObject.madeOf == RESOURCE.FOOD, $"{structureWallObject.name} has food as it's wall!");
            WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(structureWallObject.madeOf, spriteRenderer.sprite.name);
            if (structureWallObject.currentHP == structureWallObject.maxHP) {
                spriteRenderer.sprite = wallAsset.undamaged;
            } else {
                spriteRenderer.sprite = wallAsset.damaged;
            }
            
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
    public void UpdateWallState(StructureWallObject structureWallObject) {
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

    public override void Reset() {
        base.Reset();
        visionTrigger.gameObject.SetActive(false);
    }

    public override void UpdateTileObjectVisual(StructureWallObject obj) { }
    public virtual void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public virtual bool IsMapObjectMenuVisible() {
        return true; //always true so that this is skipped
    }
}
