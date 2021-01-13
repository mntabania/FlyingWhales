using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;
using Traits;

public class IcalawaWinConditionTracker : WinconditionTracker {

    private System.Action<Character> _characterEliminatedAction;
    private System.Action<Character> _characterAddedAsTargetAction;

    #region IListener
    public interface Listener {
        void OnCharacterEliminated(Character p_character);
        void OnCharacterAddedAsTarget(Character p_character);
    }
    #endregion

    public Character psychoPath { set; get; }

    public List<Character> villagersToEliminate { get; private set; }
    public int totalCharactersToEliminate { get; private set; }

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);

        villagersToEliminate = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharactertraitGain);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);

        List<Character> charactersToTrack = GetQuestCharacters(p_allCharacters);
        villagersToEliminate.Clear();
        for (int i = 0; i < charactersToTrack.Count; i++) {
            AddVillagerToEliminate(charactersToTrack[i]);
        }
        totalCharactersToEliminate = 5;
    }

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (p_character != psychoPath) {
            if (villagersToEliminate.Remove(p_character)) {
                if (p_character.causeOfDeath == INTERACTION_TYPE.RITUAL_KILLING) {
                    totalCharactersToEliminate--;
                }
            }
        }
        _characterEliminatedAction?.Invoke(p_character);
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character)) {
            villagersToEliminate.Add(p_character);
            AddCharacterToTrackList(p_character);
            _characterAddedAsTargetAction?.Invoke(p_character);
        }
    }
    #endregion

    public List<Character> GetQuestCharacters(List<Character> p_allCharacters) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < p_allCharacters.Count; i++) {
            Character character = p_allCharacters[i];
            if (character.isNormalCharacter && character.race.IsSapient() && character.traitContainer.HasTrait("Blessed")) {
                characters.Add(character);
            } else {
                psychoPath = character;
            }
        }
        return characters;
    }

    private void OnCharactertraitGain(Character p_character, Trait p_trait) {
        if (p_character == psychoPath) {
            if (psychoPath.traitContainer.HasTrait("Blessed")) { 
                //TODO Issue lose condition
            }
        }
    }

    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            EliminateVillager(p_character);
            RemoveCharacterFromTrackList(p_character);
        }
    }
    private void OnNewVillagerArrived(Character newVillager) {
        AddVillagerToEliminate(newVillager);
    }

    public void Subscribe(IcalawaWinConditionTracker.Listener p_listener) {
        _characterEliminatedAction += p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction += p_listener.OnCharacterAddedAsTarget;
    }
    public void Unsubscribe(IcalawaWinConditionTracker.Listener p_listener) {
        _characterEliminatedAction -= p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction -= p_listener.OnCharacterAddedAsTarget;
    }
}