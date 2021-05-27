using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabase {
    
    public Dictionary<string, Character> allCharacters { get; }
    public Dictionary<string, Character> limboCharacters { get; }
    
    public List<Character> allCharactersList { get; }
    public List<Character> limboCharactersList { get; }
    public List<Character> aliveVillagersList { get; }

    public CharacterDatabase() {
        allCharacters = new Dictionary<string, Character>();
        limboCharacters = new Dictionary<string, Character>();
        allCharactersList = new List<Character>();
        limboCharactersList = new List<Character>();
        aliveVillagersList = new List<Character>();
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOMES_MINION_OR_SUMMON, OnCharacterBecameSummonOrMinion);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, OnCharacterNoLongerSummonOrMinion);
    }

    #region Villagers List
    private void OnCharacterNoLongerSummonOrMinion(Character p_character) {
        if (IsConsideredVillager(p_character)) {
            AddToAliveVillagersList(p_character);
        }
    }
    private void OnCharacterDied(Character p_character) {
        RemoveFromAliveVillagersList(p_character);
    }
    private void OnCharacterBecameSummonOrMinion(Character p_character) {
        RemoveFromAliveVillagersList(p_character);
    }
    private void OnCharacterChangedClass(Character p_character, CharacterClass p_previousClass, CharacterClass p_newClass) {
        if (IsConsideredVillager(p_character)) {
            AddToAliveVillagersList(p_character);
        } else {
            RemoveFromAliveVillagersList(p_character);
        }
    }
    private void RemoveFromAliveVillagersList(Character p_character) {
        if (aliveVillagersList.Remove(p_character)) {
#if DEBUG_LOG
            Debug.Log($"Removed {p_character.name} from alive villagers list. All alive villagers are {aliveVillagersList.ComafyList()}");
#endif
        }
    }
    private void AddToAliveVillagersList(Character p_character) {
        if (!aliveVillagersList.Contains(p_character)) {
            aliveVillagersList.Add(p_character);
#if DEBUG_LOG
            Debug.Log($"Added {p_character.name} to alive villagers list. All alive villagers are {aliveVillagersList.ComafyList()}");
#endif
        }
    }
    private bool IsConsideredVillager(Character p_character) {
        return p_character.isNormalCharacter && p_character.race != RACE.RATMAN;
    }
#endregion
    
    internal void AddCharacter(Character character, bool addToAliveVillagersList = true) {
        allCharacters.Add(character.persistentID, character);
        allCharactersList.Add(character);
        if (addToAliveVillagersList && character.isNormalCharacter && !character.isDead) {
            AddToAliveVillagersList(character);    
        }
    }
    internal bool RemoveCharacter(Character character, bool removeFromAliveVillagersList = true) {
        allCharacters.Remove(character.persistentID);
        if (removeFromAliveVillagersList) {
            RemoveFromAliveVillagersList(character);    
        }
        return allCharactersList.Remove(character);
    }
    internal void AddLimboCharacter(Character character) {
        limboCharacters.Add(character.persistentID, character);
        limboCharactersList.Add(character);
    }
    internal bool RemoveLimboCharacter(Character character) {
        limboCharacters.Remove(character.persistentID);
        return limboCharactersList.Remove(character);
    }
    internal Character GetCharacterByPersistentID(string id) {
        if (id != null) {
            if (DatabaseManager.Instance.characterDatabase.allCharacters.TryGetValue(id, out Character character)) {
                return character;
            } else if (DatabaseManager.Instance.characterDatabase.limboCharacters.TryGetValue(id, out character)) {
                return character;
            }
        }
        return null;
    }
    internal Character GetCharacterByID(int id) {
        for (int i = 0; i < allCharactersList.Count; i++) {
            Character currCharacter = allCharactersList[i];
            if(currCharacter.id == id) {
                return currCharacter;
            }
        }
        for (int i = 0; i < limboCharactersList.Count; i++) {
            Character currCharacter = limboCharactersList[i];
            if (currCharacter.id == id) {
                return currCharacter;
            }
        }
        return null;
    }

    public void OnDestroy() {
        if (allCharactersList != null) {
            List<Character> allCharacterTemp = new List<Character>(allCharactersList);
            for (int i = 0; i < allCharacterTemp.Count; i++) {
                Character character = allCharacterTemp[i];
                character?.CleanUp();
            }
            allCharactersList?.Clear();
            allCharacterTemp?.Clear();
        }
        limboCharacters?.Clear();
        allCharacters?.Clear();
        limboCharactersList?.Clear();
    }
}