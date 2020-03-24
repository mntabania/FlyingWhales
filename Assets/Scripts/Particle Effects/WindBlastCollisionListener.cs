using UnityEngine;

public class WindBlastCollisionListener : ParticleCollisionListener {
    protected override void OnParticleCollision(GameObject other) {
        // Rigidbody2D _rigidbody2D = other.GetComponent<Rigidbody2D>();
        // if (_rigidbody2D != null) {
        //     _rigidbody2D.AddForce(transform.right * 2f, ForceMode2D.Impulse);
        // }
        // if (other.CompareTag("Character Marker")) {
        //     CharacterMarker characterMarker = other.GetComponent<CharacterMarker>();
        //     characterMarker.character.AdjustHP(-20, ELEMENTAL_TYPE.Wind, transform);
        // }
    }
}
