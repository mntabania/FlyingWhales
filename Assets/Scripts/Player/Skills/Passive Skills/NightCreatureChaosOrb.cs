using Traits;
using Interrupts;

public class NightCreatureChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs from Night Creatures";
    public override string description => "Chaos Orbs upon Acquiring plague symptom";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Night_Creature_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECAME_VAMPIRE, OnCharacterBecameVampire);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
    }

    void OnCharacterBecameVampire(Character p_character) {
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, 3, p_character.gridTileLocation.parentMap);
    }

    void OnCharacterDied(Character p_character) {
        if (p_character.race.IsSapient()) {
            int orbsCount = 0;
            Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
            if (responsibleCharacter != null) {
                if (responsibleCharacter.race == RACE.SKELETON || responsibleCharacter.traitContainer.HasTrait("Necromancer") || responsibleCharacter.traitContainer.HasTrait("Vampire")) {
                    orbsCount = 2;
                    Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, orbsCount, p_character.gridTileLocation.parentMap);
                }
            }
        }
    }
}