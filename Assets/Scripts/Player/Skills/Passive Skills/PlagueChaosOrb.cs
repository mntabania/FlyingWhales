using Traits;
using Interrupts;
using UtilityScripts;
using UnityEngine;
public class PlagueChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs from Plague";
    public override string description => "Chaos Orbs upon Acquiring plague symptom";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Plague_Chaos_Orb;

    public override void ActivateSkill() {
        //Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnTraitAdded);
        Messenger.AddListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnInterruptAdded);
        //Messenger.AddListener<Character>(PlayerSkillSignals.ON_PLAGUE_POISON_CLOUD_ACTIVATED, OnPoisonCloudActivated); remove for now
    }
    private void OnTraitAdded(Character character, Trait trait) {
        if (GameUtilities.RollChance(50)) {
            if (trait.name == "Depressed" || trait.name == "Insomnia" || trait.name == "Lethargic") { //|| trait.name == "Paralyzed"
#if DEBUG_LOG
                Debug.Log("Chaos Orb Produced - [" + character.name + "] - [Became Depressed - Insomnia - Lethargic] - [1]");
#endif
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.gridTileLocation.parentMap);
            }
        }
    }

    private void OnInterruptAdded(InterruptHolder interrupt) {
        if (GameUtilities.RollChance(50)) {
            Character character = interrupt.actor;
            if (character.faction.factionType.type != FACTION_TYPE.Demon_Cult && (interrupt.interrupt.type == INTERRUPT.Seizure || interrupt.interrupt.type == INTERRUPT.Sneeze || interrupt.interrupt.type == INTERRUPT.Puke)) {
#if DEBUG_LOG
                Debug.Log("Chaos Orb Produced - [" + character.name + "] - [Became Depressed - Insomnia - Lethargic] - [1]");
#endif
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.gridTileLocation.parentMap);
            }
        }
    }

    private void OnPoisonCloudActivated(Character p_character) {
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
    }
}