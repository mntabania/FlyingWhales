using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrifyingHowlCollisionListener : ParticleCollisionListener {

    //list of characters that have been affected by this terrifying howl.
    //This is so that characters that have already been affected, will not be affected again.
    private HashSet<Character> affectedCharacters; 
    private void OnEnable() {
        affectedCharacters = new HashSet<Character>();
    }
    private void OnDisable() {
        affectedCharacters = null;
    }
    protected override void OnParticleCollision(GameObject other) {
        if (other.CompareTag("Character Marker")) {
            CharacterMarker characterMarker = other.GetComponent<CharacterMarker>();
            //do not allow minions to be affected by terrifying howl.
            if (affectedCharacters.Contains(characterMarker.character) == false && characterMarker.character.minion == null) {
                CombatManager.Instance.CreateHitEffectAt(characterMarker.character, ELEMENTAL_TYPE.Normal);
                characterMarker.character.traitContainer.AddTrait(characterMarker.character, "Spooked");
                characterMarker.character.marker.AddPOIAsInVisionRange(_baseParticleEffect.targetTile.genericTileObject);
                characterMarker.character.combatComponent.Flight(_baseParticleEffect.targetTile.genericTileObject, "heard a terrifying howl");
                affectedCharacters.Add(characterMarker.character);
            }
        }
    }
}
