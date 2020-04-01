using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockWallProjectileReceiver  : ProjectileReceiver {
    public override void OnTriggerEnter2D(Collider2D collision) {
        Projectile projectileThatHit = collision.gameObject.GetComponent<Projectile>();
        if (projectileThatHit != null) { //allow all projectiles to hit walls
            projectileThatHit.OnProjectileHit(owner);
        }
    }
}
