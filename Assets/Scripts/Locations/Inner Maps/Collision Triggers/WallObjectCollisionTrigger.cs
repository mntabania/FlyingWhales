using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObjectCollisionTrigger : BaseCollisionTrigger<StructureWallObject> {
    
    void Awake() {
        mainCollider = gameObject.AddComponent<BoxCollider2D>();
        gameObject.layer = LayerMask.NameToLayer("All Vision Collision");
        (mainCollider as BoxCollider2D).size = new Vector2(0.7f, 0.7f);
    }

}
