using Inner_Maps;
using Traits;
using Interrupts;
using UnityEngine.Assertions;
using UnityEngine;

public class NightCreatureChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs from Night Creatures";
    public override string description => "Chaos Orbs from Night Creature actions";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Night_Creature_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECAME_VAMPIRE, OnCharacterBecameVampire);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Summon>(CharacterSignals.ON_CHARACTER_RAISE_DEAD_BY_NECRO, OnCharacterRaiseDeadByNecro);
    }

    void OnCharacterRaiseDeadByNecro(Summon p_character) {
        if (CharacterManager.Instance.necromancerInTheWorld != null && CharacterManager.Instance.necromancerInTheWorld.faction.factionType.type == FACTION_TYPE.Undead) {
            LocationGridTile chaosOrbSpawnLocation = !p_character.hasMarker ? p_character.deathTilePosition : p_character.gridTileLocation;
#if DEBUG_LOG
            Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [OnCharacterRaiseDeadByNecro] - [1]");
#endif
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, chaosOrbSpawnLocation.centeredWorldLocation, 1, chaosOrbSpawnLocation.parentMap);
        }
        
    }
    void OnCharacterBecameVampire(Character p_character) {
#if DEBUG_LOG
        Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [OnCharacterBecameVampire] - [3]");
#endif
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, 3, p_character.gridTileLocation.parentMap);
    }

    void OnCharacterDied(Character p_character) {
        if (p_character.race.IsSapient()) {
            Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
            if (responsibleCharacter != null) {
                if ((responsibleCharacter.race == RACE.SKELETON && responsibleCharacter.faction.factionType.type == FACTION_TYPE.Undead && CharacterManager.Instance.necromancerInTheWorld != null && CharacterManager.Instance.necromancerInTheWorld.faction == responsibleCharacter.faction) || responsibleCharacter.traitContainer.HasTrait("Necromancer") || responsibleCharacter.traitContainer.HasTrait("Vampire")) {
                    int orbsCount = 2;
                    LocationGridTile chaosOrbSpawnLocation = !responsibleCharacter.hasMarker ? responsibleCharacter.deathTilePosition : responsibleCharacter.gridTileLocation;
                    Assert.IsNotNull(chaosOrbSpawnLocation, $"Chaos orb spawn location of {responsibleCharacter.name} is null. Character that died is {p_character.name}");
                    Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, chaosOrbSpawnLocation.centeredWorldLocation, orbsCount, chaosOrbSpawnLocation.parentMap);
#if DEBUG_LOG
                    Debug.Log("Chaos Orb Produced - [" + responsibleCharacter.name + "] - [Killed by Skeleton(with 1 necro) or killed by necro or killed by vampire] - [2]");
#endif
                }
            }
        }
    }
}