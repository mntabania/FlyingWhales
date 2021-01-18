using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public abstract class WinconditionTracker {

    public List<Character> villagersToTrack { get; private set; }
    
    public abstract System.Type serializedData { get; }
    
    public WinconditionTracker() {
        villagersToTrack = new List<Character>();
    }

    public virtual void Initialize(List<Character> p_allCharacters) {
        villagersToTrack = new List<Character>();
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyPressed);
        List<Character> charactersToTrack = GetAllCharactersToBeEliminated(p_allCharacters);
        villagersToTrack.Clear();
        for (int i = 0; i < charactersToTrack.Count; i++) {
            Character target = charactersToTrack[i];
            if (!ShouldConsiderCharacterAsEliminated(target)) {
                villagersToTrack.Add(target);
            }
        }
    }

    #region Loading
    public virtual void LoadReferences(SaveDataWinConditionTracker data) {
        if (data.villagersToTrack != null) {
            villagersToTrack = SaveUtilities.ConvertIDListToCharacters(data.villagersToTrack);    
        }
    } 
    #endregion

    public virtual bool ShouldConsiderCharacterAsEliminated(Character character) {
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

    public List<Character> GetAllCharactersToBeEliminated(List<Character> p_allCharacters) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < p_allCharacters.Count; i++) {
            Character character = p_allCharacters[i];
            if (!character.isDead && character.isNormalCharacter && character.race.IsSapient()) {
                characters.Add(character);
            }
        }
        return characters;
    }

    public void RemoveCharacterFromTrackList(Character p_character) {
        if (villagersToTrack.Contains(p_character)) {
            villagersToTrack.Remove(p_character);
        }
    }

    public void AddCharacterToTrackList(Character p_character) {
        if (!villagersToTrack.Contains(p_character)) {
            villagersToTrack.Add(p_character);
        }
    }

    #region Listeners
    private void OnKeyPressed(KeyCode keyCode) {
        if (keyCode == KeyCode.Tab) {
            CenterCycle();
        }
    }
   
    #endregion

    #region List Maintenance
    #endregion

    #region Utilities
   
    private void CenterCycle() {
        if (villagersToTrack != null && villagersToTrack.Count > 0) {
            //normal objects to center
            ISelectable objToSelect = GetNextObjectToCenter(villagersToTrack.Select(c => c as ISelectable).ToList());
            if (objToSelect != null) {
                InputManager.Instance.Select(objToSelect);
            }
        }
    }
    private ISelectable GetNextObjectToCenter(List<ISelectable> selectables) {
        ISelectable objToSelect = null;
        for (int i = 0; i < selectables.Count; i++) {
            ISelectable currentSelectable = selectables[i];
            if (currentSelectable.IsCurrentlySelected()) {
                //set next selectable in list to be selected.
                objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
                break;
            }
        }
        if (objToSelect == null) {
            objToSelect = selectables[0];
        }
        return objToSelect;
    }
    #endregion
}

public class SaveDataWinConditionTracker : SaveData<WinconditionTracker> {
    public List<string> villagersToTrack;
    public override void Save(WinconditionTracker data) {
        base.Save(data);
        villagersToTrack = SaveUtilities.ConvertSavableListToIDs(data.villagersToTrack);
    }
}
