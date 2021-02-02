﻿using System;
using System.Collections.Generic;
using UtilityScripts;
namespace Locations {
    public class LocationCharacterTracker {
        private List<Character> charactersAtLocation { get; set; }

        public LocationCharacterTracker() {
            charactersAtLocation = new List<Character>();
        }

        public void AddCharacterAtLocation(Character p_character) {
            charactersAtLocation.Add(p_character);
        }
        public void RemoveCharacterFromLocation(Character p_character) {
            charactersAtLocation.Remove(p_character);
        }

        #region Utilities
        public List<T> GetAllCharactersInsideHex<T>() where T : Character {
            List<T> characters = null;
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character character = charactersAtLocation[i];
                if (character.gridTileLocation == null) { continue; }
                if (character is T converted) {
                    if (characters == null) { characters = new List<T>(); }
                    characters.Add(converted);
                }
            }
            return characters;
        }
        public void PopulateCharacterListInsideHexThatMeetCriteria(List<Character> p_characterList, Func<Character, bool> validityChecker) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character character = charactersAtLocation[i];
                if (character.gridTileLocation == null) { continue; }
                if (character.isBeingSeized) { continue; }
                if (validityChecker.Invoke(character)) {
                    p_characterList.Add(character);
                }
            }
        }
        public T GetFirstCharacterInsideHexThatMeetCriteria<T>(Func<Character, bool> validityChecker) where T : Character {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character character = charactersAtLocation[i];
                if (character.gridTileLocation == null) {
                    continue; //skip this character
                }
                if (validityChecker.Invoke(character)) {
                    if (character is T converted) {
                        return converted;
                    }
                }
                
            }
            return null;
        }
        public T GetRandomCharacterInsideHexThatMeetCriteria<T>(Func<Character, bool> validityChecker) where T : Character {
            List<T> characters = null;

            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character character = charactersAtLocation[i];
                if (character.gridTileLocation == null) {
                    continue; //skip this character
                }
                if (validityChecker.Invoke(character)) {
                    if (character is T converted) {
                        if (characters == null) { characters = new List<T>(); }
                        characters.Add(converted);
                    }
                }
                
            }
            if (characters != null && characters.Count > 0) {
                return CollectionUtilities.GetRandomElement(characters);
            }
            return null;
        }
        public int GetNumOfCharactersInsideHexThatMeetCriteria(Func<Character, bool> criteria) {
            int count = 0;
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character character = charactersAtLocation[i];
                if (character.gridTileLocation == null) {
                    continue; //skip this character
                }
                if (criteria.Invoke(character)) {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region Testing
        public string GetCharactersSummary() {
            return charactersAtLocation.ComafyList();
        }
        #endregion
    }
}