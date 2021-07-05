using UnityEngine;
using UtilityScripts;

public class PrayerChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs from Praying Cultists";
    public override string description => "Mana Orbs on Praying Cultist";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Prayer_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_PRAY_SUCCESS, OnSuccessPraying);
    }
    private void OnSuccessPraying(Character character) {
        int orbsCount = GameUtilities.RandomBetweenTwoNumbers(1, 2);
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, orbsCount, character.gridTileLocation.parentMap);
#if DEBUG_LOG
        Debug.Log("Chaos Orb Produced - [" + character.name + "] - [OnSuccessPraying] - [" + orbsCount + "]");
#endif
    }
}