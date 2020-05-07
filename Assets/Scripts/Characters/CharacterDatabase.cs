using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabase {
    
    public Dictionary<int, Character> allCharacters { get; }
    public Dictionary<int, Character> limboCharacters { get; }
    
    public List<Character> allCharactersList { get; }
    public List<Character> limboCharactersList { get; }

    public CharacterDatabase() {
        allCharacters = new Dictionary<int, Character>();
        limboCharacters = new Dictionary<int, Character>();
        allCharactersList = new List<Character>();
        limboCharactersList = new List<Character>();
    }

    internal void AddCharacter(Character character) {
        allCharacters.Add(character.id, character);
        allCharactersList.Add(character);
    }
    internal bool RemoveCharacter(Character character) {
        allCharacters.Remove(character.id);
        return allCharactersList.Remove(character);
    }
    internal void AddLimboCharacter(Character character) {
        limboCharacters.Add(character.id, character);
        limboCharactersList.Add(character);
    }
    internal bool RemoveLimboCharacter(Character character) {
        limboCharacters.Remove(character.id);
        return limboCharactersList.Remove(character);
    }

}