using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class EliminateVillagerTracker {

    #region IListener
    public interface IListener {
        void OnCharacterEliminated(Character p_character);
        void OnCharacterAddedAsTarget(Character p_character);
    }
    #endregion
    
    public int totalCharactersToEliminate { get; private set; }
    public List<Character> villagersToEliminate { get; private set; }
    
    private System.Action<Character> _characterEliminatedAction;
    private System.Action<Character> _characterAddedAsTargetAction;

    public EliminateVillagerTracker() {
        villagersToEliminate = new List<Character>();
    }

    public void Initialize(List<Character> p_allCharacters) {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
        // Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyPressed);
        
        List<Character> charactersToTrack = GetAllCharactersToBeEliminated(p_allCharacters);
        villagersToEliminate.Clear();
        for (int i = 0; i < charactersToTrack.Count; i++) {
            AddVillagerToEliminate(charactersToTrack[i]);
        }
        totalCharactersToEliminate = villagersToEliminate.Count;
        for (int i = 0; i < charactersToTrack.Count; i++) {
            Character target = charactersToTrack[i];
            if (ShouldConsiderCharacterAsEliminated(target)) {
                EliminateVillager(target);
            }
        }
    }

    private List<Character> GetAllCharactersToBeEliminated(List<Character> p_allCharacters) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < p_allCharacters.Count; i++) {
            Character character = p_allCharacters[i];
            if (!character.isDead && character.isNormalCharacter && character.race.IsSapient()) { 
                characters.Add(character);
            }
        }
        return characters;
    }

    public void Subscribe(IListener p_listener) {
        _characterEliminatedAction += p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction += p_listener.OnCharacterAddedAsTarget;
    }
    public void Unsubscribe(IListener p_listener) {
        _characterEliminatedAction -= p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction -= p_listener.OnCharacterAddedAsTarget;
    }
    
    #region Listeners
    // private void OnKeyPressed(KeyCode keyCode) {
    //     if (keyCode == KeyCode.Tab) {
    //         CenterCycle();
    //     }
    // }
    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            EliminateVillager(p_character);
        }
    }
    private void OnNewVillagerArrived(Character newVillager) {
        AddVillagerToEliminate(newVillager);
    }
    private void OnCharacterNoLongerCultist(Character p_character) {
        AddVillagerToEliminate(p_character);
    }
    #endregion

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (villagersToEliminate.Remove(p_character)) {
            totalCharactersToEliminate--;
            _characterEliminatedAction?.Invoke(p_character);
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character)) {
            villagersToEliminate.Add(p_character);
            totalCharactersToEliminate++;
            _characterAddedAsTargetAction?.Invoke(p_character);
        }
    }
    #endregion

    #region Utilities
    public static bool ShouldConsiderCharacterAsEliminated(Character character) {
        if (character.isDead) {
            return true;
        }
        if (character.traitContainer.HasTrait("Cultist")) {
            return true;
        }
        if (character.faction != null) {
            if (!character.faction.isMajorNonPlayerOrVagrant && character.faction.factionType.type != FACTION_TYPE.Ratmen) {
                return true;
            }
        }
        return false;
    }
    // private void CenterCycle() {
    //     if (villagersToEliminate != null && villagersToEliminate.Count > 0) {
    //         //normal objects to center
    //         ISelectable objToSelect = GetNextObjectToCenter(villagersToEliminate.Select(c => c as ISelectable).ToList());
    //         if (objToSelect != null) {
    //             InputManager.Instance.Select(objToSelect);
    //         }    
    //     }
    // }
    // private ISelectable GetNextObjectToCenter(List<ISelectable> selectables) {
    //     ISelectable objToSelect = null;
    //     for (int i = 0; i < selectables.Count; i++) {
    //         ISelectable currentSelectable = selectables[i];
    //         if (currentSelectable.IsCurrentlySelected()) {
    //             //set next selectable in list to be selected.
    //             objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
    //             break;
    //         }
    //     }
    //     if (objToSelect == null) {
    //         objToSelect = selectables[0];
    //     }
    //     return objToSelect;
    // }
    #endregion
}
