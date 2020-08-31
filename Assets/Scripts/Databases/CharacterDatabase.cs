using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabase {
    
    public Dictionary<string, Character> allCharacters { get; }
    public Dictionary<string, Character> limboCharacters { get; }
    
    public List<Character> allCharactersList { get; }
    public List<Character> limboCharactersList { get; }

    public CharacterDatabase() {
        allCharacters = new Dictionary<string, Character>();
        limboCharacters = new Dictionary<string, Character>();
        allCharactersList = new List<Character>();
        limboCharactersList = new List<Character>();
    }

    internal void AddCharacter(Character character) {
        allCharacters.Add(character.persistentID, character);
        allCharactersList.Add(character);
    }
    internal bool RemoveCharacter(Character character) {
        allCharacters.Remove(character.persistentID);
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
        if (DatabaseManager.Instance.characterDatabase.allCharacters.TryGetValue(id, out Character character)) {
            return character;
        } else if (DatabaseManager.Instance.characterDatabase.limboCharacters.TryGetValue(id, out character)) {
            return character;
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