using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DefaultProjectileReceiver : ProjectileReceiver {

    public override void OnTriggerEnter2D(Collider2D collision) {
        Projectile projectileThatHit = collision.gameObject.GetComponent<Projectile>();
        if (projectileThatHit != null && (projectileThatHit.targetObject == owner || projectileThatHit.targetObject == null)) { //added checker to only register hit if the object that triggered this is the actual target of the projectile.
            projectileThatHit.OnProjectileHit(owner);
        }
    }
}
