using UnityEngine;

public class WindBlastCollisionListener : ParticleCollisionListener {
    protected override void OnParticleCollision(GameObject other) {
        // Rigidbody2D _rigidbody2D = other.GetComponent<Rigidbody2D>();
        // if (_rigidbody2D != null) {
        //     _rigidbody2D.AddForce(transform.right * 2f, ForceMode2D.Impulse);
        // }
        // if (other.CompareTag("Character Marker")) {
        //     CharacterMarker characterMarker = other.GetComponent<CharacterMarker>();
        //     if (affectedCharacters.Contains(characterMarker.character) == false) {
        //         CombatManager.Instance.CreateHitEffectAt(characterMarker.character, ELEMENTAL_TYPE.Normal);
        //         characterMarker.character.traitContainer.AddTrait(characterMarker.character, "Spooked");
        //         characterMarker.character.marker.AddPOIAsInVisionRange(_baseParticleEffect.targetTile.genericTileObject);
        //         characterMarker.character.combatComponent.Flight(_baseParticleEffect.targetTile.genericTileObject, "heard a terrifying howl");
        //         affectedCharacters.Add(characterMarker.character);
        //     }
        // }
    }
}
