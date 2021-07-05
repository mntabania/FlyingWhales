using Traits;
using Interrupts;
using UnityEngine;
using Inner_Maps;

public class LycanthropeChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs from Lycanthropes";
    public override string description => "Chaos Orbs from Lycanthropes actions";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Lycanthrope_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character>(CharacterSignals.LYCANTHROPE_SHED_WOLF_PELT, OnLycanthropeShedWolfPelt);
    }

    void OnCharacterDied(Character p_character) {
        LocationGridTile gridTile = p_character.gridTileLocation;
        if (gridTile == null) {
            gridTile = p_character.deathTilePosition;
        }
        if (p_character.race.IsSapient() && gridTile != null) {
            Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
            if (responsibleCharacter != null) {
                if (responsibleCharacter.traitContainer.HasTrait("Lycanthrope")) {
#if DEBUG_LOG
                    Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [Kill by lycan] - [2]");
#endif
                    Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTile.centeredWorldLocation, 2, gridTile.parentMap);
                }
            }
        }
    }

    void OnLycanthropeShedWolfPelt(Character p_character) {
#if DEBUG_LOG
        Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [OnLycanthropeShedWolfPelt] - [2]");
#endif
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, 2, p_character.gridTileLocation.parentMap);
    }
}