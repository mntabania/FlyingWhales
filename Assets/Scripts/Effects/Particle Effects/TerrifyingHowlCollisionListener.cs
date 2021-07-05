using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

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
                int baseChance = 100;
                SkillData howlData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.TERRIFYING_HOWL);
                RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.TERRIFYING_HOWL).resistanceType;
                float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(howlData);
                float resistanceValue = characterMarker.character.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
                CombatManager.ModifyValueByPiercingAndResistance(ref baseChance, piercing, resistanceValue);
                if (GameUtilities.RollChance(baseChance)) {
                    CombatManager.Instance.CreateHitEffectAt(characterMarker.character, ELEMENTAL_TYPE.Normal);
                    //int duration = TraitManager.Instance.allTraits["Spooked"].ticksDuration + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.TERRIFYING_HOWL);
                    int duration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(howlData);
                    characterMarker.character.traitContainer.AddTrait(characterMarker.character, "Spooked", overrideDuration: duration);
                    characterMarker.character.marker.AddPOIAsInVisionRange(_baseParticleEffect.targetTile.tileObjectComponent.genericTileObject);
                    characterMarker.character.combatComponent.Flight(_baseParticleEffect.targetTile.tileObjectComponent.genericTileObject, "heard a terrifying howl");
                } else {
                    characterMarker.character.reactionComponent.ResistRuinarchPower();
                }
                affectedCharacters.Add(characterMarker.character);
            }
        }
    }
}
