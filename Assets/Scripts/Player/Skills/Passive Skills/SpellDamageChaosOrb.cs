using UnityEngine;
using UtilityScripts;
using Inner_Maps;

public class SpellDamageChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs from Spell Damage";
    public override string description => "Chaos Orbs upon spell damage";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Spell_Damage_Chaos_Orb;

    public override void ActivateSkill() {
        //Messenger.AddListener<Character, int>(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, OnSpellDamageDone);
        PlayerManager.Instance.player.damageAccumulator.SetActivatedSpellDamageChaosOrbPassiveSkill(true);
    }
    private void OnSpellDamageDone(Character character, int p_damageDone) {
        if (character == null) {
            return;
        }
        if (p_damageDone < 0) {
            p_damageDone = p_damageDone * -1;
        }
        int orbCount = (p_damageDone) / 300;
        if (orbCount > 0) {
            LocationGridTile tileLocation = character.gridTileLocation;
            if (character.isDead) {
                tileLocation = character.deathTilePosition;
            }
            if (tileLocation != null) {
#if DEBUG_LOG
                Debug.Log("Chaos Orb Produced - [" + character.name + "] - [OnSpellDamageDone] - [" + orbCount + "]");
#endif
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, tileLocation.centeredWorldLocation, orbCount, tileLocation.parentMap);
            }
        }
    }
}