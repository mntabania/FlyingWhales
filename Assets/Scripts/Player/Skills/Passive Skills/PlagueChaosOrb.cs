using Traits;
using Interrupts;

public class PlagueChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs upon Acquiring plague symptom";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Plague_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnTraitAdded);
        Messenger.AddListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnInterruptAdded);
    }
    private void OnTraitAdded(Character character, Trait trait) {
        if (character.traitContainer.HasTrait("Depressed") ||
            character.traitContainer.HasTrait("Insomnia") ||
            character.traitContainer.HasTrait("Lethargic") ||
            character.traitContainer.HasTrait("Paralyzed")) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, UnityEngine.Random.Range(1, 3), character.gridTileLocation.parentMap, CURRENCY.Chaotic_Energy);
        }
    }

    private void OnInterruptAdded(InterruptHolder interrupt) {
        Character character = interrupt.actor;
        if (character.faction.factionType.type != FACTION_TYPE.Demon_Cult && (interrupt.interrupt.type == INTERRUPT.Seizure || interrupt.interrupt.type == INTERRUPT.Sneeze || interrupt.interrupt.type == INTERRUPT.Puke)) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, UnityEngine.Random.Range(1, 3), character.gridTileLocation.parentMap, CURRENCY.Chaotic_Energy);
        }
    }
}