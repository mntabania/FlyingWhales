using UnityEngine;
using UtilityScripts;

public class SpellDamageChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs upon spell damage";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Spell_Damage_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character, int>(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, OnSpellDamageDone);
    }
    private void OnSpellDamageDone(Character character, int p_damageDone) {
        if (p_damageDone < 0) {
            p_damageDone = p_damageDone * -1;
        }
        int orbCount = (p_damageDone) / 300;
        if (orbCount > 0) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, orbCount, character.gridTileLocation.parentMap, CURRENCY.Chaotic_Energy);
        }
    }
}