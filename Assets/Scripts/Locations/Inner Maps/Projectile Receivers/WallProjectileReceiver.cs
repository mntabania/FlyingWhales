using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallProjectileReceiver : ProjectileReceiver {
    private void Awake() {
        this.gameObject.tag = "Structure_Wall";
        if (_collider == null) {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }
    }
    public override void OnTriggerEnter2D(Collider2D collision) {
        Projectile projectileThatHit = collision.gameObject.GetComponent<Projectile>();
        if (projectileThatHit != null) { //allow all projectiles to hit walls
            projectileThatHit.OnProjectileHit(owner);
        }
    }
}
