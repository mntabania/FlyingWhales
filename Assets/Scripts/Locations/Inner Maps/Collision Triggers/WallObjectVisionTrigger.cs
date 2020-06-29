using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObjectVisionTrigger : BaseVisionTrigger {
    public override void Initialize(IDamageable damageable) {
        if (_mainCollider == null) {
            BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
            boxCollider2D.size = new Vector2(0.7f, 0.7f);
            _mainCollider = boxCollider2D;
        }
        base.Initialize(damageable);
        VoteToMakeInvisibleToCharacters(); //Structure walls, by default, cannot be seen by characters.
    }
}
